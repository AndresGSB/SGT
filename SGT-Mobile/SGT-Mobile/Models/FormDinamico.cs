using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace SGTMobile.Models
{
    public class FormDinamico
    {
        public string Nombre { get; set; }
        public Label label { get; set; }
        public Object entry { get; set; }
        public TypeObject tipo { get; set; }
    }

    public enum TypeObject
    {
        Entry = 1,
        Date = 2,
        Time = 3,
        Multi = 4,
        Radio = 5
    }
}
