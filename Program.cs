using ASM_SIMS.DB;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ASM_SIMS
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<ASM_SIMS.DB.SimsDataContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // connect to database
            var provider = builder.Services.BuildServiceProvider();
            var configuration = provider.GetRequiredService<IConfiguration>();
            builder.Services.AddDbContext<SimsDataContext>(item =>
            {
                item.UseSqlServer(configuration.GetConnectionString("connection"));
            });

            var app = builder.Build();

            // Kh?i t?o t�i kho?n m?c ??nh
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<SimsDataContext>();
                await SeedDefaultAdmin(dbContext);
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Login}/{action=Index}/{id?}"); // khi ch?y d? �n th� s? ch?y login ??u ti�n 

            app.Run();
        }
        private static async Task SeedDefaultAdmin(SimsDataContext dbContext)
        {
            // Ki?m tra xem c� t�i kho?n n�o trong database kh�ng
            if (!dbContext.Accounts.Any())
            {
                var defaultAdmin = new Account
                {
                    RoleId = 1, // Admin
                    Username = "admin",
                    Password = "admin123", // N�n m� h�a trong th?c t?
                    Email = "admin@sims.com",
                    Phone = "1234567890",
                    Address = "Default Admin Address",
                    CreatedAt = DateTime.Now
                };

                dbContext.Accounts.Add(defaultAdmin);
                try
                {
                    await dbContext.SaveChangesAsync();
                    Console.WriteLine("Default Admin account created successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating default admin: {ex.Message}");
                }
            }
        }
    }
}
