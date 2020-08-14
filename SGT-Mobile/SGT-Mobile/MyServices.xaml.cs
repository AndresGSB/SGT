using Newtonsoft.Json;
using Rg.Plugins.Popup.Services;
using SGT_Mobile;
using SGTMobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SGTMobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MyServices : ContentPage
    {
        ViewCell lastCell;
        RequestService HttpClientInstance = new RequestService();
        private RecursoIngeniero Ingeniero;
        private string controller = "tickets";
        List<Tickets> tickets;
        public bool isConnected = true;

        public MyServices()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (Connectivity.NetworkAccess == NetworkAccess.None)
            {
                isConnected = false;
            }

            if (!isConnected)
            {
                internetCon.IsVisible = true;
                internetCon.FadeTo(1, 500);    
            }

            MessagingCenter.Subscribe<App>(this, "connect", (sender) =>
            {
                isConnected = true;
                internetCon.FadeTo(0, 500);
                internetCon.IsVisible = false;
                llenarTickets();
            });

            MessagingCenter.Subscribe<App>(this, "no_connect", (sender) =>
            {
                DisplayAlert("Warning", "No Internet Connection", "Ok");
                isConnected = false;
                internetCon.IsVisible = true;
                internetCon.FadeTo(1, 500);

            });

            if (Application.Current.Properties.ContainsKey("token") && Application.Current.Properties["token"] != null )
            {
                this.Ingeniero = JsonConvert.DeserializeObject<RecursoIngeniero>(Application.Current.Properties["token"] as string);

                llenarTickets();
                listaTickets.RefreshCommand = new Command(() => {
                    llenarTickets();
                });
            }
            else
            {
                Application.Current.Properties["token"] = null;
                unSubscribe();
                App.Current.MainPage = new NavigationPage(new Login());
            }

        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            unSubscribe();
        }

        private async void llenarTickets()
        {
            try
            {
                listaTickets.IsRefreshing = true;
                if (isConnected)
                {
                    var resultado = await HttpClientInstance.GetAsync(controller + "/getFE");
                    listaTickets.IsRefreshing = false;
                    switch (resultado.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            var json = resultado.Content.ReadAsStringAsync().Result;
                            tickets = Tickets.FromJson(json);
                            Application.Current.Properties["tickets"] = json;
                            tickets = changeColor(tickets);
                            listaTickets.ItemsSource = tickets;
                            CheckTickets();
                            break;

                        case HttpStatusCode.Unauthorized:
                        case HttpStatusCode.InternalServerError:
                            await DisplayAlert("Warning", "Your session is expired", "Login");
                            Application.Current.Properties["token"] = null;
                            unSubscribe();
                            App.Current.MainPage = new NavigationPage(new Login());
                            break;

                        default:
                            await DisplayAlert("Error", "Error while loading tickets", "Try it Again");
                            listaTickets.ItemsSource = null;
                            break;
                    }
                }
                else
                {
                    //lista guardada previamente 
                    listaTickets.IsRefreshing = false;
                    await DisplayAlert("Warning: No Internet Connection", "Saved previous Tickets Loaded", "Ok");
                    if (Application.Current.Properties.ContainsKey("tickets") && Application.Current.Properties["tickets"] != null)
                    {
                        tickets = Tickets.FromJson(Application.Current.Properties["tickets"] as string);
                        tickets = changeColor(tickets);
                        listaTickets.ItemsSource = tickets;
                        CheckTickets();
                    }
                    else
                    {
                        noTickets.IsVisible = true;
                        await noTickets.FadeTo(1, 500);
                    }

                }
            }
            catch(Exception ex)
            {
                await DisplayAlert("Error", "Error while loading tickets", "Try it Again");
                listaTickets.ItemsSource = null;
            }
            finally
            {
                listaTickets.IsRefreshing = false;
            }
            
            
        }

        public List<Tickets> changeColor(List<Tickets> tickets)
        {
            foreach (var tk in tickets)
            {
                switch (tk.Report_Status_Mobile)
                {
                    case "Pending to GSB":
                        tk.color = "#E2CD00";
                        break;

                    case "Rejected":
                        tk.color = "#BF0101";
                        break;

                    case "Approved":
                        tk.color = "#11AF01";
                        break;

                    default:
                        tk.color = "#C1C1C1";
                        tk.Report_Status_Mobile = "Pending to Send";
                        break;
                }
            }
            return tickets;
        }

        private void listaTickets_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var item = listaTickets.SelectedItem as Tickets;
            Navigation.PushAsync(new TicketDetail(item.Id));
        }

        public ICommand AlertTicket => new Command(async (obj) =>
        {
            var tk = (Tickets)obj;
            string textOPC; 
            switch (tk.Report_Status_Mobile)
            {
                case "Pending to GSB":
                    textOPC = "Preview Report";
                    break;

                case "Rejected":
                    textOPC = "Edit Report";
                    break;

                case "Approved":
                    textOPC = "Not accionts allowed";
                    break;

                default:
                    textOPC = "Generate Report";
                    break;
            }
            string action = await DisplayActionSheet("Options", "Close", null, textOPC);
            if(action != "Close" && action != "Not accionts allowed" && action != null)
                if (action == "Generate Report" || action == "Edit Report")
                {
                    await Navigation.PushAsync(new Report(tk));
                }
                else
                {
                    await Navigation.PushAsync(new PreviewPDF(tk));
                }
                
        });

        private void ViewCell_Tapped(object sender, EventArgs e)
        {
            if (lastCell != null)
                lastCell.View.BackgroundColor = Color.Transparent;

            var viewCell = (ViewCell)sender;

            if (viewCell.View != null)
            {
                viewCell.View.BackgroundColor = Color.FromHex("#CBCBCB");
                lastCell = viewCell;
            }
        }

        private void CheckTickets()
        {
            if (this.tickets.Count > 0)
            {
                noTickets.FadeTo(0, 500);
                noTickets.IsVisible = false;
            }
            else
            {
                noTickets.IsVisible = true;
                noTickets.FadeTo(1, 500);
            }
        }

        private void unSubscribe()
        {
            MessagingCenter.Unsubscribe<App>(this, "connect");

            MessagingCenter.Unsubscribe<App>(this, "no_connect");
        }
    }
}