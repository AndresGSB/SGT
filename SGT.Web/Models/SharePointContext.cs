using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Web;
using iText.Forms;
using iText.Kernel.Pdf;
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

        public IEnumerable<Ticket> Tickets(string Fe_name)
        {
            var web = _context.Web;
            _context.Load(web.Lists);
            _context.ExecuteQuery();

            var Lista = web.Lists.GetByTitle("LST_Tickets_20");
            //CamlQuery query = CamlQuery.CreateAllItemsQuery();
            CamlQuery query = new CamlQuery();
            query.ViewXml = $"<View><Query><Where><Eq><FieldRef Name='FE_Name' /><Value Type='Text'>{Fe_name}</Value></Eq></Where></Query></View>";
            ListItemCollection tikets = Lista.GetItems(query);

            _context.Load(tikets);
            _context.ExecuteQuery();

            TimeZoneInfo mxZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)");

            foreach (var tk in tikets)
            {
                var sd = tk["Service_Date"] as Nullable<DateTime>;
                sd = TimeZoneInfo.ConvertTimeFromUtc(sd.Value, mxZone);
                var has = tk["Attachments"] as Boolean?;
                var site = tk["Site_Name"] as FieldLookupValue;
                yield return new Ticket
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
                    Site_Name = SitioById(site.LookupId),
                    Report_Status_Mobile = tk["Report_Status_Mobile"] as string,
                    Client_Intertal = tk["SO__x0028_Curvature_x0029_"] as string,
                    HasAttachment = (Boolean)has
                };
            }
        }

        public Ticket Ticket(int ID)
        {
            var web = _context.Web;
            _context.Load(web.Lists);
            _context.ExecuteQuery();

            var Lista = web.Lists.GetByTitle("LST_Tickets_20");
            CamlQuery query = new CamlQuery();
            query.ViewXml = $"<View><Query><Where><Eq><FieldRef Name='ID' /><Value Type='Number'>{ID}</Value></Eq></Where></Query></View>";
            ListItemCollection tikets = Lista.GetItems(query);

            _context.Load(tikets);
            _context.ExecuteQuery();

            TimeZoneInfo mxZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)");

            foreach (var tk in tikets)
            {
                var sd = tk["Service_Date"] as Nullable<DateTime>;
                sd = TimeZoneInfo.ConvertTimeFromUtc(sd.Value, mxZone);
                var site = tk["Site_Name"] as FieldLookupValue;

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

        public IEnumerable<RecursoIngeniero> FE()
        {
            var web = _context.Web;
            _context.Load(web.Lists);
            _context.ExecuteQuery();

            var Lista = web.Lists.GetByTitle("LST_RecursosIngenieria");
            CamlQuery query = CamlQuery.CreateAllItemsQuery();
            ListItemCollection Ingenieros = Lista.GetItems(query);


            _context.Load(Ingenieros);
            _context.ExecuteQuery();

            foreach (var fe in Ingenieros)
            {
                var estatus = fe["Estatus"] as string;
                var aprobado = fe["Aprovado"] as Nullable<bool>;
                if (estatus == "Activo" && aprobado == true)
                {
                    yield return new RecursoIngeniero
                    {
                        ID = fe.Id,
                        Name = fe["Nombre"] as string,
                        Email = fe["Email"] as string,
                        Password = fe["Password_Mobile"] as string,
                        MobielApp = fe["Mobile_App"] as Nullable<bool>
                    };
                }
                
            }
        }

        public Stream TemplatePDF(string Account)
        {
            var web = _context.Web;
            _context.Load(web.Lists);
            _context.ExecuteQuery();

            var Lista = web.Lists.GetByTitle("CAT_Cuenta");
            CamlQuery query = new CamlQuery();
            query.ViewXml = $"<View><Query><Where><Eq><FieldRef Name='Nombre' /><Value Type='Text'>{Account}</Value></Eq></Where></Query></View>";
            //CamlQuery query = CamlQuery.CreateAllItemsQuery();
            ListItemCollection Accounts = Lista.GetItems(query);

            _context.Load(Accounts);
            _context.ExecuteQuery();

            foreach (var ct in Accounts)
            {
                if(Account == ct["Nombre"] as string)
                {
                    //Get the Site Collection
                    Site oSite = _context.Site;
                    _context.Load(oSite);
                    _context.ExecuteQuery();

                    var hasAttach = ct["Attachments"] as Boolean?;
                    if ((Boolean)hasAttach)
                    {

                        var url = oSite.Url + "/Lists/CAT_Cuenta/attachments/" + ct.Id;
                        Folder folder = web.GetFolderByServerRelativeUrl(url);
                        _context.Load(folder);
                        _context.ExecuteQuery();

                        FileCollection attachments = folder.Files;
                        _context.Load(attachments);
                        _context.ExecuteQuery();


                        foreach (Microsoft.SharePoint.Client.File oFile in folder.Files)
                        {
                            FileInformation fileInfo = Microsoft.SharePoint.Client.File.OpenBinaryDirect(_context, oFile.ServerRelativeUrl);

                            var stream = fileInfo.Stream;

                            return fileInfo.Stream;

                        }

                    }
                }
                
            }

            return new MemoryStream();
        }

        public Stream TemplatePDFBarrister(int id)
        {
            var web = _context.Web;
            _context.Load(web.Lists);
            _context.ExecuteQuery();

            var Lista = web.Lists.GetByTitle("LST_Tickets_20");
            CamlQuery query = new CamlQuery();
            query.ViewXml = $"<View><Query><Where><Eq><FieldRef Name='ID' /><Value Type='Number'>{id}</Value></Eq></Where></Query></View>";
            //CamlQuery query = CamlQuery.CreateAllItemsQuery();
            ListItemCollection Accounts = Lista.GetItems(query);

            _context.Load(Accounts);
            _context.ExecuteQuery();

            foreach (var ct in Accounts)
            {
                var hasatt = ct["Attachments"] as Boolean?;
                if (hasatt == true)
                {
                    //Get the Site Collection
                    Site oSite = _context.Site;
                    _context.Load(oSite);
                    _context.ExecuteQuery();

                    var url = oSite.Url + "/Lists/LST_Tickets_20/attachments/" + ct.Id;
                    Folder folder = web.GetFolderByServerRelativeUrl(url);
                    _context.Load(folder);
                    _context.ExecuteQuery();

                    FileCollection attachments = folder.Files;
                    _context.Load(attachments);
                    _context.ExecuteQuery();


                    foreach (Microsoft.SharePoint.Client.File oFile in folder.Files)
                    {
                        FileInformation fileInfo = Microsoft.SharePoint.Client.File.OpenBinaryDirect(_context, oFile.ServerRelativeUrl);
                        if(oFile.Name.ToLower().Contains("blank") && oFile.Name.Contains(".pdf"))
                            return fileInfo.Stream;

                    }
                }

            }

            return new MemoryStream();
        }

        public bool SaveReportByTicket(DocumentSent doc)
        {
            var web = _context.Web;
            _context.Load(web.Lists);
            _context.ExecuteQuery();

            var Lista = web.Lists.GetByTitle("LST_Tickets_20");
            //CamlQuery query = new CamlQuery();
            //query.ViewXml = $"<View><Query><Where><Eq><FieldRef Name='ID' /><Value Type='Number'>{doc.IdTicket}</Value></Eq></Where></Query></View>";
            //ListItemCollection tickets = Lista.GetItems(query);
            ListItem item = Lista.GetItemById(doc.IdTicket);
            _context.Load(item);
            _context.ExecuteQuery();

            string path = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath(@"~/App_Data"), doc.NameFile);
            using (FileStream outputFileStream = new FileStream(path, FileMode.Create))
            {
                doc.Content.CopyTo(outputFileStream);
                outputFileStream.Close();
            }

            if (System.IO.File.Exists(path))
            {
                using (PdfReader reader = new PdfReader(path))
                {

                    using (PdfDocument pdf = new PdfDocument(reader))
                    {
                        PdfAcroForm formpdf = PdfAcroForm.GetAcroForm(pdf, true);
                        var fields = formpdf.GetFormFields().Keys;
                        TimeZoneInfo mxZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)");

                        foreach (var f in fields)
                        {
                            var field = formpdf.GetField(f).GetFormType();
                            if (0 == PdfName.Tx.CompareTo(field) && !f.Contains(".1"))
                            {
                                var value = "";
                                var Service_Date = item["Service_Date"] as Nullable<DateTime>;
                                var stringDate = Service_Date.Value.ToString("yyyy/MM/dd");
                                switch (f)
                                {
                                    case "date_Service_Date":
                                        value = formpdf.GetField(f).GetValue().ToString();
                                        if (!string.IsNullOrEmpty(value))
                                            item["Real_Service_Date"] = Service_Date;
                                        break;
                                    case "time_Transit_Time":
                                        value = getTime(formpdf.GetField(f).GetValue().ToString());
                                        if (!string.IsNullOrEmpty(value))
                                            item["FE_Transit_Time"] = TimeZoneInfo.ConvertTimeFromUtc(DateTime.ParseExact(stringDate + " " + value, "yyyy/MM/dd HH:mm:ss", null).ToUniversalTime(), mxZone);
                                        break;
                                    case "time_Time_Arrival":
                                        value = getTime(formpdf.GetField(f).GetValue().ToString());
                                        if (!string.IsNullOrEmpty(value))
                                            item["FE_Time_Arrival"] = TimeZoneInfo.ConvertTimeFromUtc(DateTime.ParseExact(stringDate + " " + value, "yyyy/MM/dd HH:mm:ss", null).ToUniversalTime(), mxZone);
                                        break;
                                    case "time_Departure_Time":
                                        value = getTime(formpdf.GetField(f).GetValue().ToString());
                                        if (!string.IsNullOrEmpty(value))
                                            item["FE_DepartureTime"] = TimeZoneInfo.ConvertTimeFromUtc(DateTime.ParseExact(stringDate + " " + value, "yyyy/MM/dd HH:mm:ss", null).ToUniversalTime(), mxZone);
                                        break;
                                    case "txt_Lunch_Break":
                                        value = formpdf.GetField(f).GetValue().ToString();
                                        if (!string.IsNullOrEmpty(value))
                                            item["Lunch_Break"] = value;
                                        break;
                                }
                            }
                        }
                        pdf.Close();
                    }

                    reader.Close();
                }
            }

            item["Report_Status_Mobile"] = "Pending to GSB";

            var attInfo = new AttachmentCreationInformation();
            attInfo.FileName = doc.NameFile;
            Stream fs = System.IO.File.OpenRead(path);
            attInfo.ContentStream = fs;
            item.AttachmentFiles.Add(attInfo);

            _context.ExecuteQuery();

            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);

            return true;

            
        }

        public bool ChangePasswordFE(RecursoIngeniero fe)
        {
            var web = _context.Web;
            _context.Load(web.Lists);
            _context.ExecuteQuery();

            var Lista = web.Lists.GetByTitle("LST_RecursosIngenieria");
            ListItem Ingeniero = Lista.GetItemById(fe.ID);
            Ingeniero["Password_Mobile"] = fe.Password;
            Ingeniero["Mobile_App"] = true;

            Ingeniero.Update();
            _context.ExecuteQuery();

            return true;
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

        public bool SaveLocalFile()
        {
            return true;
        }

        private string getTime(string time)
        {
            var time_w = time.Substring(0, 5);

            if (time.Contains("a. m.") || time.Contains("a.m.") || time.Contains("AM"))
            {
                return time_w + ":00";
            }
            else
            {
                var tt = TimeSpan.Parse(time_w);
                var horas = tt.Hours + 12 == 24 ? "00:" : tt.Hours + 12 + ":";
                return horas + tt.Minutes.ToString("00") + ":00";
            }
        }


    }
}