namespace WebApi.DTOs.Print
{
    public class ClientDataDto
    {
        public Guid TransactionId { get; set; }
        public string ClientName { get; set; }
        public string ClientCode { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? AddressLine3 { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Attent { get; set; }
    }
}
