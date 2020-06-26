using SGT.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SGT.Web.Controllers
{
    public class TicketsController : ApiController
    {
        // GET: api/Tickets
        public IEnumerable<Ticket> Get()
        {
            using (var sp = new SharePointContext())
            {
                return sp.Tickets();
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

        // POST: api/Tickets
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Tickets/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Tickets/5
        public void Delete(int id)
        {
        }
    }
}
