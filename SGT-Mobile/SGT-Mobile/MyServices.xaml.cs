using SGTMobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SGTMobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MyServices : ContentPage
    {
        public MyServices()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            llenarTickets();
        }

        private async void llenarTickets()
        {

            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            HttpClient cliente = new HttpClient(clientHandler);
            string url = "https://10.0.2.2:44396/api/Tickets";

            var content = new StringContent("application/json");
            var resultado = await cliente.GetAsync(url);
            var json = resultado.Content.ReadAsStringAsync().Result;

            List<Tickets> tickets = Tickets.FromJson(json);

            listaTickets.ItemsSource = tickets;

        }


    }
}