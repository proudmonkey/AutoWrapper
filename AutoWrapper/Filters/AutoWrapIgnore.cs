using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace AutoWrapper.Filters
{
    public class AutoWrapIgnore : Attribute, IResultFilter
    {
        public void OnResultExecuting(ResultExecutingContext context)
        {
            context.HttpContext.Response.Headers.Add("AutoWrapIgnoreFilter", new string[] { "true" });
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
            // Can't add to headers here because response has started.
        }
    }
}
