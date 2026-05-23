using MarketERP.Data;

namespace MarketERP.Helpers
{
    public static class SessionPermissionHelper
    {
        public static bool HasPermission(this HttpContext httpContext, string permissionCode)
        {
            var employeeId = httpContext.Session.GetInt32("EmployeeId");

            if (employeeId == null)
                return false;

            var context = httpContext.RequestServices.GetRequiredService<AppDbContext>();

            var hasPermission = context.UserRoles
                .Where(ur => ur.EmployeeId == employeeId.Value)
                .SelectMany(ur => ur.Role.RolePermissions)
                .Any(rp => rp.Permission.Code == permissionCode);

            return hasPermission;
        }
    }
}