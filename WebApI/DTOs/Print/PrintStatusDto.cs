using WebApi.DTOs.Status;

namespace WebApi.DTOs.Print
{
    public class PrintStatusDto
    {
        public DateTime Date { get; set; }
        public string Status { get; set; }  // Descripcion del status
        public int Code { get; set; }    

        public TrackingDataDto Data { get; set; }
    }
}
