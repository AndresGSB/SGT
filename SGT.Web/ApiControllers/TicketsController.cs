using Microsoft.Ajax.Utilities;
using SGT.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;

namespace SGT.Web.Controllers
{
    [Authorize]
    [RoutePrefix("api/tickets")]
    public class TicketsController : ApiController
    {

        [HttpGet]
        [Route("getFE")]
        public IEnumerable<Ticket> GetTicketsFE()
        {
            var identity = Thread.CurrentPrincipal.Identity;
            var Fe_name = identity.Name.Split('|')[1];
            using (var sp = new SharePointContext())
            {
                TimeZoneInfo mxZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)");
                var today = DateTime.UtcNow;
                var mxDate = TimeZoneInfo.ConvertTimeFromUtc(today, mxZone);
                var tickets_FE = sp.Tickets(Fe_name).Where(x => x.Service_Date.Date == mxDate.Date).ToList();
                //filtrar tickets del dia 
                return tickets_FE;
            }
        }

        // GET: api/Tickets/5
        public Ticket Get(int id)
        {
            using (var sp = new SharePointContext())
            {
                return sp.Ticket(id);
            }
        }

        [HttpPost]
        [Route("getReportTemplate")]
        public HttpResponseMessage GetReportTemplate(DocumentRequest doc)
        {

            using (var sp = new SharePointContext())
            {
                var stream = sp.TemplatePDF(doc.Account);

                if (!stream.CanSeek)
                {
                    HttpResponseMessage httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK);
                    httpResponseMessage.Content = new StreamContent(stream);
                    httpResponseMessage.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
                    httpResponseMessage.Content.Headers.ContentDisposition.FileName = doc.Account + "_Template";
                    httpResponseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                    return httpResponseMessage;

                }
                else
                {
                    HttpResponseMessage httpResponseMessage = Request.CreateResponse(HttpStatusCode.BadRequest);
                    return httpResponseMessage;
                }


            }
        }

        [HttpPost]
        [Route("getReportTemplateBAM")]
        public HttpResponseMessage GetReportTemplateBAM(DocumentRequest doc)
        {

            using (var sp = new SharePointContext())
            {
                var stream = sp.TemplatePDFBarrister(doc.IdTicket);

                if (!stream.CanSeek)
                {
                    HttpResponseMessage httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK);
                    httpResponseMessage.Content = new StreamContent(stream);
                    httpResponseMessage.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
                    httpResponseMessage.Content.Headers.ContentDisposition.FileName = doc.Account + "_Template";
                    httpResponseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                    return httpResponseMessage;

                }
                else
                {
                    HttpResponseMessage httpResponseMessage = Request.CreateResponse(HttpStatusCode.BadRequest);
                    return httpResponseMessage;
                }


            }
        }

        [HttpPost]
        [Route("sendReport")]
        public HttpResponseMessage sendReport()
        {
            var file = HttpContext.Current.Request.Files.Count > 0 ? HttpContext.Current.Request.Files[0] : null;

            var ticket = HttpContext.Current.Request.Params["obj"];

            try
            {
                using (var sp = new SharePointContext())
                {
                    var saved = sp.SaveReportByTicket(new DocumentSent { IdTicket = Int32.Parse(ticket), Content = file.InputStream, NameFile = file.FileName });

                    if (saved)
                    {
                        HttpResponseMessage httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK);
                        return httpResponseMessage;

                    }
                    else
                    {
                        HttpResponseMessage httpResponseMessage = Request.CreateResponse(HttpStatusCode.BadRequest, "No se guardo");
                        return httpResponseMessage;
                    }


                }
            }
            catch (Exception ex)
            {
                HttpError err = new HttpError(ex.Message);
                HttpResponseMessage httpResponseMessage = Request.CreateResponse(HttpStatusCode.InternalServerError, err);
                return httpResponseMessage;
            }

            
        }
    }
}
