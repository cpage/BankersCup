using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BankersCup.Filters
{
    public class AdminAccessRequiredAttribute : ActionFilterAttribute, IActionFilter
    {

        void IActionFilter.OnActionExecuted(ActionExecutedContext filterContext)
        {
            
        }

        void IActionFilter.OnActionExecuting(ActionExecutingContext filterContext)
        {
            if(string.Compare(filterContext.ActionDescriptor.ActionName, "Authorize", true) == 0)
            {
                return;
            }

            if(filterContext.HttpContext.Request.Cookies["AdminAccess"] == null)
            {
                filterContext.Result = new RedirectToRouteResult(new System.Web.Routing.RouteValueDictionary(new { controller = "Admin", action = "Authorize" }));
            }
        }
    }
}