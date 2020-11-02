using iText.Forms;
using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using Newtonsoft.Json;
using SGT_Mobile;
using SGTMobile.Models;
using SignaturePad.Forms;
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
    public partial class SignReport : ContentPage
    {
        private Tickets ticket;
        public RequestService HttpClientInstance = new RequestService("tickets");
        public bool isConnected = true;

        public SignReport(Tickets tk)
        {
            ticket = tk;
            InitializeComponent();
        }

        #region Init
        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (Connectivity.NetworkAccess == NetworkAccess.None)
                isConnected = false;
            else
                isConnected = true;

            Subscribe();

            if (Application.Current.Properties.ContainsKey("token") && Application.Current.Properties["token"] != null)
            {
                if (getSign("sign_Signature"))
                {
                    lblTitle.Text = "POC Signature";
                    signature.IsVisible = true;
                    btnSent.IsVisible = true;
                    btnNoSent.IsVisible = true;
                }
                else
                {
                    if (getSign("sign_FE_Signature"))
                    {
                        lblTitle.Text = "FE Signature";
                        FEsignature.IsVisible = true;
                        btnSentFE.IsVisible = true;
                    }
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
        }
        #endregion

        #region Funciones
        private async void btnSent_Clicked(object sender, EventArgs e)
        {
            loader.IsVisible = true;

            string ext = "(Signed)";
            //Si el ingeniero requiere firmar
            if (getSign("sign_FE_Signature"))
                ext = "(SignedC)";

            var fileName = getNameFile();
            var pathLocal = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
            var pdf_filename_fill = System.IO.Path.Combine(pathLocal, fileName + " (Filled).pdf");
            var pdf_filename_signed = System.IO.Path.Combine(pathLocal, fileName + " " + ext + ".pdf");

            bool doesExist = File.Exists(pdf_filename_fill);
            if (doesExist)
            {
                using (PdfReader reader = new PdfReader(pdf_filename_fill))
                {

                    var writer = new PdfWriter(pdf_filename_signed, new WriterProperties());
                    using (PdfDocument pdf = new PdfDocument(reader, writer))
                    {
                        PdfAcroForm formpdf = PdfAcroForm.GetAcroForm(pdf, true);
                        PdfArray position = formpdf.GetField("sign_Signature").GetWidgets()[0].GetRectangle();
                        float width = (float)(position.GetAsNumber(2).GetValue() - position.GetAsNumber(0).GetValue());
                        float height = (float)(position.GetAsNumber(3).GetValue() - position.GetAsNumber(1).GetValue());
                        int page = pdf.GetPageNumber(formpdf.GetField("sign_Signature").GetWidgets()[0].GetPage());

                        //guardar imagen firma
                        var stream_image = await signature.GetImageStreamAsync(SignatureImageFormat.Png);
                        var memory = new MemoryStream();
                        stream_image.CopyTo(memory);
                        iText.Layout.Element.Image image = new iText.Layout.Element.Image(ImageDataFactory.Create(memory.ToArray()));
                        image.SetHeight(height);
                        image.SetFixedPosition(page, (float)position.GetAsNumber(0).GetValue(), (float)position.GetAsNumber(1).GetValue());
                        formpdf.RemoveField("sign_Signature");
                        Document doc = new Document(pdf, pdf.GetDefaultPageSize());
                        doc.Add(image);
                        doc.Close();
                        pdf.Close();
                    }

                    reader.Close();
                    //Si el ingeniero requiere firmar
                    if (!getSign("sign_FE_Signature"))
                        EnviarReporte();
                    else
                    {
                        loader.IsVisible = false;
                        lblTitle.Text = "FE Signature";
                        signature.IsVisible = false;
                        FEsignature.IsVisible = true;
                        btnSent.IsVisible = false;
                        btnNoSent.IsVisible = false;
                        btnSentFE.IsVisible = true;
                    }
                }
            }
        }

        private async void btnNoSent_Clicked(object sender, EventArgs e)
        {
            loader.IsVisible = true;

            string ext = "(Signed)";
            //Si el ingeniero requiere firmar
            if (getSign("sign_FE_Signature"))
                ext = "(SignedC)";

            var pathLocal = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
            var fileName = getNameFile();
            var pdf_filename_fill = System.IO.Path.Combine(pathLocal, fileName + " (Filled).pdf");
            var pdf_filename_signed = System.IO.Path.Combine(pathLocal, fileName + " " + ext + ".pdf");

            bool doesExist = File.Exists(pdf_filename_fill);
            if (doesExist)
            {
                string result = await DisplayPromptAsync("Question", "Why is the reason that POC can't sign?");

                if (!String.IsNullOrEmpty(result))
                    using (PdfReader reader = new PdfReader(pdf_filename_fill))
                    {

                        var writer = new PdfWriter(pdf_filename_signed, new WriterProperties());
                        using (PdfDocument pdf = new PdfDocument(reader, writer))
                        {
                            PdfAcroForm formpdf = PdfAcroForm.GetAcroForm(pdf, true);
                            formpdf.GetField("sign_Signature").SetValue(result);
                            pdf.Close();
                        }

                        reader.Close();
                        //Si el ingeniero requiere firmar
                        if (!getSign("sign_FE_Signature"))
                            EnviarReporte();
                        else
                        {
                            loader.IsVisible = false;
                            lblTitle.Text = "FE Signature";
                            signature.IsVisible = false;
                            FEsignature.IsVisible = true;
                            btnSent.IsVisible = false;
                            btnNoSent.IsVisible = false;
                            btnSentFE.IsVisible = true;
                        }
                    }
            }

            loader.IsVisible = false;
        }

        private async void btnSentFE_Clicked(object sender, EventArgs e)
        {

            var fileName = getNameFile();
            var pathLocal = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
            var pdf_filename_fill = System.IO.Path.Combine(pathLocal, fileName + " (Filled).pdf");
            var pdf_filename_signedF = System.IO.Path.Combine(pathLocal, fileName + " (Signed).pdf");
            var pdf_filename_signedC = System.IO.Path.Combine(pathLocal, fileName + " (SignedC).pdf");

            string pdf_filename = "";
            if (File.Exists(pdf_filename_signedC))
                pdf_filename = pdf_filename_signedC;
            else
                pdf_filename = pdf_filename_fill;

            bool doesExist = File.Exists(pdf_filename_fill);
            if (doesExist)
            {
                using (PdfReader reader = new PdfReader(pdf_filename))
                {

                    var writer = new PdfWriter(pdf_filename_signedF, new WriterProperties());
                    using (PdfDocument pdf = new PdfDocument(reader, writer))
                    {
                        PdfAcroForm formpdf = PdfAcroForm.GetAcroForm(pdf, true);
                        PdfArray position = formpdf.GetField("sign_FE_Signature").GetWidgets()[0].GetRectangle();
                        float width = (float)(position.GetAsNumber(2).GetValue() - position.GetAsNumber(0).GetValue());
                        float height = (float)(position.GetAsNumber(3).GetValue() - position.GetAsNumber(1).GetValue());
                        int page = pdf.GetPageNumber(formpdf.GetField("sign_FE_Signature").GetWidgets()[0].GetPage());
                        //guardar imagen firma
                        var stream_image = await FEsignature.GetImageStreamAsync(SignatureImageFormat.Png);
                        var memory = new MemoryStream();
                        stream_image.CopyTo(memory);
                        iText.Layout.Element.Image image = new iText.Layout.Element.Image(ImageDataFactory.Create(memory.ToArray()));
                        image.SetHeight(height);
                        image.SetFixedPosition(page, (float)position.GetAsNumber(0).GetValue(), (float)position.GetAsNumber(1).GetValue());
                        formpdf.RemoveField("sign_FE_Signature");
                        Document doc = new Document(pdf, pdf.GetDefaultPageSize());
                        doc.Add(image);

                        doc.Close();
                        pdf.Close();
                    }

                    reader.Close();
                    EnviarReporte();
                }
            }
        }

        private async void EnviarReporte()
        {
            try
            {
                if (isConnected)
                {
                    loader.IsVisible = true;
                    var fileName = getNameFile();
                    var pdf_filename_fill = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), fileName + " (Signed).pdf");
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
                                            string action = await DisplayActionSheet("The report has been uploaded", "Go to My Services", null, "Preview Report");
                                            switch (action)
                                            {
                                                case "Go to My Services":
                                                    Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 2]);
                                                    await Navigation.PopAsync();
                                                    break;

                                                case "Preview Report":
                                                    Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 2]);
                                                    Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 1]);
                                                    await Navigation.PushAsync(new PreviewPDF(this.ticket));
                                                    break;

                                                default:
                                                    Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 2]);
                                                    await Navigation.PopAsync();
                                                    break;
                                            }

                                            break;

                                        case HttpStatusCode.BadRequest:
                                            await DisplayAlert("Warning", "Error while sending the report ", "Ok");
                                            break;

                                        case HttpStatusCode.InternalServerError:
                                            await DisplayAlert("Warning", "Error while sending the report ", "Ok");
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    string action = await DisplayActionSheet("Can't send the report due to internet connection", "Go to My Services", null, "Preview Report");
                    switch (action)
                    {
                        case "Go to My Services":
                            Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 2]);
                            await Navigation.PopAsync();
                            break;

                        case "Preview Report":
                            Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 2]);
                            Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 1]);
                            await Navigation.PushAsync(new PreviewPDF(this.ticket));
                            break;

                        default:
                            Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 2]);
                            await Navigation.PopAsync();
                            break;
                    }
                }
            }
            catch (Exception)
            {
                await DisplayAlert("Warning", "Error while sending the report ", "Ok");
            }
            finally
            {
                loader.IsVisible = false;
            }

        }

        private string getNameFile()
        {
            string date = String.Format("{0:ddMMyyyy}", this.ticket.ServiceDate);
            return this.ticket.ClientTicket + " " + this.ticket.SiteName.NombreSitio + " " + date;
        }

        private bool getSign(string signField)
        {
            var fileName = getNameFile();
            var pdf_filename_fill = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), fileName + " (Filled).pdf");
            var boolContain = false;

            bool doesExist = File.Exists(pdf_filename_fill);
            if (doesExist)
            {
                using (PdfReader reader = new PdfReader(pdf_filename_fill))
                {
                    using (PdfDocument pdf = new PdfDocument(reader))
                    {
                        PdfAcroForm formpdf = PdfAcroForm.GetAcroForm(pdf, true);
                        var fields = formpdf.GetFormFields();
                        boolContain = fields.Keys.Contains(signField);
                        pdf.Close();
                    }
                    reader.Close();
                }

                return boolContain;
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
            });

            MessagingCenter.Subscribe<App>(this, "no_connect", async (sender) =>
            {
                await DisplayAlert("Warning: No Internet Connection", "You can only save your report ", "Ok");
                isConnected = false;
            });
        }
        #endregion

    }
}