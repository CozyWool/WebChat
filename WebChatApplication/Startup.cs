using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using WebChatApplication.DataAccess.Contexts;

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

        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = new PathString(CookieAuthenticationDefaults.LoginPath);
                options.LogoutPath = new PathString(CookieAuthenticationDefaults.LogoutPath);
                options.AccessDeniedPath = new PathString(CookieAuthenticationDefaults.AccessDeniedPath);
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