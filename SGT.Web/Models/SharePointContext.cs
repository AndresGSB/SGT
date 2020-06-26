using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Web;
using Microsoft.SharePoint.Client;

namespace SGT.Web.Models
{
    public class SharePointContext : IDisposable
    {
        private bool disposedValue;
        private ClientContext _context = new ClientContext(_sharepointURL);
        private static string _sharepointURL => ConfigurationManager.AppSettings["SharePointURL"];
        private static string _sharepointUser => ConfigurationManager.AppSettings["SharePointUser"];
        private static string _sharepointPassword => ConfigurationManager.AppSettings["SharePointPassword"];

        private static NetworkCredential credentials = new NetworkCredential(_sharepointUser, _sharepointPassword, "https://gsblat.sharepoint.com");

        public SharePointContext()
        {
            var passWord = new SecureString();
            foreach (char c in _sharepointPassword.ToCharArray()) passWord.AppendChar(c);
            _context.Credentials = new SharePointOnlineCredentials(_sharepointUser, passWord);
        }

        public IEnumerable<Ticket> Tickets()
        {
            var web = _context.Web;
            _context.Load(web.Lists);
            _context.ExecuteQuery();

            var Lista = web.Lists.GetByTitle("LST_Tickets_20_Dev");
            CamlQuery query = CamlQuery.CreateAllItemsQuery();
            ListItemCollection tikets = Lista.GetItems(query);

            _context.Load(tikets);
            _context.ExecuteQuery();

            foreach (var tk in tikets)
            {
                var sd = tk["Service_Date"] as Nullable<DateTime>;
                yield return new Ticket
                {
                    ID = tk.Id,
                    Titulo = tk["Title"] as string,
                    ClientTicket = tk["ClientTicket"] as string,
                    Client = tk["Client"] as string,
                    Account = tk["Account"] as string,
                    Service_Date = (DateTime) sd

                };
            }
        }

        public Ticket Ticket(int ID)
        {
            var web = _context.Web;
            _context.Load(web.Lists);
            _context.ExecuteQuery();

            var listName = "LST_Tickets_20_Dev";

            var Lista = web.Lists.GetByTitle("LST_Tickets_20_Dev");
            CamlQuery query = new CamlQuery();
            query.ViewXml = $"<View><Query><Where><Eq><FieldRef Name='ID' /><Value Type='Number'>{ID}</Value></Eq></Where></Query></View>";
            ListItemCollection tikets = Lista.GetItems(query);

            _context.Load(tikets);
            _context.ExecuteQuery();

            foreach (var tk in tikets)
            {
                //Get the Site Collection
                Site oSite = _context.Site;
                _context.Load(oSite);
                _context.ExecuteQuery();

                var sd = tk["Service_Date"] as Nullable<DateTime>;
                var site = tk["Site_Name"] as FieldLookupValue;

                var hasAttach = tk["Attachments"] as Boolean?;
                if ((Boolean)hasAttach)
                {
                    /*
                    var url = oSite.Url + "/Lists/" + listName + "/attachments/" + tk.Id;
                    Folder folder = web.GetFolderByServerRelativeUrl(url);
                    _context.Load(folder);
                    _context.ExecuteQuery();

                    FileCollection attachments = folder.Files;
                    _context.Load(attachments);
                    _context.ExecuteQuery();

                    foreach (Microsoft.SharePoint.Client.File oFile in folder.Files)
                    {
                        FileInfo myFileinfo = new FileInfo(oFile.Name);
                        WebClient client1 = new WebClient();

                        var credentials = new NetworkCredential(_sharepointUser, _sharepointPassword, "https://gsblat.sharepoint.com/sites/GSB-SGTGlobal");
                        //client1.Credentials = credentials;

                        var fileUrl = "https://gsblat.sharepoint.com" + oFile.ServerRelativeUrl;
                        byte[] fileContents = client1.DownloadData(fileUrl);

                        FileStream fStream = new FileStream(@"C:Temp" +
                              oFile.Name, FileMode.Create);

                        fStream.Write(fileContents, 0, fileContents.Length);
                        fStream.Close();
                    }*/

                }
                

                return new Ticket
                {
                    ID = tk.Id,
                    Titulo = tk["Title"] as string,
                    ClientTicket = tk["ClientTicket"] as string,
                    Client = tk["Client"] as string,
                    Account = tk["Account"] as string,
                    Service_Date = (DateTime)sd,
                    POC_1 = tk["POC_1"] as string,
                    Phone_POC_1 = tk["Phone_POC_1"] as string,
                    Email_POC_1 = tk["Email_POC_1"] as string,
                    Final_User = tk["Final_User"] as string,
                    Final_User_Phone = tk["Final_User_Phone"] as string,
                    Final_User_Email = tk["Final_User_Email"] as string,
                    Service_Details_short_Activity = tk["Service_Details_short_Activity"] as string,
                    Site_Name = SitioById(site.LookupId)

                };
            }

            return new Ticket();
        }

        public Sitio SitioById(int SitioID)
        {
            var web = _context.Web;
            _context.Load(web.Lists);
            _context.ExecuteQuery();

            var Lista = web.Lists.GetByTitle("LST_Sitios");
            CamlQuery query = new CamlQuery();
            query.ViewXml = $"<View><Query><Where><Eq><FieldRef Name='ID' /><Value Type='Number'>{SitioID}</Value></Eq></Where></Query></View>";
            ListItemCollection Sitio = Lista.GetItems(query);

            _context.Load(Sitio);
            _context.ExecuteQuery();

            foreach (var st in Sitio)
            {
                return new Sitio
                {
                    ID = st.Id,
                    Nombre_Sitio = st["Nombre_Sitio"] as string,
                    Cliente = st["S_Cliente"] as string,
                    Cuenta = st["S_Cuenta"] as string,
                    Tipo = st["Tipo"] as string,
                    Direccion = st["Direccion"] as string,
                    URL_Map = st["URL_Map"] as string

                };
            }

            return new Sitio();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _context.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // No cambie este código. Coloque el código de limpieza en el método "Dispose(bool disposing)".
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}