using System.Net.Http;
using System.Text;
using System.Text.Json;
using WebApi.DTOs.SalesOrder;
using Microsoft.Extensions.Logging;

namespace WebApi.Services
{
    public class SalesOrderService
    {
        private readonly HttpClient _client;                       
        private readonly ILogger<SalesOrderService> _log;      

        // URL    CERM 
        private const string _cermApiUrl = "https://cerm/api/salesorders";

        public SalesOrderService(HttpClient client, ILogger<SalesOrderService> log)
        {
            _client = client;
            _log = log;
        }

        /// <summary>
        /// Envía un pedido de venta (SalesOrder) al sistema externo (CERM).
        /// </summary>
        public async Task<HttpResponseMessage> SendOrderAsync(SalesOrderDto salesOrder)
        {
            try
            {
                // Convertir el DTO en la estructura JSON esperada por CERM
                var payload = new
                {
                    ReferenceAtCustomer = salesOrder.ReferenceAtCustomer,
                    LineComment1 = salesOrder.LineComment1,
                    Delivery = new
                    {
                        Type = "Address",
                        Comment = salesOrder.LineComment1 ?? "No comments",
                        AddressId = salesOrder.AddressId,
                        ExpectedDate = salesOrder.ExpectedDate.ToString("yyyy-MM-dd")
                    },
                    InvoiceLines = new[]
                    {
                        new
                        {
                            InvoicePriceLineId = Guid.NewGuid().ToString(),
                            Description = $"Product {salesOrder.ProductId}",
                            Quantity = salesOrder.OrderQuantity,
                            UnitPrice = salesOrder.UnitPrice,
                            Type = "SalesOrder"
                        }
                    },
                    OrderQuantity = salesOrder.OrderQuantity,
                    ProductId = salesOrder.ProductId,
                    UnitPrice = salesOrder.UnitPrice,
                    Prepayment = new
                    {
                        TransactionId = salesOrder.TransactionId,
                        TransactionTime = salesOrder.TransactionTime.ToString("o"),
                        TransactionAmount = salesOrder.TransactionAmount,
                        TransactionMethod = salesOrder.TransactionMethod
                    }
                };

                var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _log.LogInformation("Enviando SalesOrder a CERM: {json}", json);

                // Enviar POST a CERM
                var response = await _client.PostAsync(_cermApiUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _log.LogWarning("CERM devolvió un error ({StatusCode}): {Body}", response.StatusCode, errorBody);
                }

                return response;
            }
            catch (HttpRequestException ex)
            {
                _log.LogError(ex, "Error al enviar SalesOrder a CERM");
                throw new Exception("No se pudo enviar el pedido a CERM. Ver logs para más detalles.", ex);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error inesperado al enviar SalesOrder a CERM");
                throw;
            }
        }
    }
}
