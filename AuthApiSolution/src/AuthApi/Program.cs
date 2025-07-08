
using System.Text;
using AuthApi.BusinessLogic.Services;
using AuthApi.Data;
using AuthApi.Data.Repositories;
using AuthApi.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace AuthApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            
            builder.Services.AddDbContext<AuthDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("Database")
                )
            );
            builder.Logging.ClearProviders();
            builder.Host.UseSerilog((ctx, lc) => lc
               .ReadFrom.Configuration(ctx.Configuration)
               .WriteTo.Console()
               .WriteTo.Seq("http://seq:5341")
           );

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            builder.Services.AddAuthorization();

            builder.Services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.RequireHttpsMetadata = false; // not production ready !!!
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = "AuthApi",

                        ValidateAudience = true,
                        ValidAudience = "TaskApi",

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"])),

                        ValidateLifetime = true
                    };
                });

            var app = builder.Build();

            app.UseRouting();
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
