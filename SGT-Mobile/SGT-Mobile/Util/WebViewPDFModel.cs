using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace SGTMobile.Util
{
    public class WebViewPDFModel
    {
        public bool IsPdf { get; set; }
        public string Uri { get; set; }

        public WebViewPDFModel(string uri, bool isPdf)
        {
            Uri = uri;
            IsPdf = isPdf;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}

