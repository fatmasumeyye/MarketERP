using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MarketERP.Helpers
{
    public class PermissionAuthorizeAttribute : ActionFilterAttribute
    {
        private readonly string _permissionCode;

        public PermissionAuthorizeAttribute(string permissionCode)
        {
            _permissionCode = permissionCode;
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

            if (!httpContext.HasPermission(_permissionCode))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Login", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}