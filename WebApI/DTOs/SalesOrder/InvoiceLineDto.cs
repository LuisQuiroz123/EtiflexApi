using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs.SalesOrder
{
    public class InvoiceLineDto
    {
        public string InvoicePriceLineId { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string Type { get; set; } = "SalesOrder";

    }
}
