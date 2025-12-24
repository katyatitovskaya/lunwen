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
    }

}
