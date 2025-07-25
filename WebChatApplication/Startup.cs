using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using WebChatApplication.DataAccess.Contexts;
using WebChatApplication.DataAccess.Repositories;
using WebChatApplication.Extensions;
using WebChatApplication.Hubs;
using WebChatApplication.Middlewares;
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
                                                        options.UseNpgsql(configuration
                                                                              .GetConnectionString("WebchatConnection"));
                                                    });

        services.AddScoped<IUserRelationRepository, UserRelationRepository>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRoleService, RoleService>();

        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IChatRepository, ChatRepository>();

        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        
        services.AddScoped<IAttachmentRepository, AttachmentRepository>();

        services.AddSignalR(options =>
                            {
                                options.MaximumReceiveMessageSize = 1024 * 1024 * 20;
                            });

        services.AddFluentEmail(configuration);
        services.AddScoped<IEmailService, EmailService>();

        services.AddMinio(configuration);
        services.AddScoped<IS3Service, S3Service>();

        services.AddHangfire(globalConfiguration => globalConfiguration
                                                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                                                    .UseSimpleAssemblyNameTypeSerializer()
                                                    .UseRecommendedSerializerSettings()
                                                    .UsePostgreSqlStorage(c =>
                                                                              c.UseNpgsqlConnection(
                                                                               configuration
                                                                                   .GetConnectionString("HangfireConnection"))));
        services.AddHangfireServer();

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
        app.UseStatusCodePagesWithRedirects("/error/{0}");

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseMiddleware<LastActivityMiddleware>();

        app.UseEndpoints(endpoints =>
                         {
                             endpoints.MapControllerRoute("default",
                                                          "{controller=WebChat}/{action=Index}");
                             endpoints.MapHub<ChatHub>("/chatHub");
                             if (env.IsDevelopment())
                             {
                                 endpoints.MapHangfireDashboard();
                             }
                         });
    }
}