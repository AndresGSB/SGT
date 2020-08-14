using iText.Forms;
using iText.IO.Image;
using iText.Kernel.Pdf;
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
        private RecursoIngeniero ingeniero;
        private String controller = "tickets";

        public RequestService HttpClientInstance = new RequestService();

        public SignReport(Tickets tk)
        {
            ticket = tk;
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (Application.Current.Properties.ContainsKey("token"))
            {
                this.ingeniero = JsonConvert.DeserializeObject<RecursoIngeniero>(Application.Current.Properties["token"] as string);
            }
            else
            {
                App.Current.MainPage = new NavigationPage(new Login());
            }

        }

        private async void btnSent_Clicked(object sender, EventArgs e)
        {
            var fileName = getNameFile();
            var pdf_filename_fill = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), fileName + " (Filled).pdf");
            var pdf_filename_signed = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), fileName + " (Signed).pdf");

            bool doesExist = File.Exists(pdf_filename_fill);
            if (doesExist)
            {
                using (PdfReader reader = new PdfReader(pdf_filename_fill))
                {

                    var writer = new PdfWriter(pdf_filename_signed, new WriterProperties());
                    using (PdfDocument pdf = new PdfDocument(reader, writer))
                    {
                        PdfAcroForm formpdf = PdfAcroForm.GetAcroForm(pdf, true);

                        //guardar imagen firma
                        var stream_image = await signature.GetImageStreamAsync(SignatureImageFormat.Png);
                        var memory = new MemoryStream();
                        stream_image.CopyTo(memory);
                        var coor = formpdf.GetField("sign_Signature").GetWidgets()[0].GetRectangle();
                        iText.Layout.Element.Image image = new iText.Layout.Element.Image(ImageDataFactory.Create(memory.ToArray())).ScaleAbsolute(150, 50).SetFixedPosition(coor.GetAsNumber(1).FloatValue(), coor.GetAsNumber(2).FloatValue());
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
                            //var content = JsonConvert.SerializeObject(this.ticket.Id);
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
                                    Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 2]);
                                    await Navigation.PopAsync();
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

        private string getNameFile()
        {
            string date = String.Format("{0:ddMMyyyy}", this.ticket.ServiceDate);
            return this.ticket.ClientTicket + " " + this.ticket.SiteName.NombreSitio + " " + date;
        }

    }
}