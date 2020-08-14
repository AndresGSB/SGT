using SGTMobile.Models;
using SGTMobile.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.Xaml;

namespace SGTMobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PreviewPDF : ContentPage
    {
        private Tickets ticket;
        private string local;
        public PreviewPDF(Tickets tk)
        {
            ticket = tk;
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            var fileName = getNameFile();
            var pdf_filename_fill = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), fileName + " (Filled).pdf");
            var pdf_filename_signed = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), fileName + " (Signed).pdf");
            var path = "";
            if (File.Exists(pdf_filename_signed))
                path = pdf_filename_signed;
            else
                path = pdf_filename_fill;

            if (Device.RuntimePlatform == Device.Android)
            {
                var dependency = DependencyService.Get<ILocalFileProvider>();

                if (dependency == null)
                {
                    DisplayAlert("Error loading PDF", "Try again", "OK");

                    return;
                }
                using (FileStream pdfStream = File.OpenRead(path))
                {
                    var uuid = Guid.NewGuid().ToString();
                    local = Task.Run(() => dependency.SaveFileToDisk(pdfStream, $"{uuid}.pdf")).Result;
                }

            }

            

            if (Device.RuntimePlatform == Device.Android)
                PdfView.Source = $"file:///android_asset/pdfjs/web/viewer.html?file={"file:///" + WebUtility.UrlEncode(local)}";
            else
                PdfView.Source = path;
        }

        private string getNameFile()
        {
            string date = String.Format("{0:ddMMyyyy}", this.ticket.ServiceDate);
            return this.ticket.ClientTicket + " " + this.ticket.SiteName.NombreSitio + " " + date;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (File.Exists(local))
            {
                File.Delete(local);
            }
        }




    }
}