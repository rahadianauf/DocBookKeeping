namespace DocBookKeeping.Models;

public class MonthlySummaryDto
{
    public string Bulan { get; set; } = string.Empty;
    public decimal Total { get; set; }
}

public class DashboardSummaryDto
{
    public decimal TotalPemasukan { get; set; }
    public decimal TotalPengeluaran { get; set; }
}