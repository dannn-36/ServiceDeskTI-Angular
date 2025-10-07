using Microsoft.EntityFrameworkCore;
using ServiceDeskNg.Server.Data;
using ServiceDeskNg.Server.Models;
using ServiceDeskNg.Server.Repositories; // <-- agrega esto
using ServiceDeskNg.Server.Repositories.Interfaces; // <-- y esto si usas interfaces

namespace ServiceDeskNg.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ======================================================
            // 🔹 CONFIGURAR BASE DE DATOS
            // ======================================================
            builder.Services.AddDbContext<ServiceDeskContext>(options =>
                options.UseMySql(
                    builder.Configuration.GetConnectionString("ServiceDeskDB"),
                    new MySqlServerVersion(new Version(8, 0, 41))
                )
            );

            // ======================================================
            // 🔹 INYECTAR REPOSITORIOS 
            // ======================================================
            builder.Services.AddScoped<ICrudRepository<Usuario>, UsuarioRepository>();


            // ======================================================
            // 🔹 CONFIGURAR CONTROLADORES Y VISTAS
            // ======================================================
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();
            builder.Services.AddControllers();

            // ======================================================
            // 🔹 OPENAPI (Swagger)
            // ======================================================
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // ======================================================
            // 🔹 ARCHIVOS ESTÁTICOS Y RUTAS BASE
            // ======================================================
            app.UseDefaultFiles();
            app.MapStaticAssets();

            // ======================================================
            // 🔹 VERIFICACIÓN DE CONEXIÓN A LA BD
            // ======================================================
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ServiceDeskContext>();
                if (context.Database.CanConnect())
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("[OK] Conexión a la base de datos exitosa.");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[ERROR] No se pudo conectar a la base de datos.");
                    Console.ResetColor();
                }
            }

            // ======================================================
            // 🔹 PIPELINE DE MIDDLEWARE
            // ======================================================
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.MapFallbackToFile("/index.html");

            app.Run();
        }
    }
}
