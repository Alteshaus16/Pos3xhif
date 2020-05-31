using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WahlfachAnmeldung.Models;

namespace WahlfachAnmeldung
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
            services.AddDbContext<RegistrationContext>();
            var razorService = services.AddRazorPages();
            // Damit eine Live�nderung der cshtml Dateien m�glich ist (nach Aktualisieren des Browsers),
            // wird die Runtime Compilation im Debugmodus aktiviert.
#if DEBUG
            // Nuget: Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation 
            razorService.AddRazorRuntimeCompilation();
#endif
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
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                // Damit auch API Controller ber�cksichtigt werden (CSV Export)
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}
