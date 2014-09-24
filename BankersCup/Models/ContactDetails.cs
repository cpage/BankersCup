using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BankersCup.Models
{
    public class ContactDetails
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Company { get; set; }
        public string Topic { get; set; }
        public string EventName { get; set; }
        public DateTime DateCreated { get; set; }
        public bool Consent { get; set; }
    }
}