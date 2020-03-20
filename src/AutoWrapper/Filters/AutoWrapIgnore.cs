using AutoWrapper.Helpers;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace AutoWrapper.Filters
{
    public class AutoWrapIgnore : Attribute, IActionFilter
    {
        public bool ShouldLogRequestData{ get; set; } = true;
        public AutoWrapIgnore(){}
        public void OnActionExecuting(ActionExecutingContext context)
        {
            context.HttpContext.Request.Headers.Add(TypeIdentifier.AutoWrapIgnoreFilterHeader, new string[] { "true" });

            if (!ShouldLogRequestData)
            {
                context.HttpContext.Request.Headers.Add(TypeIdentifier.ShouldLogRequestDataFilterHeader, new string[] { "false" });
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // our code after action executes
        }
    }
}
