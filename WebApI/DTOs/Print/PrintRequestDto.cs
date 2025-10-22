using System.Text.Json.Serialization;

namespace WebApi.DTOs.Print
{
    public class PrintRequestDto
    {
        public string RequestNumber { get; set; }
        public string DeliveryType { get; set; }

        [JsonPropertyName("Data")]
        public ClientDataDto ClientData { get; set; } 
        public string? Notes { get; set; }

        [JsonPropertyName("Files")] 
        public List<RequestFileDto> RequestFiles { get; set; } = new();

    }
}
