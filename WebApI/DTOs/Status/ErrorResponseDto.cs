using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs.Status
{
    public class ErrorResponseDto
    {
        public string Error { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public IEnumerable<string>? Details { get; set; }
        public DateTime Timestamp { get; set; }
        public string Path { get; set; } = string.Empty;
        public string TraceId { get; set; } = string.Empty;
    }

}