using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Serilog;
using WebApi.DTOs.SalesOrder;

namespace WebApi.Services
{
    public class CermOrderService
    {
        private readonly HttpClient _client; 

        public CermOrderService(HttpClient client)
        {
            _client = client;
        }

        public async Task<string> SendOrderAsync(SalesOrderDto order)
        {
            try
            {
                // POST a la API de CERM
                var response = await _client.PostAsJsonAsync("endpoint/de/ventas", order);

                response.EnsureSuccessStatusCode(); // Lanza excepción si el status no es 2xx

                var result = await response.Content.ReadAsStringAsync();
                return result;
            }
            catch (HttpRequestException ex)
            {
                Log.Error(ex, "Error enviando la orden a CERM");
                throw new Exception("No se pudo enviar la orden a CERM. Ver logs para más detalles.", ex);
            }
            catch (TaskCanceledException ex)
            {
                Log.Error(ex, "Timeout al enviar la orden a CERM");
                throw new Exception("Tiempo de espera agotado al enviar la orden a CERM.", ex);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error inesperado al enviar la orden a CERM");
                throw;
            }
        }
    }
}
