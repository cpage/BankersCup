using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BankersCup.Filters
{
    public class RegistrationRequiredAttribute : ActionFilterAttribute, IActionFilter
    {

        void IActionFilter.OnActionExecuted(ActionExecutedContext filterContext)
        {
            
        }

        void IActionFilter.OnActionExecuting(ActionExecutingContext filterContext)
        {
            object id;
            filterContext.ActionParameters.TryGetValue("id", out id);

            int gameId;
            if(id == null || !Int32.TryParse(id.ToString(), out gameId))
            {
                return;
            }

            var registeredTeamId = RegistrationHelper.GetRegistrationCookieValue(filterContext.HttpContext, gameId);
            
            if (registeredTeamId == RegistrationHelper.InvalidTeamId)
                filterContext.Result = new RedirectToRouteResult(new System.Web.Routing.RouteValueDictionary(new { controller = "Game", action = "Join", id = gameId }));
        }
    }
}