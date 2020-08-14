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
    public partial class Password : ContentPage
    {

        HttpClientHandler clientHandler;
        HttpClient cliente;
        string url = "https://10.0.2.2:44396/api/Login";

        public Password()
        {
            InitializeComponent();
            clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            cliente = new HttpClient(clientHandler);
        }

        private void ButtonLogin_Clicked(object sender, EventArgs e)
        {
            var email = txt_email.Text;
            try
            {
                bool isEmail = Regex.IsMatch(email, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);
                if (isEmail)
                {
                    CheckEmail(email);
                }
                else
                {
                    lbl_email.Text = "Email is invalid";
                }
            }
            catch (Exception ex)
            {

            }
        }

        private async void CheckEmail(string email)
        {
            var content = JsonConvert.SerializeObject(new LoginRequest { Email = email });
            var buffer = System.Text.Encoding.UTF8.GetBytes(content);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var resultado = await cliente.PostAsync(url+"/login", byteContent);
            switch(resultado.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    lbl_email.Text = "Email is invalid";
                    break;

                case HttpStatusCode.Accepted:
                    lbl_email.Text = "Not password";
                    break;

                case HttpStatusCode.NotAcceptable:
                    lbl_email.Text = "Information is invalid";
                    break;

                case HttpStatusCode.OK:
                    lbl_email.Text = "Information is valid";
                    //se guarda el token y accede a mis reportes
                    //App.Current.MainPage = new MasterDetailPageApp();
                    break;
            }
        }
    }
}