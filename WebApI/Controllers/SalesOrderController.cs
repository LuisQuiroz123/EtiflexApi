using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Services;
using Microsoft.Extensions.Logging;
using DtoDelivery = WebApi.DTOs.SalesOrder.DeliveryDto;
using WebApi.DTOs.SalesOrder;

[ApiController]
[Route("salesorder")]
public class SalesOrderController : ControllerBase
{
    private readonly SalesOrderService _salesOrderService;
    private readonly EtiDbContext _context;
    private readonly ILogger<SalesOrderController> _logger;

    public SalesOrderController(SalesOrderService salesOrderService, EtiDbContext context,
        ILogger<SalesOrderController> logger)
    {
        _salesOrderService = salesOrderService;
        _context = context;
        _logger = logger;
    }

    // POST /salesorder -> Recibe lista de órdenes

    [HttpPost]
    public async Task<IActionResult> CreateSalesOrders([FromBody] List<SalesOrderRequestDto> orders)
    {
        if (orders == null || !orders.Any())
            return BadRequest(new { error = "BAD_REQUEST", message = "Datos incompletos" });

        var results = new List<object>();

        // consulta el numero de orden o referencia del cliente registrados
        var existingReferences = await _context.SalesOrders
            .Select(o => o.ReferenceAtCustomer)
            .ToListAsync();

        foreach (var dto in orders)
        {
            try
            {
                // Verificar duplicado
                if (existingReferences.Contains(dto.ReferenceAtCustomer))
                {
                    results.Add(new
                    {
                        reference = dto.ReferenceAtCustomer,
                        orderState = "Failed",
                        error = "Ya existe una orden con esta Referencia"
                    });
                    continue; // omitir esta orden
                }

                // Crear la orden
                var localOrder = new SalesOrder
                {
                    ReferenceAtCustomer = dto.ReferenceAtCustomer,
                    LineComment1 = dto.LineComment1,
                    ProductId = dto.ProductId,
                    OrderQuantity = dto.OrderQuantity,
                    UnitPrice = dto.UnitPrice,
                    AddressId = dto.Delivery?.AddressId,
                    ExpectedDate = dto.Delivery?.ExpectedDate ?? DateTime.UtcNow,
                    TransactionId = dto.Prepayment?.TransactionId ?? Guid.NewGuid(),
                    TransactionTime = dto.Prepayment?.TransactionTime ?? DateTime.UtcNow,
                    TransactionAmount = dto.Prepayment?.TransactionAmount ?? 0,
                    TransactionMethod = dto.Prepayment?.TransactionMethod ?? "N/A",
                    OrderState = "Pending"
                };

                // Agregar líneas de la orden
                foreach (var line in dto.InvoiceLines)
                {
                    localOrder.InvoiceLines.Add(new SalesOrderLine
                    {
                        InvoicePriceLineId = line.InvoicePriceLineId,
                        Description = line.Description,
                        Quantity = line.Quantity,
                        UnitPrice = line.UnitPrice,
                        Type = line.Type,
                        SalesOrderId = localOrder.SalesOrderId
                    });
                }

                _context.SalesOrders.Add(localOrder);

                // Añadir a la lista de referencias existentes para evitar duplicados en el mismo batch
                existingReferences.Add(dto.ReferenceAtCustomer);

                results.Add(new
                {
                    reference = localOrder.ReferenceAtCustomer,
                    orderState = localOrder.OrderState,
                    message = "Orden guardada correctamente"
                });
            }
            catch (Exception ex)
            {
                results.Add(new
                {
                    reference = dto.ReferenceAtCustomer,
                    orderState = "Failed",
                    error = ex.Message
                });
            }
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "INTERNAL_SERVER_ERROR", message = ex.Message });
        }

        return Created("", results);
    }



    // GET /salesorder/{reference} -> Obtener orden específica
    [HttpGet("{reference}")]
    public async Task<IActionResult> GetSalesOrder(string reference)
    {
        if (string.IsNullOrWhiteSpace(reference))
            return BadRequest(new { error = "BAD_REQUEST", message = "Referencia requerida" });

        try
        {
            var order = await _context.SalesOrders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.ReferenceAtCustomer.ToLower() == reference.Trim().ToLower());

            if (order == null)
                return NotFound(new { error = "NOT_FOUND", message = "Orden no encontrada" });

            var dto = new SalesOrderRequestDto
            {
                ReferenceAtCustomer = order.ReferenceAtCustomer,
                LineComment1 = order.LineComment1 ?? string.Empty,
                Delivery = new DtoDelivery
                {
                    Type = "Address",
                    Comment = order.LineComment1 ?? string.Empty,
                    AddressId = order.AddressId ?? string.Empty,
                    ExpectedDate = order.ExpectedDate
                },
                InvoiceLines = new List<InvoiceLineDto>(),
                Prepayment = new PrepaymentDto
                {
                    TransactionId = order.TransactionId,
                    TransactionTime = order.TransactionTime,
                    TransactionAmount = order.TransactionAmount,
                    TransactionMethod = order.TransactionMethod ?? "N/A"
                },
                OrderQuantity = order.OrderQuantity,
                ProductId = order.ProductId ?? string.Empty,
                UnitPrice = order.UnitPrice
            };

            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo la orden {Reference}", reference);
            return StatusCode(500, new { error = "INTERNAL_SERVER_ERROR", message = ex.Message });
        }
    }


    // GET /salesorder -> Obtener todas las órdenes
    [HttpGet]
    public async Task<IActionResult> GetAllSalesOrders()
    {
        try
        {
            var dtoList = await _context.SalesOrders
                .AsNoTracking()
                .Select(order => new SalesOrderRequestDto
                {
                    ReferenceAtCustomer = order.ReferenceAtCustomer,
                    LineComment1 = order.LineComment1 ?? string.Empty,
                    Delivery = new DtoDelivery
                    {
                        Type = "Address",
                        Comment = order.LineComment1 ?? string.Empty,
                        AddressId = order.AddressId ?? string.Empty,
                        ExpectedDate = order.ExpectedDate
                    },
                    InvoiceLines = new List<InvoiceLineDto>(),
                    Prepayment = new PrepaymentDto
                    {
                        TransactionId = order.TransactionId,
                        TransactionTime = order.TransactionTime,
                        TransactionAmount = order.TransactionAmount,
                        TransactionMethod = order.TransactionMethod ?? "N/A"
                    },
                    OrderQuantity = order.OrderQuantity,
                    ProductId = order.ProductId ?? string.Empty,
                    UnitPrice = order.UnitPrice
                })
                .ToListAsync();

            return Ok(dtoList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo todas las órdenes");
            return StatusCode(500, new { error = "INTERNAL_SERVER_ERROR", message = ex.Message });
        }
    }

}
