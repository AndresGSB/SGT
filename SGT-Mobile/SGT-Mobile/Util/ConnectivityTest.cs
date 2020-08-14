using SGT_Mobile;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SGTMobile.Util
{
    public static class ConnectivityTest
    {
        public static void StartListening()
        {
            // Register for connectivity changes
            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
        }

        public static void StopListening()
        {
            // Un-register listener for changes
            Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;
        }

        async static void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            var access = e.NetworkAccess;
            //MessagingCenter.Send<App>(this, access);

        }
    }
}
