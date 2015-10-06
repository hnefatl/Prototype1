using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace WebService
{
    public class MvcApplication
        : System.Web.HttpApplication
    {
        public override void Init()
        {
            AuthenticateRequest += OnAuthenticateRequest;

            base.Init();
        }

        protected void OnAuthenticateRequest(object sender, EventArgs e)
        {
            if (HttpContext.Current.User != null && HttpContext.Current.User.Identity.IsAuthenticated)
            {
                HttpCookie Cookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                FormsAuthenticationTicket Ticket = FormsAuthentication.Decrypt(Cookie.Value);

                HttpContext.Current.User = new GenericPrincipal(HttpContext.Current.User.Identity, new string[] { Ticket.UserData });
            }
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            GlobalConfiguration.Configuration.Filters.Add(new System.Web.Http.AuthorizeAttribute());

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);


        }
    }
}