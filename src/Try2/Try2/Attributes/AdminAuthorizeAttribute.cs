using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Try2.Attributes
{
    public class AdminAuthorizeAttribute: AuthorizeAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity.IsAuthenticated)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            var isAdminClaim = user.FindFirst("IsAdmin");
            if (isAdminClaim == null || !bool.Parse(isAdminClaim.Value))
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
