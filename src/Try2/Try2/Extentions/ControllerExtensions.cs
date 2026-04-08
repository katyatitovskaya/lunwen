using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Try2.Extentions
{
    public static class ControllerExtensions
    {
        public static int GetUserId(this ControllerBase controller)
        {
            return int.Parse(
                controller.User.FindFirstValue(ClaimTypes.NameIdentifier)
            );
        }

        public static bool IsAdmin(this ClaimsPrincipal user)
        {
            var isAdminClaim = user.FindFirstValue("IsAdmin");
            return isAdminClaim != null && bool.Parse(isAdminClaim);
        }
    }

}
