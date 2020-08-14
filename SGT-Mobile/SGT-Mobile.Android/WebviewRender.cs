using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SGT_Mobile.Droid;
using SGTMobile.Util;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(WebViewPDF), typeof(WebviewRender))]
namespace SGT_Mobile.Droid
{
    public class WebviewRender : WebViewRenderer
    {
        public WebviewRender(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<WebView> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                Control.Settings.AllowUniversalAccessFromFileURLs = true;
                Control.Settings.AllowFileAccessFromFileURLs = true;
                Control.Settings.AllowContentAccess = true;
                Control.Settings.AllowFileAccess = true;
            }

        }
    }
}