using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BankersCup.Helpers
{
    public class RegistrationCookieValues
    {
        public int TeamId { get; set; }
        public int PlayerId { get; set; }
        private static readonly RegistrationCookieValues INVALID_REGISTRATION = new RegistrationCookieValues(-1, -1);

        public RegistrationCookieValues(int teamId, int playerId)
        {
            this.TeamId = teamId;
            this.PlayerId = playerId;
        }

        public static RegistrationCookieValues InvalidValues
        {
            get
            {
                return INVALID_REGISTRATION;
            }
        }
    }
}