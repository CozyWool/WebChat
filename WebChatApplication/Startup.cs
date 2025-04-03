using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using WebChatApplication.DataAccess.Contexts;
using WebChatApplication.DataAccess.Repositories;
using WebChatApplication.Extensions;
using WebChatApplication.Services;

namespace WebChatApplication;

public class Startup(IConfiguration configuration)
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services.AddAutoMapper(typeof(Startup).Assembly);

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();

        services.AddFluentEmail(configuration);
        services.AddScoped<IEmailService, EmailService>();

        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = new PathString("/account/login");
                options.LogoutPath = new PathString("/account/logout");
                options.AccessDeniedPath = new PathString("/account/access-denied");
                options.Cookie.HttpOnly = true;
            });

        services.AddMvc();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseStaticFiles();
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(name: "default",
                pattern: "{controller=WebChat}/{action=Index}/{id?}");
        });
    }
}