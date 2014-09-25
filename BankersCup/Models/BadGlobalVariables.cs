using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BankersCup.Models
{
    public static class BadGlobalVariables
    {
        public static readonly List<AdViewModel> Ads = new List<AdViewModel>() {
            new AdViewModel() {
                ImageUrl = "/Content/ads/app_migration.png",
                Topic = "App Migration"
            },
            new AdViewModel() {
                ImageUrl = "/Content/ads/Azure_Governance.png",
                Topic = "Azure Governance"
            },
            new AdViewModel() {
                ImageUrl = "/Content/ads/disruptive_innovation.png",
                Topic = "Disruptive Innovation"
            },
            new AdViewModel() {
                ImageUrl = "/Content/ads/modern_app_banner.png",
                Topic = "Modern UI Apps"
            },
            new AdViewModel() {
                ImageUrl = "/Content/ads/omnichannel.png",
                Topic = "Omnichannel"
            }
        };
    }
}