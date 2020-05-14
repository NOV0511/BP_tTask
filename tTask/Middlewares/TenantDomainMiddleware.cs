using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace tTask.Middlewares
{
    public class TenantDomainMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantDomainMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {

            var tenantDomain = string.Empty;

            string host = context.Request.Host.Host;

            if (!string.IsNullOrWhiteSpace(host))
            {
                tenantDomain = host.Split('.')[0] == "www" ? host.Split('.')[1] : host.Split('.')[0];
            }

            tenantDomain = tenantDomain.Trim().ToLower();

            if (tenantDomain == "ttask" || tenantDomain == "localhost") tenantDomain = "default";

            //context.Items["domain"] = tenantDomain;


            string url = context.Request.Path.Value;
            var domain = url.Split('/')[1] == string.Empty ? "default" : url.Split('/')[1];
            context.Items["domain"] = domain;


            await _next(context);
        }
    }
}
