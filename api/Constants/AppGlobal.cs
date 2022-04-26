using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Constants
{
    public class AppGlobal
    {
        public static readonly int TOKEN_EXPIRY = 2;        //Hours


        public static string CallbackUrlPath(HttpContext httpContext, string path)
        {
            //string path = "/auth/password-reset";
            string ret = string.Empty;

            var headers = httpContext.Request.Headers;
            var origin = headers["origin"];
            // We can also check the flag headers['sec-fetch-site']
            /// if the flag headers['sec-fetch-site'] == same-site; then the headers["origin"] will have a value
            /// if the headers['sec-fetch-site'] == same-origin; then the headers["origin"] will be empty
            if (origin != Microsoft.Extensions.Primitives.StringValues.Empty)
            {
                return string.Concat(origin.FirstOrDefault().ToString(), path);
            }

            // This is the default fallback in-case headers["origin"] is empty
            var request = httpContext.Request;
            ret = string.Concat(request.Scheme, "://",
                        request.Host.ToUriComponent(),
                        request.PathBase.ToUriComponent(),
                        path);

            return ret;
        }
    }
}
