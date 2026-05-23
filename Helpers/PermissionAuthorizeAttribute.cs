using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MarketERP.Helpers
{
    public class PermissionAuthorizeAttribute : ActionFilterAttribute
    {
        private readonly string[] _permissionCodes;

        public PermissionAuthorizeAttribute(params string[] permissionCodes)
        {
            _permissionCodes = permissionCodes;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;

            var employeeId = httpContext.Session.GetInt32("EmployeeId");

            if (employeeId == null)
            {
                context.Result = new RedirectToActionResult("Index", "Login", null);
                return;
            }

            var hasAnyPermission = _permissionCodes.Any(permission =>
                httpContext.HasPermission(permission));

            if (!hasAnyPermission)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Login", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}