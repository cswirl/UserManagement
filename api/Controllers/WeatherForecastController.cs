using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace api.Controllers
{
    public class WeatherForecastController : AppBaseController
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IConfiguration configuration;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IConfiguration configuration)
        {
            _logger = logger;
            this.configuration = configuration;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet("Test1")]
        public string Test()
        {
            var request = HttpContext.Request;

            var uri = string.Concat(request.Scheme, "://",
                        request.Host.ToUriComponent(),
                        request.PathBase.ToUriComponent(),
                        request.Path.ToUriComponent(),
                        request.QueryString.ToUriComponent());



            return uri;
        }

        [HttpGet("Test2")]
        public string Test2()
        {
            var client_pathBase = configuration.GetSection("AngularClient").Value;
            var resetPassword_path = HttpContext.Request.Path.ToUriComponent().Replace("Test2", "reset-password"); ;
            var abs_path = string.Concat(client_pathBase, resetPassword_path);

            var uri = abs_path;

            return uri;
        }

        [HttpGet("test3")]
        public string Test3()
        {
            string path = "/auth/password-reset";
            string ret = "";

            var headers = HttpContext.Request.Headers;
            var origin = headers["origin"];
            if (origin != Microsoft.Extensions.Primitives.StringValues.Empty)
            {
                return string.Concat(origin.FirstOrDefault().ToString(), path);
            }

            var request = HttpContext.Request;
            ret = string.Concat(request.Scheme, "://",
                        request.Host.ToUriComponent(),
                        request.PathBase.ToUriComponent(),
                        path);

            return ret;
        }
    }
}
