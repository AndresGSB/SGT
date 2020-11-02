using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace SGT.Web.Models
{
    public class DocumentRequest
    {
        public string Account { get; set; }
        public string Client { get; set; }
        public int IdTicket { get; set; }
    }

    public class DocumentSent
    {
        public int IdTicket { get; set; }
        public string NameFile { get; set; }
        public Stream Content { get; set; }
    }

}