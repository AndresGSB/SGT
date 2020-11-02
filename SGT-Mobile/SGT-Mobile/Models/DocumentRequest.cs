using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SGTMobile.Models
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

        public MemoryStream Content { get; set; }
    }
}
