namespace MarketERP.Models;

public class CustomerDetailViewModel
{
    public required Customer Customer { get; set; }
    public decimal TotalShopping { get; set; }
    public int TotalOrders { get; set; }
    public DateTime? LastSaleDate { get; set; }
    public decimal? LastSaleAmount { get; set; }
}

public class SupplierDetailViewModel
{
    public required Supplier Supplier { get; set; }
    public decimal TotalPurchases { get; set; }
    public int TotalOrders { get; set; }
    public PurchaseOrder? LastOrder { get; set; }
}

public class EmployeeDetailViewModel
{
    public required Employee Employee { get; set; }
    public int TotalLeaveRequests { get; set; }
    public int ApprovedLeaveRequests { get; set; }
    public int ApprovedLeaveDays { get; set; }
    public int TotalShifts { get; set; }
    public decimal TotalShiftHours { get; set; }
    public EmployeeShift? NextShift { get; set; }
}
