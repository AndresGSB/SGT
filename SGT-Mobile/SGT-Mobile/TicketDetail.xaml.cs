using Newtonsoft.Json;
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
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SGTMobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TicketDetail : ContentPage
    {
        RequestService HttpClientInstance = new RequestService();
        private string controller = "tickets";
        private int TicketId;
        public Tickets ticket;
        public List<Tickets> listTicket;
        public bool isConnected = true;

        public TicketDetail(int id)
        {
            InitializeComponent();
            this.TicketId = id;
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
                internetCon.FadeTo(1, 500);
            }

            MessagingCenter.Subscribe<App>(this, "connect", (sender) =>
            {
                isConnected = true;
                internetCon.FadeTo(0, 500);
                internetCon.IsVisible = false;
                loadTicket();
            });

            MessagingCenter.Subscribe<App>(this, "no_connect", (sender) =>
            {
                isConnected = false;
                DisplayAlert("Warning", "No Internet Connection", "Ok");
                internetCon.IsVisible = true;
                internetCon.FadeTo(1, 500);
            });

            if (Application.Current.Properties.ContainsKey("token") && Application.Current.Properties["token"] != null)
            {       
                loadTicket();
            }
            else
            {
                Application.Current.Properties["token"] = null;
                unSubscribe();
                App.Current.MainPage = new Login();
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            unSubscribe();
        }

        public async void loadTicket()
        {
            try
            {
                Loader.IsVisible = true;
                if (isConnected)
                {
                    var resultado = await HttpClientInstance.GetAsync(controller + "/" + TicketId);
                    switch (resultado.StatusCode)
                    {
                        case HttpStatusCode.OK:


                            var json = resultado.Content.ReadAsStringAsync().Result;
                            ticket = Tickets.FromJsonUnique(json);
                            lblTitulo.Text = ticket.Titulo;
                            lblClientTicket.Text = ticket.ClientTicket;
                            lblAccount.Text = ticket.Account;
                            lbl_ServiceDetails.Text = ticket.Service_Details;
                            lbl_ServiceDate.Text = ticket.ServiceDate.ToString();
                            lbl_SiteName_Name.Text = ticket.SiteName.NombreSitio;
                            lbl_SiteName_Location.Text = ticket.SiteName.Direccion;
                            lbl_POC_1.Text = ticket.POC_1;
                            lbl_Phone_POC_1.Text = ticket.Phone_POC_1;
                            lbl_Email_POC_1.Text = ticket.Email_POC_1;
                            lbl_Final_User.Text = ticket.Final_User;
                            lbl_Final_User_Phone.Text = ticket.Final_User_Phone;
                            lbl_Final_User_Email.Text = ticket.Final_User_Email;

                            gridTicket.IsVisible = true;

                            SaveTicket();

                            break;

                        case HttpStatusCode.Unauthorized:
                        case HttpStatusCode.InternalServerError:
                            Loader.IsVisible = false;

                            await DisplayAlert("Warning", "Your session is expired", "Login");
                            Application.Current.Properties["token"] = null;
                            unSubscribe();
                            App.Current.MainPage = new Login();
                            break;

                        default:
                            //error
                            break;
                    }
                }
                else
                {
                    //ticket guardada previamente    
                    if (Application.Current.Properties.ContainsKey("tickets") && Application.Current.Properties["tickets"] != null)
                    {
                        
                        listTicket = Tickets.FromJson(Application.Current.Properties["tickets"] as string);
                        ticket = listTicket.Where(x => x.Id == TicketId).FirstOrDefault();
                        if (ticket != null)
                        {
                            await DisplayAlert("Warning: No Internet Connection", "Saved Ticket Loaded", "Ok");
                            lblTitulo.Text = ticket.Titulo;
                            lblClientTicket.Text = ticket.ClientTicket;
                            lblAccount.Text = ticket.Account;
                            lbl_ServiceDetails.Text = ticket.Service_Details;
                            lbl_ServiceDate.Text = ticket.ServiceDate.ToString();
                            lbl_SiteName_Name.Text = ticket.SiteName.NombreSitio;
                            lbl_SiteName_Location.Text = ticket.SiteName.Direccion;
                            lbl_POC_1.Text = ticket.POC_1;
                            lbl_Phone_POC_1.Text = ticket.Phone_POC_1;
                            lbl_Email_POC_1.Text = ticket.Email_POC_1;
                            lbl_Final_User.Text = ticket.Final_User;
                            lbl_Final_User_Phone.Text = ticket.Final_User_Phone;
                            lbl_Final_User_Email.Text = ticket.Final_User_Email;

                            gridTicket.IsVisible = true;
                        }
                        else
                        {
                            await DisplayAlert("Warning: No Internet Connection", "None information saved", "Ok");
                            unSubscribe();
                            await Navigation.PopAsync();
                        }
                    }
                }
                
            }
            catch(Exception ex)
            {
                await DisplayAlert("Error", "Error while loading ticket", "Try it Again");
                await Navigation.PopAsync();
            }
            finally
            {
                Loader.IsVisible = false;
            }
            
        }

        private void SaveTicket()
        {
            if (Application.Current.Properties.ContainsKey("tickets") && Application.Current.Properties["tickets"] != null)
            {
                listTicket = Tickets.FromJson(Application.Current.Properties["tickets"] as string);
                var tk = listTicket.Where(x => x.Id == TicketId).FirstOrDefault();
                if (tk != null)
                {
                    tk = this.ticket;
                    Application.Current.Properties["tickets"] = Tickets.ToJson(listTicket);
                }


            }
        }

        private void unSubscribe()
        {
            MessagingCenter.Unsubscribe<App>(this, "connect");

            MessagingCenter.Unsubscribe<App>(this, "no_connect");
        }

    }
}