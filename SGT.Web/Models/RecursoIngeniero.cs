using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SGT.Web.Models
{
    public class RecursoIngeniero
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool? MobielApp { get; set; } 
    }

    public class RecursoIngenieria_App
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
    }
}