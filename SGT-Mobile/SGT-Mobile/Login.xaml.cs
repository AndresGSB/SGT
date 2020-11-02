using Newtonsoft.Json;
using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;
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
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SGTMobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Login : ContentPage
    {
        RequestService HttpClientInstance = new RequestService("Login");

        public Login()
        {
            InitializeComponent();
        }

        #region Init
        protected async override void OnAppearing()
        {
            base.OnAppearing();

            if (Connectivity.NetworkAccess == NetworkAccess.None)
            {
                await internetCon.FadeTo(1, 500);
                btn_login.IsEnabled = false;
            }
            else
            {
                btn_login.IsEnabled = true;
                await internetCon.FadeTo(0, 500);
            }

            Subscribe();

            if (Application.Current.Properties.ContainsKey("email") && Application.Current.Properties["email"] != null)
            {
                txt_email.Text = Application.Current.Properties["email"] as string;
            }

            if (Application.Current.Properties.ContainsKey("password") && Application.Current.Properties["password"] != null)
            {
                bool isAvailable = await CrossFingerprint.Current.IsAvailableAsync(false);
                if (isAvailable)
                {
                    txt_password.IsVisible = false;
                    lblPassword.IsVisible = false;
                    imgPasswordEye.IsVisible = false;

                    AuthenticationRequestConfiguration conf = new AuthenticationRequestConfiguration("Login", "Authenticate in order to get access");
                    var authResult = await CrossFingerprint.Current.AuthenticateAsync(conf);
                    var pass = Application.Current.Properties["password"] as string;
                    var email = Application.Current.Properties["email"] as string;
                    if (authResult.Authenticated)
                    {
                        LoginFE(new LoginRequest { Email = email.ToLower(), Password = pass });
                    }
                }
            }

        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            unSubscribe();
        }

        #endregion

        #region Funciones
        private async void ButtonLogin_Clicked(object sender, EventArgs e)
        {
            try
            {
                lbl_error.Text = "";

                if (Application.Current.Properties.ContainsKey("password") && Application.Current.Properties["password"] != null)
                {
                    bool isAvailable = await CrossFingerprint.Current.IsAvailableAsync(false);
                    if (isAvailable)
                    {
                        AuthenticationRequestConfiguration conf = new AuthenticationRequestConfiguration("Login", "Authenticate in order to get access");
                        var authResult = await CrossFingerprint.Current.AuthenticateAsync(conf);
                        var pass = Application.Current.Properties["password"] as string;
                        var email = Application.Current.Properties["email"] as string;
                        if (authResult.Authenticated)
                        {
                            LoginFE(new LoginRequest { Email = email.ToLower(), Password = pass });
                        }
                    }
                    else
                    {
                        txt_password.IsVisible = true;
                    }
                }
                else
                {
                    var email = txt_email.Text.Trim();
                    var password = txt_password.Text;

                    bool isEmail = Regex.IsMatch(email, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);
                    if (isEmail)
                    {
                        LoginFE(new LoginRequest { Email = email.ToLower(), Password = password });
                    }
                    else
                    {
                        lbl_error.Text = "Email format is invalid";
                    }
                }

                
            }
            catch (Exception)
            {
                lbl_error.Text = "Error with credentials";
            }
        }

        private void txt_email_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (txt_email.Text.Trim() == Application.Current.Properties["email"] as string)
                {
                    txt_password.IsVisible = false;
                    lblPassword.IsVisible = false;
                    imgPasswordEye.IsVisible = false;
                }
                else
                {
                    txt_password.IsVisible = true;
                    lblPassword.IsVisible = true;
                    imgPasswordEye.IsVisible = true;
                    Application.Current.Properties["password"] = null;
                    Application.Current.Properties["email"] = null;
                }
                
            }
            catch(Exception){

            }
            
        }

        private async void LoginFE(LoginRequest login)
        {
            try
            {
                lbl_error.Text = "";
                loader.IsVisible = true;
                var resultado = await HttpClientInstance.PostAsyncJSON("/login", login);

                switch (resultado.StatusCode)
                {
                    case HttpStatusCode.BadRequest:
                        lbl_error.Text = "Information is invalid";
                        Application.Current.Properties["password"] = null;
                        Application.Current.Properties["email"] = null;
                        txt_password.IsVisible = true;
                        lblPassword.IsVisible = true;
                        imgPasswordEye.IsVisible = true;
                        break;

                    case HttpStatusCode.Accepted:
                        lbl_error.Text = "Information is invalid";
                        break;

                    case HttpStatusCode.Forbidden:
                        await DisplayAlert("Alert", "You are not registered yet", "Sign Up");
                        await Navigation.PushAsync(new Signup());
                        break;

                    case HttpStatusCode.Unauthorized:
                        Application.Current.Properties["password"] = null;
                        Application.Current.Properties["email"] = null;
                        txt_password.IsVisible = true;
                        lblPassword.IsVisible = true;
                        imgPasswordEye.IsVisible = true;
                        await DisplayAlert("Alert", "You need to change your password", "Change it");
                        await Navigation.PushAsync(new ChangePassword());
                        break;


                    case HttpStatusCode.OK:
                        await DisplayAlert("Success", "Welcome", "Ok");
                        var jsonToken = resultado.Content.ReadAsStringAsync().Result;
                        Application.Current.Properties["token"] = jsonToken;
                        Application.Current.Properties["password"] = login.Password;
                        Application.Current.Properties["email"] = login.Email;
                        await Application.Current.SavePropertiesAsync();
                        App.Current.MainPage = new NavigationPage(new MasterDetailPageApp());
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
        public ICommand PopupForgot => new Command(async () =>{
            await Navigation.PushAsync(new ChangePassword(true)); 
        });

        public ICommand ScreenSigin => new Command(async () => await Navigation.PushAsync(new Signup()));

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
                btn_login.IsEnabled = true;
                await internetCon.FadeTo(0, 500);
            });

            MessagingCenter.Subscribe<App>(this, "no_connect", async (sender) =>
            {
                await DisplayAlert("Warning", "No Internet Connection", "Ok");
                btn_login.IsEnabled = false;
                await internetCon.FadeTo(1, 500);
            });
        }
        #endregion

        
    }
}