using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SGTMobile
{

    public class MenuItem
    {
        public MenuItem()
        {
            TargetType = typeof(MenuItem);
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public Page Pagina { get; set; }
        public Type TargetType { get; set; }

        public string Imagen { get; set; }
    }
}