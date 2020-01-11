using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Couchbase;
using Couchbase.Authentication;
using Couchbase.Configuration.Client;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.Linq;
using HelloCouch.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HelloCouch
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
            //CouchBase configuration using Dependency Injection
            services
                .AddCouchbase(Configuration.GetSection("Couchbase"))
                .AddCouchbaseBucket<IContactsBucketProvider>("contacts");
            services.AddTransient(x =>
            {
                var contactsBucket = x.GetRequiredService<IContactsBucketProvider>();
                return new BucketContext(contactsBucket.GetBucket());
            });

            //Without Dependency Injection
            //ClusterHelper.Initialize(new ClientConfiguration
            //{
            //    Servers = new List<Uri>
            //    {
            //        new Uri("couchbase://localhost")
            //    },
            //    UseSsl = false
            //}, new PasswordAuthenticator("hellocouchappuser", "P@ssw0rd"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime hostApplicationLifetime)
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

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            hostApplicationLifetime.ApplicationStopped.Register(() =>
            {
                //Cleaning up using using Dependency Injection
                app.ApplicationServices.GetRequiredService<ICouchbaseLifetimeService>().Close();
                //Without Dependency Injection
                //ClusterHelper.Close();
            });
        }
    }
}
