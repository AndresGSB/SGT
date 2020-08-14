using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using SGT.Web.Models;
using SGT.Model;
using SGT.Web.Util;

namespace SGT.Web.ApiControllers
{
    [AllowAnonymous]
    [RoutePrefix("api/login")]
    public class LoginController : ApiController
    {
        List<RecursoIngeniero> Ingenieros;
        private SGTContext db = new SGTContext();

        [HttpPost]
        [Route("login")]
        public IHttpActionResult Login(LoginRequest login)
        {

            if (login == null)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            //No existe el email en SGT
            if (!checkFEEmail(login.Email))
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            //Esta registrado en SGT
            if (!checkFEMobileApp(login.Email))
                throw new HttpResponseException(HttpStatusCode.Forbidden);

            using (var sp = new SharePointContext())
            {
                var fes = Ingenieros.Where(x => x.Email == login.Email && x.Password == Encrypt.GetSHA256(login.Password) && x.MobielApp == true).FirstOrDefault();
                if (fes == null)
                {
                    throw new HttpResponseException(HttpStatusCode.BadRequest);
                }

                var token = TokenGenerator.GenerateTokenJwt(fes.Email + "|"+ fes.Name);

                return Ok(new RecursoIngenieria_App { ID = fes.ID , Name = fes.Name, Email = fes.Email , Token = token});
            }
        }

        [HttpPost]
        [Route("register")]
        public IHttpActionResult Register(LoginRequest login)
        {
            if (login == null || login.Email == "" || login.Password== "" )
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            //No existe el email en SGT
            if (!checkFEEmail(login.Email))
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            //Esta registrado en SGT
            if (checkFEMobileApp(login.Email))
                throw new HttpResponseException(HttpStatusCode.Forbidden);

            using (var sp = new SharePointContext())
            {
                this.Ingenieros = sp.FE().ToList();
                var fes = Ingenieros.Where(x => x.Email.ToLower() == login.Email).FirstOrDefault();
                if (fes != null)
                {
                    fes.Password = Encrypt.GetSHA256(login.Password);
                    sp.ChangePasswordFE(fes);
                }
            }

            return Ok();
        }

        private bool checkFEEmail(string email)
        {
            using (var sp = new SharePointContext())
            {
                this.Ingenieros = sp.FE().ToList();
                var fes = Ingenieros.Where(x => x.Email.ToLower() == email).FirstOrDefault();

                if (fes != null)
                {
                    return true;
                }
            }
            return false;
        }

        private bool checkFEMobileApp(string email)
        {
            var fe = Ingenieros.Where(x => x.Email.ToLower() == email).FirstOrDefault();
            if (fe.MobielApp == true)
            {
                return true;
            }
            return false;
        }

    }
}
