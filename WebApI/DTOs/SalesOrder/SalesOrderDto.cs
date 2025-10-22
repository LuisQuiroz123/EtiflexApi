namespace WebApi.DTOs.SalesOrder
{
    public class SalesOrderDto
    {
        public string ReferenceAtCustomer { get; set; }
        public string LineComment1 { get; set; }
        public string ProductId { get; set; }
        public int OrderQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string AddressId { get; set; }
        public DateTime ExpectedDate { get; set; }

        public Guid TransactionId { get; set; }
        public DateTime TransactionTime { get; set; }
        public decimal TransactionAmount { get; set; }
        public string TransactionMethod { get; set; }

        public string Type { get; set; } = "SalesOrder";

        public decimal TotalAmount => OrderQuantity * UnitPrice;
    }
}
