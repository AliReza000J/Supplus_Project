using _0_Framework.Application;
using _01_PharmaPlusQuery.Contracts.Ticket;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiteManagement.Application.Contracts.Ticket;
using System.Security.Claims;

namespace PharmaStore.WebApi.Controllers.Ticket
{
    /// <summary>
    /// Manages support tickets for users.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly ITicketApplication _ticketApp;
        private readonly ITicketQuery _ticketQuery;

        /// <summary>
        /// Initializes a new instance of the TicketController.
        /// </summary>
        public TicketController(ITicketApplication ticketApp, ITicketQuery ticketQuery)
        {
            _ticketApp = ticketApp;
            _ticketQuery = ticketQuery;
        }

        /// <summary>
        /// Creates a new support ticket.
        /// </summary>
        /// <remarks>
        /// This endpoint allows an authenticated user to create a new support ticket.
        /// The user's ID is automatically attached from their authentication token.
        /// The input is taken as `form-data`.
        /// <br/>
        /// </remarks>
        /// <param name="input">The details of the ticket to be created (`FromForm`).</param>
        /// <param name="cancellation">A token to cancel the operation.</param>
        /// <response code="200">Ticket created successfully. The response body contains the tracking code for the new ticket.</response>
        /// <response code="400">Bad Request. The input data is invalid. Check the validation messages in the response.</response>
        /// <response code="401">Unauthorized. The user is not authenticated.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<string>>> Create([FromForm] CreateTicketDto input, CancellationToken cancellation)
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

            return result.IsSucceeded ? Ok(ApiResponse<string>.Ok(code, result.Message))
                : BadRequest(ApiResponse<string>.Fail(result.Message));
        }
    }

}
