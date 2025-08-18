namespace E_Commerce.Common.Domain.Enums;

public enum CustomerStatus
{
    Active = 1,
    Inactive = 2,
    Suspended = 3,
    Pending = 4,
    Blocked = 5,
    Deleted = 6
}

public static class CustomerStatusExtensions
{
    public static bool IsActive(this CustomerStatus status) => status == CustomerStatus.Active;
    
    public static bool CanPlaceOrders(this CustomerStatus status) => 
        status == CustomerStatus.Active || status == CustomerStatus.Pending;
    
    public static bool IsBlocked(this CustomerStatus status) => 
        status == CustomerStatus.Blocked || status == CustomerStatus.Suspended;
    
    public static string GetDisplayName(this CustomerStatus status) => status switch
    {
        CustomerStatus.Active => "Active",
        CustomerStatus.Inactive => "Inactive", 
        CustomerStatus.Suspended => "Suspended",
        CustomerStatus.Pending => "Pending Verification",
        CustomerStatus.Blocked => "Blocked",
        CustomerStatus.Deleted => "Deleted",
        _ => status.ToString()
    };
}
