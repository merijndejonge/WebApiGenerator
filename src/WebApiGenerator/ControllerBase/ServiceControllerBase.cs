using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace OpenSoftware.WebApiGenerator.ControllerBase
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class ServiceControllerBase : Microsoft.AspNetCore.Mvc.ControllerBase
    {
        protected T GetFromHttpHeader<T>(string headerName)  
        {
            var header = GetHeader(Request, headerName);
            if (header == null) return default(T);
            var headerValue = Convert.ChangeType(header, typeof(T));
            return (T) headerValue;
        }

        protected T GetFromClaim<T>(string claimName)
        {
            var claim = User.Identity.GetClaim(claimName);

            var claimValue = Convert.ChangeType(claim.Value, typeof(T));
            return (T) claimValue;
        }

        private static string GetHeader(HttpRequest request, string headerName)
        {
            return request.Headers.Where(x => string.Equals(x.Key, headerName, StringComparison.OrdinalIgnoreCase))
                .SelectMany(x => x.Value).FirstOrDefault();
        }
    }
}