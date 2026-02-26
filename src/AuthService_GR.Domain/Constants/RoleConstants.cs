
using System.Globalization;

namespace AuthService_GR.Domain.Constants;

public static class RoleConstants
{
    public const string CUSTOMER = "CUSTOMER";  
    public const string RESTAURANT_ADMIN = "RESTAURANT_ADMIN";
    public const string PLATFORM_ADMIN = "PLATFORM_ADMIN";

    public static readonly string [] AllowedRoles = [CUSTOMER, RESTAURANT_ADMIN, PLATFORM_ADMIN];
    
}