namespace Ecommerce_Modelos.ViewModels
{
    public class SalesReportVM
    {
        public string Range { get; set; } = SalesReportRanges.Last7Days;
        public string RangeLabel { get; set; } = "Ultimos 7 dias";
        public DateTime? StartUtc { get; set; }
        public DateTime? EndUtc { get; set; }

        public int TotalOrders { get; set; }
        public int TotalUnitsSold { get; set; }
        public double TotalRevenue { get; set; }

        public IList<SalesReportItemVM> TopProducts { get; set; } = new List<SalesReportItemVM>();
    }

    public class SalesReportItemVM
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int TotalQuantitySold { get; set; }
        public double TotalRevenue { get; set; }
    }

    public static class SalesReportRanges
    {
        public const string Today = "today";
        public const string Last7Days = "7d";
        public const string Last30Days = "30d";
        public const string AllTime = "all";
    }
}
