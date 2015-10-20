using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using Data.DataContext;

namespace WebService
{
    public static class Authentication
    {
        public static HttpCookie CreateTicket(string Username, AuthenticationRole Role)
        {
            return new HttpCookie(FormsAuthentication.FormsCookieName,
                FormsAuthentication.Encrypt(new FormsAuthenticationTicket(1, Username, DateTime.Now, DateTime.Now.AddMinutes(30),
                    false, Enum.GetName(typeof(AuthenticationRole), Role), FormsAuthentication.FormsCookiePath)));
        }
        public static AuthenticationRole GetAuthentication()
        {
            foreach(string Role in Enum.GetNames(typeof(AuthenticationRole)))
                if (HttpContext.Current.User.IsInRole(Role))
                    return (AuthenticationRole)Enum.Parse(typeof(AuthenticationRole), Role);

            return AuthenticationRole.Unknown;
        }
    }
}