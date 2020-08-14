using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SGT.Web.Models
{
    public class Ticket
    {
        public int ID { get; set; }

        public string Titulo { get; set; }
        public string ClientTicket { get; set; }
        public string Client { get; set; }
        public string Account { get; set; }

        public string POC_1 { get; set; }
        public string Phone_POC_1 { get; set; }
        public string Email_POC_1 { get; set; }

        public string Final_User { get; set; }
        public string Final_User_Phone { get; set; }
        public string Final_User_Email { get; set; }

        public DateTime Service_Date { get; set; }
        public Sitio Site_Name { get; set; }

        public File Attachment { get; set; }

        public string Service_Details_short_Activity { get; set; }

        public string FE_Name { get; set; }

        public string Report_Status_Mobile { get; set; }

        public string Client_Intertal { get; set; }

    }
}