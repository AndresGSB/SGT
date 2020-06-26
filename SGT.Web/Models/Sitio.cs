using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SGT.Web.Models
{
    public class Sitio
    {
        public int ID { get; set; }
        public string Nombre_Sitio { get; set; }
        public string Cliente { get; set; }
        public string Cuenta { get; set; }
        public string Tipo { get; set; }
        public string Direccion { get; set; }
        public string URL_Map { get; set; }
        public bool Activo { get; set; }

    }
}