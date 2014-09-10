using BankersCup.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace BankersCup
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static readonly List<Team> Teams = new List<Team>()
        {
            new Team() {
                RegistrationCode = "12345",
                TeamId = 1,
                TeamName = "Infusion",
                Players = new List<Player>() {
                    new Player() {
                        Company = "Infusion",
                        Email = "bbaldasti@infusion.com",
                        Name = "Bill Baldasti"
                    },
                    new Player() {
                        Company = "Infusion",
                        Email = "sellis@infusion.com",
                        Name = "Steve Ellis"
                    },
                    new Player() {
                        Company = "Infusion",
                        Email = "cpage@infusion.com",
                        Name = "Chris Page"
                    }
                }
            },
            new Team() {
                RegistrationCode = "67890",
                TeamId = 2,
                TeamName = "Indigo",
                Players = new List<Player>() {
                    new Player() {
                        Company = "Indigo",
                        Email = "bpo@indigo.ca",
                        Name = "Bo P"
                    },
                    new Player() {
                        Company = "Indigo",
                        Email = "mtolley@indigo.ca",
                        Name = "Mark Tolley"
                    },
                    new Player() {
                        Company = "Indigo",
                        Email = "sgoneau@indigo.ca",
                        Name = "Stephen Goneau"
                    }
                }
            }

        };

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
