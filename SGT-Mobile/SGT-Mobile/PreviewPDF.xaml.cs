using SGTMobile.Models;
using SGTMobile.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
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

        protected async override void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                var fileName = getNameFile();
                var pdf_filename_fill = System.IO.Path.Combine(Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), fileName + " (Filled).pdf");
                var pdf_filename_signed = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), fileName + " (Signed).pdf");
                var path = "";
                if (File.Exists(pdf_filename_signed))
                    path = pdf_filename_signed;
                else
                    path = pdf_filename_fill;

                if (Device.RuntimePlatform == Device.Android)
                {
                    var status_write = await RequestPermision.CheckAndRequestPermissionAsync<Permissions.StorageWrite>(new Permissions.StorageWrite());
                    var status_read = await RequestPermision.CheckAndRequestPermissionAsync<Permissions.StorageRead>(new Permissions.StorageRead());

                    if (status_write == PermissionStatus.Granted && status_read == PermissionStatus.Granted)
                    {
                        var dependency = DependencyService.Get<ILocalFileProvider>();

                        if (dependency == null)
                        {
                            await DisplayAlert("Error loading PDF", "Try again", "OK");
                            return;
                        }
                        using (FileStream pdfStream = File.OpenRead(path))
                        {
                            local = Task.Run(() => dependency.SaveFileToDisk(pdfStream, $"{fileName}.pdf")).Result;
                        }
                    }
                    else
                    {
                        await DisplayAlert("Error Permissions", "Grant permission in order to preview the file", "OK");
                        await Navigation.PopAsync();
                    }
                }

                var list = Android.App.Application.Context.Assets.List("pdfjs");

                if (Device.RuntimePlatform == Device.Android)
                    PdfView.Source = $"file:///android_asset/pdfjs/web/viewer.html?file={"file:///" + local}";
                else
                    PdfView.Source = path;
            }
            catch (Exception)
            {
                await DisplayAlert("Error", "Try it again", "OK");
            }
            
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }

        private string getNameFile()
        {
            string date = String.Format("{0:ddMMyyyy}", this.ticket.ServiceDate);
            return this.ticket.ClientTicket + " " + this.ticket.SiteName.NombreSitio + " " + date;
        }

    }
}