using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketFlow.Contracts.Dtos;
using TicketFlow.Contracts.Events;
using TicketFlow.OrderingService.Domain.Commands;
using TicketFlow.OrderingService.Domain.Entities;
using TicketFlow.OrderingService.Infrastructure.Data;

namespace TicketFlow.OrderingService.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController(OrderingDbContext db, ReserveOrderHandler reserveHandler) : ControllerBase
{
    private Guid GetCurrentUserId() => Guid.Parse("00000000-0000-0000-0000-000000000001"); // Mocking identity retrieval
    private string GetCurrentUserEmail() => "customer@example.com";

    [HttpPost("reserve")]
    public async Task<IActionResult> Reserve([FromBody] ReserveOrderRequest request)
    {
        var result = await reserveHandler.HandleAsync(request, GetCurrentUserId(), GetCurrentUserEmail());
        return Accepted(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var order = await db.Orders.Include(o => o.Items).AsNoTracking().FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == GetCurrentUserId());
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost("{id:guid}/pay")]
    public async Task<IActionResult> Pay(Guid id, [FromBody] PayOrderRequest request, [FromServices] PayOrderHandler payHandler)
    {
        try
        {
            var order = await payHandler.HandleAsync(id, request);
            if (order is null) return NotFound();
            // Assuming identity checks should happen inside handler or here. Simplified for now.
            if (order.CustomerId != GetCurrentUserId()) return Forbid();
            return Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromServices] CancelOrderHandler cancelHandler)
    {
        var result = await cancelHandler.HandleAsync(id, GetCurrentUserId(), "UserCancelled");
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpGet("my")]
    public async Task<IActionResult> My()
    {
        var customerId = GetCurrentUserId();
        var orders = await db.Orders.Include(o => o.Items).Where(o => o.CustomerId == customerId).OrderByDescending(o => o.CreatedAt).AsNoTracking().ToListAsync();
        return Ok(orders);
    }
}
