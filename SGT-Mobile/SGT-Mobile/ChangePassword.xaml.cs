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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SGTMobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChangePassword : ContentPage
    {

        RequestService HttpClientInstance = new RequestService("Login");
        bool RequestMessages;

        public ChangePassword(bool RequestMessage = false)
        {
            RequestMessages = RequestMessage;
            InitializeComponent();
        }

        #region Init
        protected async override void OnAppearing()
        {
            base.OnAppearing();
            if(RequestMessages)
             await DisplayAlert("Alert", "In order to reset your password, contact with your admin", "OK");
            if (Connectivity.NetworkAccess == NetworkAccess.None)
            {
                await internetCon.FadeTo(1, 500);
                btn_change.IsEnabled = false;
            }
            else
            {
                btn_change.IsEnabled = true;
                await internetCon.FadeTo(0, 500);
            }

            Subscribe();

        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            unSubscribe();
        }

        #endregion

        #region Funciones
        private void ButtonSigin_Clicked(object sender, EventArgs e)
        {      
            try
            {
                lbl_error.Text = "";
                var email = txt_email.Text.ToLower().Trim();
                bool isEmail = Regex.IsMatch(email, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);
                if (isEmail)
                {
                    if (txt_password.Text == txt_confirm_password.Text)
                    {
                        var login = new LoginRequest { Email = email, Password = txt_password.Text };
                        Change(login);
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
            catch (Exception)
            {
                lbl_error.Text = "Error with data given";
            }
        }

        private async void ButtonBack_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void Change(LoginRequest login)
        {
            try
            {
                loader.IsVisible = true;
                lbl_error.Text = "";
                var resultado = await HttpClientInstance.PostAsyncJSON("/changePassword", login);
                switch (resultado.StatusCode)
                {
                    case HttpStatusCode.BadRequest:
                        lbl_error.Text = "Information is invalid";
                        break;

                    case HttpStatusCode.Forbidden:
                        lbl_error.Text = "Can't change your password, contact with your admin";
                        break;

                    case HttpStatusCode.InternalServerError:
                        await DisplayAlert("Error", "Try again", "Ok");
                        lbl_error.Text = "";
                        break;

                    case HttpStatusCode.OK:
                        await DisplayAlert("Success", "Password Changed", "Login");
                        App.Current.MainPage = new Login();
                        break;
                }
                
            }
            catch (Exception ex)
            {
                lbl_error.Text = "Application Error, try again";
                await DisplayAlert("Application Error", ex.Message, "Ok");
            }
            finally
            {
                loader.IsVisible = false;
            }
            

        }
        #endregion

        #region Commands
        private void ShowPassword(object sender, EventArgs e)
        {
            txt_password.IsPassword = !txt_password.IsPassword;
            imgPasswordEye.Source = txt_password.IsPassword ? "eye.png" : "eyeSlash.png";
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
                btn_change.IsEnabled = true;
                await internetCon.FadeTo(0, 500);
            });

            MessagingCenter.Subscribe<App>(this, "no_connect", async (sender) =>
            {
                await DisplayAlert("Warning", "No Internet Connection", "Ok");
                btn_change.IsEnabled = false;
                await internetCon.FadeTo(1, 500);
            });
        }
        #endregion

    }
}