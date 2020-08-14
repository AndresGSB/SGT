using Newtonsoft.Json;
using SGT_Mobile;
using SGTMobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SGTMobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Signup : ContentPage
    {

        RequestService HttpClientInstance = new RequestService();
        private String Controller = "Login";

        public Signup()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            MessagingCenter.Subscribe<App>(this, "connect", (sender) =>
            {
                btn_sign.IsEnabled = true;
                internetCon.FadeTo(0, 500);
            });

            MessagingCenter.Subscribe<App>(this, "no_connect", (sender) =>
            {
                DisplayAlert("Warning", "No Internet Connection", "Ok");
                btn_sign.IsEnabled = false;
                internetCon.FadeTo(1, 500);
            });

        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            unSubscribe();
        }

        private void ButtonSigin_Clicked(object sender, EventArgs e)
        {
            lbl_error.Text = "";
            var email = txt_email.Text.ToLower().Trim();
            try
            {
                bool isEmail = Regex.IsMatch(email, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);
                if (isEmail)
                {
                    if (txt_password.Text == txt_confirm_password.Text)
                    {
                        var login = new LoginRequest { Email = email, Password = txt_password.Text };
                        Register(login);
                    }
                    else
                    {
                        lbl_error.Text = "Passwords are not same";
                    }
                    
                }
                else
                {
                    lbl_error.Text = "Email is invalid";
                }
            }
            catch (Exception ex)
            {
                lbl_error.Text = "Error with data given";
            }
        }

        private async void ButtonBack_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void Register(LoginRequest login)
        {
            loader.IsVisible = true;
            lbl_error.Text = "";

            var content = JsonConvert.SerializeObject(login);
            var buffer = System.Text.Encoding.UTF8.GetBytes(content);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var resultado = await HttpClientInstance.PostAsyncJSON(Controller + "/register", byteContent);
            switch(resultado.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    lbl_error.Text = "Information is invalid";
                    break;

                case HttpStatusCode.Forbidden:
                    lbl_error.Text = "Email is already registered";
                    break;

                case HttpStatusCode.InternalServerError:
                    await DisplayAlert("Error", "Try again", "Ok");
                    lbl_error.Text = "";
                    break;

                case HttpStatusCode.OK:
                    await DisplayAlert("Success", "You have been registered","Login");
                    App.Current.MainPage = new Login();
                    break;
            }

            loader.IsVisible = false;

        }

        private void unSubscribe()
        {
            MessagingCenter.Unsubscribe<App>(this, "connect");

            MessagingCenter.Unsubscribe<App>(this, "no_connect");
        }
    }
}