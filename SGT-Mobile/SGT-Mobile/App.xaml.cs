using SGTMobile;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SGT_Mobile
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage( new MasterDetailPageApp());
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
