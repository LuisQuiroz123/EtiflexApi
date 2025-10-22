using System;

namespace WebApi.DTOs.SalesOrder
{
    public class PrepaymentDto
    {
        public Guid TransactionId { get; set; }
        public DateTime TransactionTime { get; set; }
        public decimal TransactionAmount { get; set; }
        public string TransactionMethod { get; set; }
    }
}
