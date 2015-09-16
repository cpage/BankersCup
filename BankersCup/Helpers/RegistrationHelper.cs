using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BankersCup.Helpers
{
    public class RegistrationHelper
    {
        private static readonly string registrationCookieName = "GameRegistration{0}";
        private static readonly string registrationCookieValue = "{0}-{1}";
        public static readonly int InvalidTeamId = -1;

        public static void SetRegistrationCookie(HttpContextBase context, int gameId, int teamId, int playerId)
        {
            var cookie = new HttpCookie(string.Format(registrationCookieName, gameId), string.Format(registrationCookieValue, teamId, playerId));
            cookie.Expires = DateTime.Now.AddDays(180);
            context.Response.SetCookie(cookie);
        }

        public static RegistrationCookieValues GetRegistrationCookieValue(HttpContextBase context, int gameId)
        {
            var regCookie = context.Request.Cookies[string.Format(registrationCookieName, gameId)];
            if(regCookie == null)
            {
                return RegistrationCookieValues.InvalidValues;
            }

            if(string.IsNullOrWhiteSpace(regCookie.Value))
            {
                return RegistrationCookieValues.InvalidValues;
            }

            int teamId;
            int playerId;
            string[] rawCookieValues = regCookie.Value.Split('-');

            if (rawCookieValues.Length != 2 || !Int32.TryParse(rawCookieValues[0], out teamId) || !Int32.TryParse(rawCookieValues[1], out playerId))
                return RegistrationCookieValues.InvalidValues;


            return new RegistrationCookieValues(teamId, playerId);
        }

    }
}