using System;
using System.Collections.Generic;

namespace WebApi.DTOs.SalesOrder
{
    public class SalesOrderRequestDto
    {
        public string ReferenceAtCustomer { get; set; }
        public string LineComment1 { get; set; }

        public DeliveryDto Delivery { get; set; }
        public List<InvoiceLineDto> InvoiceLines { get; set; }
        public PrepaymentDto Prepayment { get; set; }

        public int OrderQuantity { get; set; }
        public string ProductId { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
