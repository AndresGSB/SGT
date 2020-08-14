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
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SGTMobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Login : ContentPage
    {
        RequestService HttpClientInstance = new RequestService();
        private String Controller = "Login";
        public bool isInternet = true;

        public Login()
        {
            InitializeComponent();
            
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            MessagingCenter.Subscribe<App>(this, "connect", (sender) =>
            {
                btn_login.IsEnabled = true;
                internetCon.FadeTo(0,500);
            });

            MessagingCenter.Subscribe<App>(this, "no_connect", (sender) =>
            {
                DisplayAlert("Warning", "No Internet Connection", "Ok");
                btn_login.IsEnabled = false;
                internetCon.FadeTo(1, 500);
            });

        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            unSubscribe();
        }

        private void ButtonLogin_Clicked(object sender, EventArgs e)
        {
            lbl_error.Text = "";
            var email = txt_email.Text.Trim();
            var password = txt_password.Text;
            try
            {
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
            catch (Exception ex)
            {
                lbl_error.Text = "Error with credentials";
            }
        }

        private async void LoginFE(LoginRequest login)
        {
            try
            {
                loader.IsVisible = true;
                var content = JsonConvert.SerializeObject(login);
                var buffer = System.Text.Encoding.UTF8.GetBytes(content);
                var byteContent = new ByteArrayContent(buffer);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var resultado = await HttpClientInstance.PostAsyncJSON(Controller + "/login", byteContent);

                switch (resultado.StatusCode)
                {
                    case HttpStatusCode.BadRequest:
                        lbl_error.Text = "Information is invalid";
                        break;

                    case HttpStatusCode.Accepted:
                        lbl_error.Text = "Information is invalid";
                        break;

                    case HttpStatusCode.Forbidden:
                        await DisplayAlert("Alert", "You are not registered yet", "Sign Up");
                        await Navigation.PushAsync(new Signup());
                        break;


                    case HttpStatusCode.OK:
                        await DisplayAlert("Success", "Welcome", "Ok");
                        var jsonToken = resultado.Content.ReadAsStringAsync().Result;
                        Application.Current.Properties["token"] = jsonToken;
                        await Application.Current.SavePropertiesAsync();
                        App.Current.MainPage = new NavigationPage(new MasterDetailPageApp());
                        break;
                }
            }
            catch(Exception ex){
                lbl_error.Text = "Application Error, try again";
            }
            finally{
                loader.IsVisible = false;
            }
            

            
        }

        public ICommand PopupForgot => new Command(async () =>{
            await DisplayAlert("Alert", "In order to reset your password, contact with your admin", "OK");
        });

        public ICommand ScreenSigin => new Command(async () => await Navigation.PushAsync(new Signup()));

        private void unSubscribe()
        {
            MessagingCenter.Unsubscribe<App>(this, "connect");

            MessagingCenter.Unsubscribe<App>(this, "no_connect");
        }
    }
}