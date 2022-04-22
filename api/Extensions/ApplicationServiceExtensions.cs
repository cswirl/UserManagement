using api.Data;
using api.Helpers;
using api.Interfaces;
using api.Services;
using api.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);

            services.AddScoped<ITokenService, TokenService>();

            services.AddScoped<IEmailService, EmailService>();

            // This will give us pre-populated SmtpSetting object - works similar to an automapper
            services.Configure<SmtpSettings>(config.GetSection("SMTP"));



            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(config.GetConnectionString("Default"));
            });

            return services;
        }
    }
}
