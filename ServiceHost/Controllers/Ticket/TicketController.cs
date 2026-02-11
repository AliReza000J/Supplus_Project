using _0_Framework.Application;
using _01_PharmaPlusQuery.Contracts.Ticket;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceHost.Filters;
using SiteManagement.Application.Contracts.Ticket;
using System.Security.Claims;

namespace ServiceHost.Controllers.Ticket
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly ITicketApplication _ticketApp;
        private readonly ITicketQuery _ticketQuery;

        public TicketController(ITicketApplication ticketApp, ITicketQuery ticketQuery)
        {
            _ticketApp = ticketApp;
            _ticketQuery = ticketQuery;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [RateLimit(Limit = 5, Seconds = 60)]
        public async Task<ActionResult<ApiResponse<object>>> Create([FromForm] CreateTicketDto input, CancellationToken cancellation)
        {
            var dto = new CreateTicket
            {
                Email = input.Email,
                Message = input.Message,
                Subject = input.Subject
            };

            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            if (long.TryParse(idClaim, out var parsedId))
                dto.UserId = parsedId;

            var (code, result) = await _ticketApp.CreateTicket(dto, cancellation);

            return result.IsSucceeded ? Ok(ApiResponse<object>.Ok(code, result.Message))
                : BadRequest(ApiResponse<object>.Fail(result.Message));
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("track/{code}")]
        public async Task<ActionResult<ApiResponse<TicketQueryModel>>> Track(string code, CancellationToken cancellation)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            var result = await _ticketQuery.GetByCode(code, idClaim, cancellation);

            if (result == null)
                return NotFound(ApiResponse<TicketQueryModel>
                    .Fail("موردی یافت نشد."));

            return Ok(ApiResponse<TicketQueryModel>.Ok(result));
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("my")]
        public async Task<ActionResult<ApiResponse<List<TicketQueryModel>>>> MyTickets(CancellationToken cancellation)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            var result = await _ticketQuery.GetUserTickets(idClaim, cancellation);

            if (result == null || !result.Any())
                return NotFound(ApiResponse<List<TicketQueryModel>>
                    .Fail("موردی یافت نشد."));

            return Ok(ApiResponse<List<TicketQueryModel>>.Ok(result));
        }
    }
}
