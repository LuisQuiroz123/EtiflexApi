using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.DTOs.Status;

[ApiController]
[Route("status")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class StatusController : ControllerBase
{
    private readonly EtiDbContext _context;

    public StatusController(EtiDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> UpdatePrintStatus([FromBody] StatusUpdateDto dto)
    {
        // 1️⃣ Validación básica
        if (dto == null)
            return BadRequest(new
            {
                error = "BAD_REQUEST",
                message = "Datos de actualización de estado inválidos",
                timestamp = DateTime.UtcNow,
                path = HttpContext.Request.Path,
                traceId = HttpContext.TraceIdentifier
            });

        if (dto.PrintRequestId == Guid.Empty)
            return BadRequest(new
            {
                error = "BAD_REQUEST",
                message = "El campo 'printRequestId' debe ser un UUID válido",
                timestamp = DateTime.UtcNow,
                path = HttpContext.Request.Path,
                traceId = HttpContext.TraceIdentifier
            });

        // 2️⃣ Validar que la solicitud exista
        var printRequest = await _context.PrintRequests
            .FirstOrDefaultAsync(r => r.PrintRequestId == dto.PrintRequestId);

        if (printRequest == null)
            return BadRequest(new
            {
                error = "BAD_REQUEST",
                message = $"No se encontró la solicitud con ID {dto.PrintRequestId}",
                timestamp = DateTime.UtcNow,
                path = HttpContext.Request.Path,
                traceId = HttpContext.TraceIdentifier
            });

        // 3️⃣ Validar que el código y status existan en la tabla AllowedPrintStatuses
        var allowedStatus = await _context.AllowedPrintStatuses
            .FirstOrDefaultAsync(s => s.Code == dto.Code);

        if (allowedStatus == null)
            return BadRequest(new
            {
                error = "BAD_REQUEST",
                message = $"Código de estado inválido: {dto.Code}",
                details = new[] { "El código no es un dato valido" },
                timestamp = DateTime.UtcNow,
                path = HttpContext.Request.Path,
                traceId = HttpContext.TraceIdentifier
            });

        if (dto.Status != allowedStatus.Status)
            return BadRequest(new
            {
                error = "BAD_REQUEST",
                message = $"El status '{dto.Status}' no coincide con el código {dto.Code}",
                timestamp = DateTime.UtcNow,
                path = HttpContext.Request.Path,
                traceId = HttpContext.TraceIdentifier
            });

        // 4️⃣ Guardar el estado
        var newStatus = new PrintStatus
        {
            PrintRequestId = dto.PrintRequestId,
            Date = dto.Date,
            Status = dto.Status,
            Code = dto.Code,
            TrackingId = dto.Data?.TrackingId
        };

        _context.PrintStatuses.Add(newStatus);
        await _context.SaveChangesAsync();

        return Ok(new { success = true });
    }
}

