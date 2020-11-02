using iText.Forms;
using iText.Forms.Fields;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Filter;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Layout;
using Newtonsoft.Json;
using SGT_Mobile;
using SGTMobile.Models;
using SGTMobile.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        bool hasSign = false;
        bool hasFESign = false;
        public List<FormDinamico> lista_form = new List<FormDinamico>();
        public RequestService HttpClientInstance = new RequestService("tickets");
        public bool isConnected = true;
        public bool Displayit = true;

        public Report(Tickets tk)
        {
            ticket = tk;
            InitializeComponent();
        }

        #region Init
        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (Connectivity.NetworkAccess == NetworkAccess.None)
            {
                isConnected = false;
                btnGuardarEnviar.IsEnabled = false;
            }
            else
            {
                isConnected = true;
                btnGuardarEnviar.IsEnabled = true;
            }

            Subscribe();

            if (Application.Current.Properties.ContainsKey("token") && Application.Current.Properties["token"] != null)
            {
                this.ingeniero = JsonConvert.DeserializeObject<RecursoIngeniero>(Application.Current.Properties["token"] as string);
                
                if (hasBAMReport())
                {
                    DownloadReportBAM(new DocumentRequest { Account = ticket.Account, Client = ticket.Client, IdTicket = ticket.Id });
                }
                else
                {
                    DownloadReport(new DocumentRequest { Account = ticket.Account, Client = ticket.Client, IdTicket = ticket.Id });
                }
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
            this.layout_Form.Children.Clear();
        }
        #endregion

        #region Funciones
        private async void DownloadReport(DocumentRequest doc)
        {
            try
            {
                loader.IsVisible = true;
                var fileName = getNameFile();
                var pathLocal = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
                //validar si existe el reporte
                var pdf_filename = System.IO.Path.Combine(pathLocal, fileName + " (Filled).pdf");
                var pdf_filename2 = System.IO.Path.Combine(pathLocal, fileName + " (Blank).pdf");
                var pdf_filename3 = System.IO.Path.Combine(pathLocal, fileName + " (Signed).pdf");

                if (!File.Exists(pdf_filename) && !File.Exists(pdf_filename2) && !File.Exists(pdf_filename3))
                {
                    var resultado = await HttpClientInstance.PostAsyncJSON("/getReportTemplate", doc);
                    switch (resultado.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            var docPDF = resultado.Content;
                            var path = Path.Combine(pathLocal, fileName + " (Blank).pdf");
                            using (var file = System.IO.File.Create(path))
                            {
                                var contentStream = await docPDF.ReadAsStreamAsync();
                                await contentStream.CopyToAsync(file);
                                file.Close();
                                LoadForm();
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
                    LoadForm();
                }
            }
            catch (Exception)
            {
                await DisplayAlert("Error downloading the report", "Try it again", "Ok");
                await Navigation.PopAsync();
                loader.IsVisible = false;
            }
        }

        private async void DownloadReportBAM(DocumentRequest doc)
        {
            try
            {
                loader.IsVisible = true;
                var fileName = getNameFile();
                var pathLocal = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
                //validar si existe el reporte
                var pdf_filename = System.IO.Path.Combine(pathLocal, fileName + " (Filled).pdf");
                var pdf_filename2 = System.IO.Path.Combine(pathLocal, fileName + " (Blank).pdf");

                var createForm = true;
                if (!File.Exists(pdf_filename) && !File.Exists(pdf_filename2))
                {
                    var resultado = await HttpClientInstance.PostAsyncJSON("/getReportTemplateBAM", doc);
                    switch (resultado.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            var docPDF = resultado.Content;
                            var path = Path.Combine(pathLocal, fileName + " (BlankPREV).pdf");
                            using (var file = System.IO.File.Create(path))
                            {
                                var contentStream = await docPDF.ReadAsStreamAsync();
                                await contentStream.CopyToAsync(file);
                                file.Close();
                            }
                            break;

                        case HttpStatusCode.Unauthorized:
                        case HttpStatusCode.InternalServerError:
                            await DisplayAlert("Warning", "Your session is expired", "Login");
                            Application.Current.Properties["token"] = null;
                            App.Current.MainPage = new Login();
                            break;

                        case HttpStatusCode.BadRequest:
                            createForm = false;
                            this.DownloadReport(doc);
                            //await DisplayAlert("Warning", "Error Downloading the Report ", "Ok");
                            //await Navigation.PopAsync();
                            break;
                    }

                    if (createForm)
                    {
                        //crear formulario BAM
                        var pdf_filename_blank = System.IO.Path.Combine(pathLocal, fileName + " (Blank).pdf");
                        var pdf_filename_download = System.IO.Path.Combine(pathLocal, fileName + " (BlankPREV).pdf");

                        var reader = new PdfReader(pdf_filename_download);
                        var writer = new PdfWriter(pdf_filename_blank);

                        var location = new TextLocationStrategy();

                        PdfDocument pdfDoc = new PdfDocument(reader, writer);
                        FilteredEventListener listener = new FilteredEventListener();
                        var strat = listener.AttachEventListener(location);
                        PdfCanvasProcessor processor = new PdfCanvasProcessor(listener);
                        processor.ProcessPageContent(pdfDoc.GetPage(1));

                        // obtenemos el *Date lo demas lo calculamos
                        var posicion = new textChunk();
                        var palabras = location.objectResult;
                        int loop = 0;
                        foreach (var aux in palabras)
                        { 
                            if (aux.text.Contains("*"))
                            {
                                if (palabras.ElementAt(loop + 1).text.Contains("D"))
                                {
                                    posicion = aux;
                                }
                            }
                            loop++;
                        }

                        var initialX = posicion.rect.GetX();
                        var initialY = posicion.rect.GetY() - 25;

                        PdfTextFormField date_Date = PdfFormField.CreateText(pdfDoc, new iText.Kernel.Geom.Rectangle(16, initialY, 133, 15), "date_Service_Date", "");
                        PdfTextFormField time_Transit_Time = PdfFormField.CreateText(pdfDoc, new iText.Kernel.Geom.Rectangle(153, initialY, 104, 14), "time_Transit_Time", "");
                        PdfTextFormField time_Time_Arrival = PdfFormField.CreateText(pdfDoc, new iText.Kernel.Geom.Rectangle(261, initialY, 80, 15), "time_Time_Arrival", "");
                        PdfTextFormField time_Departure_Time = PdfFormField.CreateText(pdfDoc, new iText.Kernel.Geom.Rectangle(345, initialY, 99, 15), "time_Departure_Time", "");
                        PdfTextFormField txt_Lunch_Break = PdfFormField.CreateText(pdfDoc, new iText.Kernel.Geom.Rectangle(448, initialY, 110, 15), "txt_Lunch_Break", "");
                        PdfTextFormField sign_Signature = PdfFormField.CreateText(pdfDoc, new iText.Kernel.Geom.Rectangle(231, initialY -175, 135, 17), "sign_Signature", "");
                        PdfTextFormField txt_POC_Name_Sign = PdfFormField.CreateText(pdfDoc, new iText.Kernel.Geom.Rectangle(180, initialY - 175 - 22, 262, 15), "txt_POC_Name_Sign", "");
                        PdfTextFormField date_Date_Sign = PdfFormField.CreateText(pdfDoc, new iText.Kernel.Geom.Rectangle(180, initialY - 175 - 22 - 21, 263, 15), "date_Date_Sign", "");

                        PdfAcroForm.GetAcroForm(pdfDoc, true).AddField(date_Date, pdfDoc.GetFirstPage());
                        PdfAcroForm.GetAcroForm(pdfDoc, true).AddField(time_Transit_Time, pdfDoc.GetFirstPage());
                        PdfAcroForm.GetAcroForm(pdfDoc, true).AddField(time_Time_Arrival, pdfDoc.GetFirstPage());
                        PdfAcroForm.GetAcroForm(pdfDoc, true).AddField(time_Departure_Time, pdfDoc.GetFirstPage());
                        PdfAcroForm.GetAcroForm(pdfDoc, true).AddField(txt_Lunch_Break, pdfDoc.GetFirstPage());
                        PdfAcroForm.GetAcroForm(pdfDoc, true).AddField(sign_Signature, pdfDoc.GetFirstPage());
                        PdfAcroForm.GetAcroForm(pdfDoc, true).AddField(txt_POC_Name_Sign, pdfDoc.GetFirstPage());
                        PdfAcroForm.GetAcroForm(pdfDoc, true).AddField(date_Date_Sign, pdfDoc.GetFirstPage());
                        pdfDoc.Close();

                        LoadForm();
                    }
                    
                }
                else
                {
                    LoadForm();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error downloading the report", "Try it again", "Ok");
                await Navigation.PopAsync();
            }
        }

        private async void LoadForm()
        {
            try
            {
                var fileName = getNameFile();
                var pathLocal = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);

                var pdf_filename_blank = System.IO.Path.Combine(pathLocal, fileName + " (Blank).pdf");
                var pdf_filename_fill = System.IO.Path.Combine(pathLocal, fileName + " (Filled).pdf");
                var pdf_filename_signed = System.IO.Path.Combine(pathLocal, fileName + " (Signed).pdf");

                string pdf_filename = "";
                if (File.Exists(pdf_filename_fill))
                    pdf_filename = pdf_filename_fill;
                else
                    pdf_filename = pdf_filename_blank;

                bool doesExist = File.Exists(pdf_filename);
                Displayit = true;

                if (File.Exists(pdf_filename_signed))
                {
                    string action = await DisplayActionSheet("You have a signed Report, What do you want to do?", "Go to My Services", null, "Preview Report", "Send Report to GSB", "Edit it (You must signed it again)");
                    switch (action)
                    {
                        case "Go to My Services":
                            await Navigation.PopAsync();
                            Displayit = false;
                            break;

                        case "Preview Report":
                            await Navigation.PushAsync(new PreviewPDF(this.ticket));
                            Displayit = false;
                            break;

                        case "Send Report to GSB":
                            EnviarReporte("Signed");
                            Displayit = false;
                            break;

                        case "Edit it (You must signed it again)":
                            Displayit = true;
                            break;
                    }
                }

                if (doesExist && Displayit)
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
                                            date.Date = valor == "" ? DateTime.Now : DateTime.ParseExact(valor, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                                            fd.entry = date;
                                            fd.tipo = TypeObject.Date;
                                            break;

                                        case "time":
                                            var time = new TimePickerNullable();
                                            layout_Form.Children.Add(time);
                                            if (valor != "")
                                                time.NullableTime = getTime(valor);
                                            else
                                                time.NullableTime = null;
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
                                            if (f.Contains("FE_Signature"))
                                                hasFESign = true;
                                            else
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
                    loader.IsVisible = false;
                    layout_Form.IsVisible = true;
                    options_Form.IsVisible = true;
                    if (hasSign || hasFESign)
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
                    if (Displayit)
                    {
                        await DisplayAlert("Warning", "Error Downloading the Report, try again ", "Ok");
                        await Navigation.PopAsync();
                    }
                }

            }
            catch (Exception ex)
            {
                var fileName = getNameFile();
                await DisplayAlert("Error", "Error loading the report, try again", "Ok");
                var pathLocal = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
                //validar si existe el reporte
                var pdf_filename = System.IO.Path.Combine(pathLocal, fileName + " (Filled).pdf");
                var pdf_filename2 = System.IO.Path.Combine(pathLocal, fileName + " (Blank).pdf");
                File.Delete(pdf_filename);
                File.Delete(pdf_filename2);
                await Navigation.PopAsync();
            }
            finally
            {
                loader.IsVisible = false;
            }

        }

        private async Task<bool> GuardarReporte()
        {
            try
            {
                var fileName = getNameFile();

                var pdf_filename_blank = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), fileName + " (Blank).pdf");

                var pdf_filename_fill = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), fileName + " (Filled).pdf");

                var pdf_filename_signed = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), fileName + " (Signed).pdf");

                //Eliminamos el reporte firmado
                if (File.Exists(pdf_filename_signed))
                    File.Delete(pdf_filename_signed);

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
                                        var obj = (TimePickerNullable)formDi.entry;
                                        var time = obj.Time;
                                        var dt = DateTime.Today.Add(time);
                                        valor = obj.NullableTime.HasValue ? dt.ToString("hh:mm tt") : "";
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

                    //Guardar siempre external
                    if (Device.RuntimePlatform == Device.Android)
                    {
                        var status_write = await RequestPermision.CheckAndRequestPermissionAsync<Permissions.StorageWrite>(new Permissions.StorageWrite());
                        var status_read = await RequestPermision.CheckAndRequestPermissionAsync<Permissions.StorageRead>(new Permissions.StorageRead());

                        if (status_write == PermissionStatus.Granted && status_read == PermissionStatus.Granted)
                        {
                            var dependency = DependencyService.Get<ILocalFileProvider>();

                            if (dependency != null)
                            {
                                using (FileStream pdfStream = File.OpenRead(pdf_filename_fill))
                                {
                                    var local = Task.Run(() => dependency.SaveFileToDisk(pdfStream, $"{fileName}.pdf")).Result;
                                }
                            }
                            
                        }
                    }

                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                await DisplayAlert("Warning", "Error saving the report, try it again ", "Ok");
                return false;
            }
            finally
            {

            }

        }

        private async void EnviarReporte(string type = "Filled")
        {
            try
            {
                loader.IsVisible = true;
                var fileName = getNameFile();
                var pdf_filename_fill = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), fileName + " (" + type + ").pdf");
                using (var form = new MultipartFormDataContent())
                {
                    using (var fs = File.OpenRead(pdf_filename_fill))
                    {
                        using (var streamContent = new StreamContent(fs))
                        {
                            using (var fileContent = new ByteArrayContent(await streamContent.ReadAsByteArrayAsync()))
                            {
                                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                                var buffer = System.Text.Encoding.UTF8.GetBytes(this.ticket.Id + "");
                                var byteContent = new ByteArrayContent(buffer);
                                form.Add(fileContent, "file", Path.GetFileName(pdf_filename_fill));
                                form.Add(byteContent, "obj");

                                var response = await HttpClientInstance.PostAsyncForm("/sendReport", form);
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
            catch (Exception)
            {
                await DisplayAlert("Warning", "Error while sending the report, try it again ", "Ok");
                if (!Displayit)
                {
                    await Navigation.PopAsync();
                }
            }
            finally
            {
                loader.IsVisible = false;
            }
        }

        private async void btnGuardar_Clicked(object sender, EventArgs e)
        {
            loader.IsVisible = true;
            var res = await GuardarReporte();
            loader.IsVisible = false;
            if (res)
            {
                string action = await DisplayActionSheet("Report Saved", "Close", null, "View PDF");
                if (action == "View PDF")
                    await Navigation.PushAsync(new PreviewPDF(this.ticket));
            }

        }

        private async void btnGuardarFirmar_Clicked(object sender, EventArgs e)
        {
            loader.IsVisible = true;
            var res = await GuardarReporte();
            loader.IsVisible = false;
            if (res)
                await Navigation.PushAsync(new SignReport(this.ticket));
        }

        private async void btnGuardarEnviar_Clicked(object sender, EventArgs e)
        {
            loader.IsVisible = true;
            var res = await GuardarReporte();
            loader.IsVisible = false;
            if (res)
                EnviarReporte();

        }


        private string[] getTypeForm(string name)
        {
            var type = new string[2];
            var name_split = name.Split('_');
            type[0] = name_split[0];
            string FieldName = "";
            for (int i = 1; i < name_split.Length; i++)
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

        private bool existRadio(string name)
        {
            var radiosButton = this.lista_form.Where(x => x.tipo == TypeObject.Radio && x.Nombre == name).ToList();
            if (radiosButton.Count > 0)
            {
                return true;
            }
            return false;
        }

        private TimeSpan getTime(string time)
        {
            var time_w = time.Substring(0, 5);

            if (time.Contains("a. m.") || time.Contains("a.m.") || time.Contains("AM"))
            {
                return TimeSpan.Parse(time_w);
            }
            else
            {
                var tt = TimeSpan.Parse(time_w);
                var horas = tt.Hours + 12 == 24 ? "00:" : tt.Hours + 12 + ":";
                return TimeSpan.Parse(horas + tt.Minutes);
            }
        }

        private string getValueTicket(string f)
        {
            string valor = "";
            if (f.Contains("Service_Date"))
                valor = this.ticket.ServiceDate.ToString("dd/MM/yyyy");

            if (f.Contains("Ticket_Number"))
                valor = this.ticket.ClientTicket;

            if (f.Contains("Site_Name"))
                valor = this.ticket.SiteName.NombreSitio;

            if (f.Contains("Site_Address"))
                valor = this.ticket.SiteName.Direccion;

            if (f.Contains("POC_Name"))
                valor = this.ticket.POC_1;

            if (f.Contains("POC_Email"))
                valor = this.ticket.Email_POC_1;

            if (f.Contains("POC_Phone"))
                valor = this.ticket.Phone_POC_1;

            if (f.Contains("Final_User_Name"))
                valor = this.ticket.Final_User;

            if (f.Contains("FE_Name"))
                valor = this.ingeniero.Name;

            if (f.Contains("Service_Description"))
                valor = this.ticket.Service_Details;

            if (f.Contains("Client_Internal_Ticket_Number"))
                valor = this.ticket.ClientIntertal;

            if (f.Contains("Account"))
                valor = this.ticket.Account;

            if (f == "txt_Client")
                valor = this.ticket.Client;

            return valor;
        }

        private bool hasBAMReport()
        {
            if (this.ticket.Client == "Barrister")
            {
                if (this.ticket.Account != "Signify" && this.ticket.HasAttachment == true)
                    return true;
            }
            return false;
        }
        #endregion

        #region MessageCenter
        private void unSubscribe()
        {
            MessagingCenter.Unsubscribe<App>(this, "connect");

            MessagingCenter.Unsubscribe<App>(this, "no_connect");
        }

        private void Subscribe()
        {
            MessagingCenter.Subscribe<App>(this, "connect", (sender) =>
            {
                isConnected = true;
                btnGuardarEnviar.IsEnabled = true;
            });

            MessagingCenter.Subscribe<App>(this, "no_connect", async (sender) =>
            {
                await DisplayAlert("Warning: No Internet Connection", "You can only save your report ", "Ok");
                btnGuardarEnviar.IsEnabled = false;
                isConnected = false;

            });
        }
        #endregion

    }
}