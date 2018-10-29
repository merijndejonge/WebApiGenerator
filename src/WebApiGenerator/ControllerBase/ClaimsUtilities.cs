using System.Security.Claims;
using System.Security.Principal;

namespace OpenSoftware.WebApiGenerator.ControllerBase
{
    internal static class ClaimsUtilities
    {
        public static Claim GetClaim(this IIdentity id, string name)
        {
            var identity = id as ClaimsIdentity;

            return identity?.FindFirst(name);
        }
    }
}