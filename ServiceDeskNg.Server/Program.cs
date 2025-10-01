
using Microsoft.EntityFrameworkCore;
using ServiceDeskNg.Server.Data;

namespace ServiceDeskNg.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // servicio de base de datos
            builder.Services.AddDbContext<ServiceDeskContext>(options =>
                options.UseMySql(
                    builder.Configuration.GetConnectionString("ServiceDeskDB"),
                    new MySqlServerVersion(new Version(8, 0, 41)) // tu versión de MySQL
                )
            );

            // Add services to the container.

        
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();
            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            app.UseDefaultFiles();
            app.MapStaticAssets();

           

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ServiceDeskContext>();
                if (context.Database.CanConnect())
                {
                    Console.WriteLine(" Conexión a la base de datos exitosa.");
                }
                else
                {
                    Console.WriteLine(" No se pudo conectar a la base de datos.");
                }
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.MapFallbackToFile("/index.html");

            app.Run();
        }
    }
}
