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
        RequestService HttpClientInstance = new RequestService("tickets");
        List<Tickets> tickets;
        public bool isConnected = true;

        public MyServices()
        {
            InitializeComponent();
        }

        #region Init
        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (Connectivity.NetworkAccess == NetworkAccess.None)
            {
                isConnected = false;
                internetCon.FadeTo(1, 500);
                internetCon.IsVisible = true;
            }
            else
            {
                isConnected = true;
                internetCon.FadeTo(0, 500);
                internetCon.IsVisible = false;
            }

            Subscribe();

            if (Application.Current.Properties.ContainsKey("token") && Application.Current.Properties["token"] != null)
            {
                //listaTickets.ItemsSource = null;
                LlenarTickets();
                listaTickets.RefreshCommand = new Command(() => {
                    LlenarTickets();
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
        #endregion

        #region Funciones
        private async void LlenarTickets()
        {
            try
            {
                listaTickets.IsRefreshing = true;
                if (isConnected)
                {
                    var resultado = await HttpClientInstance.GetAsync("/getFE");
                    listaTickets.IsRefreshing = false;
                    switch (resultado.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            var json = resultado.Content.ReadAsStringAsync().Result;
                            tickets = Tickets.FromJson(json);
                            Application.Current.Properties["tickets"] = json;
                            tickets = ChangeColor(tickets);
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
                    //await DisplayAlert("Warning: No Internet Connection", "Saved previous Tickets Loaded", "Ok");
                    if (Application.Current.Properties.ContainsKey("tickets") && Application.Current.Properties["tickets"] != null)
                    {
                        tickets = Tickets.FromJson(Application.Current.Properties["tickets"] as string);
                        tickets = ChangeColor(tickets);
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
            catch (Exception)
            {
                await DisplayAlert("Error", "Error while loading tickets", "Try it Again");
                listaTickets.ItemsSource = null;
            }
            finally
            {
                listaTickets.IsRefreshing = false;
            }


        }

        private void ListaTickets_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var item = listaTickets.SelectedItem as Tickets;
            Navigation.PushAsync(new TicketDetail(item.Id));
        }

        public List<Tickets> ChangeColor(List<Tickets> tickets)
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

        private async void CheckTickets()
        {
            if (this.tickets.Count > 0)
            {
                await noTickets.FadeTo(0, 500);
                noTickets.IsVisible = false;
            }
            else
            {
                noTickets.IsVisible = true;
                await noTickets.FadeTo(1, 500);
            }
        }
        #endregion

        #region Commands
        public ICommand AlertTicket => new Command(async (obj) =>
        {
            var tk = (Tickets)obj;
            string textOPC = tk.Report_Status_Mobile switch
            {
                "Pending to GSB" => "Preview Report",
                "Rejected" => "Edit Report",
                "Approved" => "Not accionts allowed",
                _ => "Generate Report",
            };
            if (textOPC == "Generate Report")
            {
                string date = String.Format("{0:ddMMyyyy}", tk.ServiceDate);
                var nameDoc = tk.ClientTicket + " " + tk.SiteName.NombreSitio + " " + date;

                var pathLocal = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);

                var pdf_filename_fill = System.IO.Path.Combine(pathLocal, nameDoc + " (Filled).pdf");
                if (File.Exists(pdf_filename_fill))
                    textOPC = "Edit Report";
            }
            string action = await DisplayActionSheet("Options", "Close", null, textOPC);
            if (action != "Close" && action != "Not accionts allowed" && action != null)
                if (action == "Generate Report" || action == "Edit Report")
                    await Navigation.PushAsync(new Report(tk));
                else
                    await Navigation.PushAsync(new PreviewPDF(tk));
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
        #endregion

        #region MessageCenter
        private void unSubscribe()
        {
            MessagingCenter.Unsubscribe<App>(this, "connect");

            MessagingCenter.Unsubscribe<App>(this, "no_connect");
        }

        private void Subscribe()
        {
            MessagingCenter.Subscribe<App>(this, "connect", async (sender) =>
            {
                isConnected = true;
                await internetCon.FadeTo(0, 500);
                internetCon.IsVisible = false;
                listaTickets.ItemsSource = null;
                LlenarTickets();
            });

            MessagingCenter.Subscribe<App>(this, "no_connect", async (sender) =>
            {
                await DisplayAlert("Warning", "No Internet Connection", "Ok");
                isConnected = false;
                internetCon.IsVisible = true;
                await internetCon.FadeTo(1, 500);

            });
        }
        #endregion

    }
}