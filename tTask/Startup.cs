using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using tTask.Middlewares;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using tTask.ORM;
using tTask.ORM.DAO;
using tTask.ORM.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Rewrite;

namespace tTask
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddRazorPages().AddRazorRuntimeCompilation();

            services.AddDbContext<SharedDbContext>(optionsAction =>
                optionsAction.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
            );

            services.AddSecondIdentity<GlobalUser, Role>(config =>
            {
                config.Password.RequireDigit = false;
                config.Password.RequiredLength = 8;
                config.Password.RequireUppercase = false;
                config.Password.RequireLowercase = false;
                config.Password.RequireNonAlphanumeric = false;
            })
                .AddEntityFrameworkStores<SharedDbContext>()
                .AddDefaultTokenProviders();

            services.AddDbContext<AppDbContext>();
            services.AddIdentity<User, Role>(config =>
            {
                config.Password.RequireDigit = false;
                config.Password.RequiredLength = 8;
                config.Password.RequireUppercase = false;
                config.Password.RequireLowercase = false;
                config.Password.RequireNonAlphanumeric = false;
            })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("TenantPolicy", policy =>
                    policy.Requirements.Add(new TenantPolicyRequirement()));
            });

            services.AddScoped<IAuthorizationHandler, TenantPolicyHandler>();

            services.AddScoped<Func<bool, SharedDbContext>>((provider) => (isNewTenantCon => new SharedDbContext(isNewTenantCon, provider.GetService<IConfiguration>())));
            


            services.ConfigureApplicationCookie(config =>
            {
                config.Cookie.Name = "Identity.Cookie";
                config.LoginPath = "/default/Home/LoginRedirect";
                config.AccessDeniedPath = "/default/Home/AccessDenied";
                config.ExpireTimeSpan = TimeSpan.FromDays(30);
            });

            services.AddControllersWithViews();

            services.AddHttpContextAccessor();
            services.AddScoped<TenantTable>();
            services.AddScoped<UserTable>();
            services.AddScoped<UserTaskTable>();
            services.AddScoped<ServiceOrderTable>();
            services.AddScoped<PaymentTable>();
            services.AddScoped<NotificationTable>();

            services.AddScoped<ProjectTable>();
            services.AddScoped<TaskTable>();
            services.AddScoped<TaskUserCommentTable>();
            services.AddScoped<NewTenantProcedure>();
            services.AddScoped<Helper>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseTenantDomain();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            var options = new RewriteOptions();
            options.Rules.Add(new NonWwwRule());

            app.UseRewriter(options);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{domain=default}/{controller=Home}/{action=Index}/{id?}");

            });

        }
    }
}
