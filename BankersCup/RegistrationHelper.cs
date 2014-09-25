using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BankersCup
{
    public class RegistrationHelper
    {
        private static readonly string registrationCookieName = "GameRegistration{0}";
        public static readonly int InvalidTeamId = -1;

        public static void SetRegistrationCookie(HttpContextBase context, int gameId, int teamId)
        {
            var cookie = new HttpCookie(string.Format(registrationCookieName, gameId), teamId.ToString());
            cookie.Expires = DateTime.Now.AddDays(180);
            context.Response.SetCookie(cookie);
        }

        public static int GetRegistrationCookieValue(HttpContextBase context, int gameId)
        {
            var regCookie = context.Request.Cookies[string.Format(registrationCookieName, gameId)];
            if(regCookie == null)
            {
                return InvalidTeamId;
            }

            if(string.IsNullOrWhiteSpace(regCookie.Value))
            {
                return InvalidTeamId;
            }

            int teamId;
            if (!Int32.TryParse(regCookie.Value, out teamId))
                return InvalidTeamId;

            return teamId;
        }

    }
}