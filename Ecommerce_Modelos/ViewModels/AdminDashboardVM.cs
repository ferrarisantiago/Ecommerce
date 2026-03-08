namespace Ecommerce_Modelos.ViewModels
{
    public class AdminDashboardVM
    {
        public int ActiveProducts { get; set; }
        public int InactiveProducts { get; set; }
        public int LowStockCount { get; set; }
        public int TotalOrders { get; set; }
        public double TotalRevenue { get; set; }
        public IList<LowStockAlertItemVM> LowStockItems { get; set; } = new List<LowStockAlertItemVM>();
    }

    public class LowStockAlertItemVM
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int MinimumStockLevel { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
