namespace InsightApp.Entities
{
    public class SalesReport
    {
        public int? OrderId { get; set; }
        public DateOnly? OrderDate { get; set; }
        public int? NumberOfItems { get; set; }
        public double? TotalPayment { get; set; }
        public string? Email { get; set; }
    }
}

