using CecoChat.Cassandra;
using CecoChat.Materialize.Server.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CecoChat.Materialize.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCassandra(Configuration.GetSection("Cassandra"));
            services.AddSingleton<IDbInitializer, DbInitializer>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            IDbInitializer db = app.ApplicationServices.GetRequiredService<IDbInitializer>();
            db.Initialize();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
        }
    }
}