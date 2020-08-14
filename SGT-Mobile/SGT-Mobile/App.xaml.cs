using SGTMobile;
using SGTMobile.Util;
using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SGT_Mobile
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            Device.SetFlags(new string[] { "RadioButton_Experimental" });
            MainPage = new MasterDetailPageApp();
        }

        protected override void OnStart()
        {
            ConnectivityTest.StartListening();

            Connectivity.ConnectivityChanged += (sender, args) =>
            {
                if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    MessagingCenter.Send<App>(this, "connect");
                }

                if (Connectivity.NetworkAccess == NetworkAccess.None)
                {
                    MessagingCenter.Send<App>(this, "no_connect");
                }

            };
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
