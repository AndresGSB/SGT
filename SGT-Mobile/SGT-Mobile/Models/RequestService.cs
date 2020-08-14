using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SGTMobile.Models
{
    public class RequestService 
    {
        private static HttpClient instance;
        //public static string Server = "http://10.0.2.2/SGT/api/";
        public static string Server = "http://192.168.1.70/SGT/api/";
        //public static string Server = "http://74.208.87.192/SGT/api/";
        private static HttpClientHandler clientHandler 
            { 
                get{
                    HttpClientHandler ch = new HttpClientHandler();
                    ch.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
                    return ch;
                }
            }
        private static HttpClient HttpClientInstance => instance ?? ( instance = new HttpClient(clientHandler));
        

        public async Task<HttpResponseMessage> GetAsync(string uri)
        {
            setupHttpClient();
            HttpResponseMessage response = await HttpClientInstance.GetAsync(Server + uri);
            return response;
        }

        public async Task<HttpResponseMessage> PostAsyncJSON(string uri, ByteArrayContent content)
        {
            setupHttpClient();
            HttpResponseMessage response = await HttpClientInstance.PostAsync(Server + uri, content);
            return response;
        }

        public async Task<HttpResponseMessage> PostAsyncForm(string uri, MultipartFormDataContent content)
        {
            setupHttpClient();
            HttpResponseMessage response = await HttpClientInstance.PostAsync(Server + uri, content);
            
            return response;
        }

        private void setupHttpClient()
        {
            if (Application.Current.Properties.ContainsKey("token") && Application.Current.Properties["token"] != null)
            {
                var ingeniero = JsonConvert.DeserializeObject<RecursoIngeniero>(Application.Current.Properties["token"] as string);
                if (!string.IsNullOrWhiteSpace(ingeniero.Token))
                {
                    HttpClientInstance.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ingeniero.Token);
                }
            }
            else
            {
                HttpClientInstance.DefaultRequestHeaders.Authorization = null;
            }
            
        }

    }
}
