namespace WebApi.DTOs.SalesOrder
{
    public class DeliveryDto
    {
        public string Type { get; set; } = "Address";
        public string Comment { get; set; }
        public string AddressId { get; set; }
        public DateTime ExpectedDate { get; set; }
    }
}
