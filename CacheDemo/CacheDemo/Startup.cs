using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CacheDemo
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
            services.AddControllersWithViews();
            //Sử dụng Redis Cache
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = "127.0.0.1:6379";
            });
            //Sql server Cache
            //services.AddDistributedSqlServerCache(options =>
            //{
            //    options.ConnectionString = @"Data Source=DESKTOP-L0RDFL3\SQLEXPRESS;Initial Catalog=DistCache;Integrated Security=True;";
            //    options.SchemaName = "dbo";
            //    options.TableName = "TestCache1";
            //});
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //Sử dụng Response Cache
            app.UseResponseCaching();

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
            app.UseStaticFiles(new StaticFileOptions()
            {
                OnPrepareResponse = ctx =>
                {
                    // Cache static files for 30 days
                    ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=2592000");
                    ctx.Context.Response.Headers.Append("Expires", DateTime.UtcNow.AddDays(30).ToString("R", CultureInfo.InvariantCulture));
                }
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
