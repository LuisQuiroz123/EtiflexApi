using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs.Print
{
    public class RequestFileDto
    {
        [Required(ErrorMessage = "El campo 'Type' es obligatorio.")]
        public string Type { get; set; }
        [Required(ErrorMessage = "El campo 'Name' es obligatorio.")]
        public string Name { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "El campo 'TotalLabels' debe ser mayor que cero.")]

        public int TotalLabels { get; set; }

        [Required(ErrorMessage = "El campo 'Url' es obligatorio.")]
        [Url(ErrorMessage = "El campo 'Url' no tiene un formato válido.")]
        public string Url { get; set; }
    }
}
