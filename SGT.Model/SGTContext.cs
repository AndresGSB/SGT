using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGT.Model
{
    public partial class SGTContext : DbContext
    {
        public SGTContext(string cs) : base(cs)
        {

        }
    }
}
