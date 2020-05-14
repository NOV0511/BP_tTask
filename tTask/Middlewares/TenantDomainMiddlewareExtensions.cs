using Microsoft.AspNetCore.Builder;

namespace tTask.Middlewares

{
    public static class TenantDomainMiddlewareExtensions
    {
        public static IApplicationBuilder UseTenantDomain(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TenantDomainMiddleware>();
        }
    }
}
