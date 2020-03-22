using AutoWrapper.Helpers;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace AutoWrapper.Filters
{
    public class RequestDataLogIgnore: Attribute, IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            context.HttpContext.Request.Headers.Add(TypeIdentifier.ShouldLogRequestDataFilterHeader, new string[] { "false" });
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // our code after action executes
        }
    }
}
