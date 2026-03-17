using EventEase_st10157545_POE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EventEase_st10157545_POE.Filter
{
    /// <summary>
    /// To Apply to any of  controller or action that requires a logged-in user.
    /// Redirects to /Account/Login if no session is found.
    /// </summary>
    public class RequireLoginAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var auth = context.HttpContext.RequestServices.GetService<AuthService>();

            if (auth == null || !auth.IsSignedIn())
            {
                var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                context.Result = new RedirectToActionResult("Login", "Account",
                    new { returnUrl });
            }

            base.OnActionExecuting(context);
        }
    }

    /// <summary>
    /// Applied to actions that only Admins may access (e.g. managing specialists).
    /// Returns 403 Forbidden if user is logged in but is not an Admin.
    /// </summary>
    public class RequireAdminAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var auth = context.HttpContext.RequestServices.GetService<AuthService>();

            if (auth == null || !auth.IsSignedIn())
            {
                var returnUrl = context.HttpContext.Request.Path;
                context.Result = new RedirectToActionResult("Login", "Account",
                    new { returnUrl });
                return;
            }

            if (!auth.IsAdmin())
            {
                // Logged in but not Admin — show access denied page
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
