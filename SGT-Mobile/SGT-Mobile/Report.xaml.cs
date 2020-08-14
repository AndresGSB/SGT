using iText.Forms;
using iText.Forms.Fields;
using iText.Kernel.Pdf;
using iText.Layout;
using Newtonsoft.Json;
using SGT_Mobile;
using SGTMobile.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SGTMobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Report : ContentPage
    {
        private Tickets ticket;
        private RecursoIngeniero ingeniero;
        private String controller = "tickets";
        bool hasSign = false;
        public List<FormDinamico> lista_form = new List<FormDinamico>();
        public RequestService HttpClientInstance = new RequestService();
        public bool isConnected = true;

        public Report(Tickets tk)
        {
            ticket = tk;
            InitializeComponent();
            
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();

            if (Connectivity.NetworkAccess == NetworkAccess.None)
            {
                isConnected = false;
            }

            if (!isConnected)
            {
                isConnected = false;
            }

            MessagingCenter.Subscribe<App>(this, "connect", (sender) =>
            {
                isConnected = true;
                btnGuardarEnviar.IsEnabled = true;
            });

            MessagingCenter.Subscribe<App>(this, "no_connect", (sender) =>
            {
                DisplayAlert("Warning: No Internet Connection", "You can only save your report ", "Ok");
                btnGuardarEnviar.IsEnabled = false;
                isConnected = false;

            });

            if (Application.Current.Properties.ContainsKey("token") && Application.Current.Properties["token"] != null)
            {
                this.ingeniero = JsonConvert.DeserializeObject<RecursoIngeniero>(Application.Current.Properties["token"] as string);
                downloadReport(new DocumentRequest { Account = ticket.Account, Client = ticket.Client });

            }
            else
            {
                App.Current.MainPage = new NavigationPage(new Login());
            }

        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            unSubscribe();
        }

        private async void downloadReport(DocumentRequest doc)
        {
            try
            {
                var content = JsonConvert.SerializeObject(doc);
                var buffer = System.Text.Encoding.UTF8.GetBytes(content);
                var byteContent = new ByteArrayContent(buffer);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var fileName = getNameFile();
                //validar si existe el reporte
                var pdf_filename = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), fileName + " (Filled).pdf");
                var pdf_filename2 = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), fileName + " (Blank).pdf");

                if (!File.Exists(pdf_filename) || !File.Exists(pdf_filename2))
                {
                    var resultado = await HttpClientInstance.PostAsyncJSON(controller + "/getReportTemplate", byteContent);
                    switch (resultado.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            var docPDF = resultado.Content;
                            var path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), fileName + " (Blank).pdf");
                            using (var file = System.IO.File.Create(path))
                            {
                                var contentStream = await docPDF.ReadAsStreamAsync();
                                await contentStream.CopyToAsync(file);
                                file.Close();
                                loadForm();
                            }
                            break;

                        case HttpStatusCode.Unauthorized:
                        case HttpStatusCode.InternalServerError:
                            await DisplayAlert("Warning", "Your session is expired", "Login");
                            Application.Current.Properties["token"] = null;
                            App.Current.MainPage = new Login();
                            break;

                        case HttpStatusCode.BadRequest:
                            await DisplayAlert("Warning", "Error Downloading the Report ", "Ok");
                            await Navigation.PopAsync();
                            break;
                    }
                }
                else
                {
                    loadForm();
                }
            }
            catch(Exception ex){
                await DisplayAlert("Error downloading the report", "Try it again", "Ok");
                await Navigation.PopAsync();
            }
            finally
            {

            }  
        }

        private async void loadForm()
        {
            try
            {
                var fileName = getNameFile();

                var pdf_filename_blank = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), fileName + " (Blank).pdf");

                var pdf_filename_fill = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), fileName + " (Filled).pdf");

                string pdf_filename = "";
                if (File.Exists(pdf_filename_fill))
                    pdf_filename = pdf_filename_fill;
                else
                    pdf_filename = pdf_filename_blank;

                bool doesExist = File.Exists(pdf_filename);
                if (doesExist)
                {
                    using (PdfReader reader = new PdfReader(pdf_filename))
                    {

                        using (PdfDocument pdf = new PdfDocument(reader))
                        {
                            PdfAcroForm formpdf = PdfAcroForm.GetAcroForm(pdf, true);
                            var fields = formpdf.GetFormFields().Keys;
                            foreach (var f in fields)
                            {
                                var field = formpdf.GetField(f).GetFormType();
                                if (0 == PdfName.Tx.CompareTo(field) && !f.Contains(".1"))
                                {
                                    var types = getTypeForm(f);
                                    var radios = this.lista_form.Where(x => x.tipo == TypeObject.Radio && x.Nombre == types[1]).ToList().Count;
                                    Label lbl = new Label();
                                    lbl.Text = types[1];
                                    lbl.Style = (Xamarin.Forms.Style)Application.Current.Resources["lbl_formReport"];
                                    var fd = new FormDinamico { Nombre = types[1], label = lbl };
                                    if (radios == 0 && types[0] != "sign")
                                        layout_Form.Children.Add(lbl);

                                    var valor = formpdf.GetField(f).GetValue() == null ? getValueTicket(f) : formpdf.GetField(f).GetValue().ToString();
                                    
                                    switch (types[0])
                                    {
                                        case "txt":
                                            var entry = new Entry();
                                            layout_Form.Children.Add(entry);
                                            entry.Text = valor;
                                            fd.entry = entry;
                                            fd.tipo = TypeObject.Entry;
                                            break;

                                        case "date":
                                            var date = new DatePicker();
                                            date.Format = "dd/MM/yyyy";
                                            layout_Form.Children.Add(date);
                                            date.Date = valor == "" ? DateTime.Now : DateTime.Parse(valor);
                                            fd.entry = date;
                                            fd.tipo = TypeObject.Date;
                                            break;

                                        case "time":
                                            var time = new TimePicker();
                                            layout_Form.Children.Add(time);
                                            time.Time = valor == "" ? new TimeSpan() : TimeSpan.Parse(valor);
                                            time.Format = "HH:mm tt";
                                            fd.entry = time;
                                            fd.tipo = TypeObject.Time;
                                            break;

                                        case "multi":
                                            var multi = new Editor();
                                            multi.HeightRequest = 100;
                                            multi.AutoSize = EditorAutoSizeOption.TextChanges;
                                            multi.Text = valor;
                                            layout_Form.Children.Add(multi);
                                            fd.entry = multi;
                                            fd.tipo = TypeObject.Multi;
                                            break;

                                        case "sign":
                                            hasSign = true;
                                            break;

                                        case "radio":
                                            /*
                                            var radio = new RadioButton();
                                            radio.GroupName = f;
                                            if (!existRadio(types[1]))
                                                radio.Text = "Yes";
                                            else
                                                radio.Text = "No";
                                            layout_Form.Children.Add(radio);
                                            fd.entry = radio;
                                            fd.tipo = TypeObject.Radio;
                                            */
                                            break;
                                    }

                                    lista_form.Add(fd);
                                }
                            }
                            pdf.Close();
                        }
                    }
                    Loader.IsRunning = false;
                    Loader.IsVisible = false;
                    layout_Form.IsVisible = true;
                    options_Form.IsVisible = true;
                    if (hasSign)
                    {
                        btnGuardarFirmar.IsVisible = true;
                        btnGuardarEnviar.IsVisible = false;
                    }
                    else
                    {
                        btnGuardarFirmar.IsVisible = false;
                        btnGuardarEnviar.IsVisible = true;
                    }
                }
                else
                {
                    await DisplayAlert("Warning", "Error Downloading the Report, try again ", "Ok");
                    await Navigation.PopAsync();
                }
            }
            catch (Exception ex)
            {
                var fileName = getNameFile();
                await DisplayAlert("Error", "Error loading the report, try again", "Ok");
                var pdf_filename_fill = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), fileName + " (Filled).pdf");
                File.Delete(pdf_filename_fill);
                await Navigation.PopAsync();
            }
            finally
            {

            }
            
        }

        private string[] getTypeForm(string name)
        {
            var type = new string[2];
            var name_split = name.Split('_');
            type[0] = name_split[0];
            string FieldName = "";
            for(int i = 1; i < name_split.Length ; i++)
            {
                FieldName += name_split[i] + " ";
            }
            type[1] = FieldName;
            return type;
        }

        private string getNameFile()
        {
            string date = String.Format("{0:ddMMyyyy}", this.ticket.ServiceDate);
            return this.ticket.ClientTicket + " " + this.ticket.SiteName.NombreSitio + " " + date;
        }

        private string getValueTicket(string f)
        {
            string valor ="";
            if (f.Contains("Service_Date"))
                valor = this.ticket.ServiceDate.ToString();

            if (f.Contains("Ticket_Number"))
                valor = this.ticket.ClientTicket;

            if (f.Contains("Site_Name"))
                valor = this.ticket.SiteName.NombreSitio;

            if (f.Contains("Site_Address"))
                valor = this.ticket.SiteName.Direccion;

            if (f.Contains("POC_Name"))
                valor = this.ticket.POC_1;

            if (f.Contains("Final_User_Name"))
                valor = this.ticket.Final_User;

            if (f.Contains("FE_Name"))
                valor = this.ingeniero.Name;

            if (f.Contains("Service_Description"))
                valor = this.ticket.Service_Details;

            if (f.Contains("Client_Internal_Ticket_Number"))
                valor = this.ticket.ClientIntertal;

            return valor;
        }

        private bool existRadio(string name)
        {
            var radiosButton = this.lista_form.Where(x => x.tipo == TypeObject.Radio && x.Nombre == name).ToList();
            if (radiosButton.Count > 0 )
            {
                return true;
            }
            return false;
        }

        private async void btnGuardar_Clicked(object sender, EventArgs e)
        {
            loader.IsVisible = true;
            GuardarReporte();
            loader.IsVisible = false;
            string action = await DisplayActionSheet("Report Saved", "Close", null, "View PDF");
            if (action == "View PDF")
                await Navigation.PushAsync(new PreviewPDF(this.ticket));
        }

        private async void btnGuardarFirmar_Clicked(object sender, EventArgs e)
        {
            GuardarReporte();
            await Navigation.PushAsync(new SignReport(this.ticket));
        }

        private void btnGuardarEnviar_Clicked(object sender, EventArgs e)
        {
            loader.IsVisible = true;
            GuardarReporte();
            EnviarReporte();
            loader.IsVisible = false;
        }

        private async void GuardarReporte()
        {
            try
            {
                var fileName = getNameFile();

                var pdf_filename_blank = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), fileName + " (Blank).pdf");
                var pdf_filename_fill = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), fileName + " (Filled).pdf");

                bool doesExist = File.Exists(pdf_filename_blank);
                if (doesExist)
                {
                    using (PdfReader reader = new PdfReader(pdf_filename_blank))
                    {

                        var writer = new PdfWriter(pdf_filename_fill, new WriterProperties());
                        using (PdfDocument pdf = new PdfDocument(reader, writer))
                        {
                            PdfAcroForm formpdf = PdfAcroForm.GetAcroForm(pdf, true);
                            var fields = formpdf.GetFormFields().Keys;
                            foreach (var f in fields)
                            {
                                var field = formpdf.GetField(f).GetFormType();
                                if (0 == PdfName.Tx.CompareTo(field) && !f.Contains(".1") && !f.Contains("sign"))
                                {
                                    var formDi = this.lista_form.Where(x => x.Nombre == getTypeForm(f)[1]).ToList().FirstOrDefault();
                                    string valor = "";
                                    if (formDi.tipo == TypeObject.Entry)
                                    {
                                        var obj = (Entry)formDi.entry;
                                        valor = obj.Text != null ? obj.Text : "";
                                    }
                                    if (formDi.tipo == TypeObject.Date)
                                    {
                                        var obj = (DatePicker)formDi.entry;
                                        valor = obj.Date.ToString() != null ? String.Format("{0:dd/MM/yyyy}", obj.Date) : "";
                                    }
                                    if (formDi.tipo == TypeObject.Time)
                                    {
                                        var obj = (TimePicker)formDi.entry;
                                        var time = obj.Time;
                                        var dt = DateTime.Today.Add(time);
                                        string displayTime = time.ToString("hh:mm tt");
                                        valor = !string.IsNullOrEmpty(time.ToString()) ? dt.ToString("hh:mm tt") : "";
                                    }
                                    if (formDi.tipo == TypeObject.Multi)
                                    {
                                        var obj = (Editor)formDi.entry;
                                        valor = obj.Text != null ? obj.Text : "";
                                    }
                                    formpdf.GetField(f).SetValue(valor);
                                }
                            }
                            pdf.Close();
                            reader.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Warning", "Error saving the report, try it again ", "Ok");
            }
            finally
            {

            }
            
        }

        private async void EnviarReporte()
        {
            try
            {
                var fileName = getNameFile();
                var pdf_filename_fill = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), fileName + " (Filled).pdf");
                using (var form = new MultipartFormDataContent())
                {
                    using (var fs = File.OpenRead(pdf_filename_fill))
                    {
                        using (var streamContent = new StreamContent(fs))
                        {
                            using (var fileContent = new ByteArrayContent(await streamContent.ReadAsByteArrayAsync()))
                            {
                                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                                //var content = JsonConvert.SerializeObject(this.ticket.Id + "");
                                var buffer = System.Text.Encoding.UTF8.GetBytes(this.ticket.Id + "");
                                var byteContent = new ByteArrayContent(buffer);
                                // "file" parameter name should be the same as the server side input parameter name
                                form.Add(fileContent, "file", Path.GetFileName(pdf_filename_fill));
                                form.Add(byteContent, "obj");

                                var response = await HttpClientInstance.PostAsyncForm(controller + "/sendReport", form);
                                switch (response.StatusCode)
                                {
                                    case HttpStatusCode.OK:
                                        await DisplayAlert("Success", "The report has been uploaded", "Ok");
                                        await Navigation.PopAsync();
                                        break;

                                    case HttpStatusCode.BadRequest:
                                        await DisplayAlert("Warning", "Error while sending the report, try it again ", "Ok");
                                        break;

                                    case HttpStatusCode.InternalServerError:
                                        await DisplayAlert("Warning", "Error while sending the report, try it again ", "Ok");
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception){
                await DisplayAlert("Warning", "Error while sending the report, try it again ", "Ok");
            }
            finally
            {
                
            }

        }

        private void unSubscribe()
        {
            MessagingCenter.Unsubscribe<App>(this, "connect");

            MessagingCenter.Unsubscribe<App>(this, "no_connect");
        }

    }
}