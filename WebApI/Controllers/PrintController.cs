using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.DTOs.Print;
using WebApi.DTOs.Status;

[ApiController]
[Route("print")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class PrintController : ControllerBase
{
    private readonly EtiDbContext _context;
    private readonly IMapper _mapper;

    public PrintController(EtiDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    #region Create Print Request
    [HttpPost]
    public async Task<IActionResult> CreatePrintRequest([FromBody] PrintRequestDto dto)
    {
        if (dto == null || dto.ClientData == null || string.IsNullOrEmpty(dto.RequestNumber))
            return BadRequest(new { error = "BAD_REQUEST", message = "Datos incompletos o inválidos" });

        try
        {
            var entity = _mapper.Map<PrintRequest>(dto);
            if (entity.ClientData.TransactionId == Guid.Empty)
                entity.ClientData.TransactionId = Guid.NewGuid();

            _context.PrintRequests.Add(entity);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPrintRequest), new { id = entity.PrintRequestId }, _mapper.Map<PrintRequestDto>(entity));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "INTERNAL_SERVER_ERROR", message = ex.Message });
        }
    }
    #endregion

    #region Get Single Print Request
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPrintRequest(Guid id)
    {
        var request = await _context.PrintRequests
            .Include(r => r.RequestFiles)
            .FirstOrDefaultAsync(r => r.PrintRequestId == id);

        if (request == null)
            return NotFound(new { error = "NOT_FOUND", message = "Solicitud no encontrada" });

        return Ok(_mapper.Map<PrintRequestDto>(request));
    }
    #endregion

    #region Get All Requests
    [HttpGet("all")]
    public async Task<IActionResult> GetAllRequests()
    {
        var requests = await _context.PrintRequests
            .Include(r => r.RequestFiles)
            .ToListAsync();

        return Ok(_mapper.Map<List<PrintRequestDto>>(requests));
    }
    #endregion

    #region Get Requests by Client
    [HttpGet("by-client/{clientCode}")]
    public async Task<IActionResult> GetRequestsByClient(string clientCode)
    {
        if (string.IsNullOrEmpty(clientCode))
            return BadRequest(new { error = "BAD_REQUEST", message = "Código de cliente requerido" });

        var filtered = await _context.PrintRequests
            .Include(r => r.RequestFiles)
            .Where(r => r.ClientData.ClientCode == clientCode)
            .ToListAsync();

        if (!filtered.Any())
            return NotFound(new { error = "NOT_FOUND", message = "No se encontraron solicitudes para este cliente" });

        return Ok(_mapper.Map<List<PrintRequestDto>>(filtered));
    }
    #endregion

    #region Reprint Print Request
    [HttpPost("reprint/{id:guid}")]
    public async Task<IActionResult> ReprintPrintRequest(Guid id)
    {
        var original = await _context.PrintRequests
            .Include(r => r.RequestFiles)
            .FirstOrDefaultAsync(r => r.PrintRequestId == id);

        if (original == null)
            return NotFound(new { error = "NOT_FOUND", message = "Solicitud original no encontrada" });

        try
        {
            var newRequest = new PrintRequest
            {
                PrintRequestId = Guid.NewGuid(),
                RequestNumber = original.RequestNumber,
                DeliveryType = original.DeliveryType,
                Notes = original.Notes,
                ClientData = new ClientData
                {
                    TransactionId = Guid.NewGuid(),
                    ClientName = original.ClientData.ClientName,
                    ClientCode = original.ClientData.ClientCode,
                    AddressLine1 = original.ClientData.AddressLine1,
                    AddressLine2 = original.ClientData.AddressLine2,
                    AddressLine3 = original.ClientData.AddressLine3,
                    PhoneNumber = original.ClientData.PhoneNumber,
                    Attent = original.ClientData.Attent
                },
                RequestFiles = original.RequestFiles.Select(f => _mapper.Map<RequestFile>(f)).ToList()
            };

            _context.PrintRequests.Add(newRequest);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPrintRequest), new { id = newRequest.PrintRequestId }, _mapper.Map<PrintRequestDto>(newRequest));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "INTERNAL_SERVER_ERROR", message = ex.Message });
        }
    }
    #endregion

    #region Get Print Status
    [HttpGet("{id:guid}/status")]
    public async Task<IActionResult> GetPrintStatus(Guid id)
    {
        var statusList = await _context.PrintStatuses
            .Where(s => s.PrintRequestId == id)
            .Select(s => new PrintStatusDto
            {
                Date = s.Date,
                Status = s.Status,
                Code = s.Code,
                Data = new TrackingDataDto { TrackingId = s.TrackingId }
            })
            .ToListAsync();

        if (!statusList.Any())
            return NotFound(new { error = "NOT_FOUND", message = "No se encontraron estados para esta solicitud" });

        return Ok(statusList);
    }
    #endregion
}
