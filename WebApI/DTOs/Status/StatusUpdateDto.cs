using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs.Status
{
    public class StatusUpdateDto
    {
        [Required]
        public Guid PrintRequestId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;

        [Required]
        public int Code { get; set; }

        public TrackingDataDto Data { get; set; } = new TrackingDataDto();
    }

}