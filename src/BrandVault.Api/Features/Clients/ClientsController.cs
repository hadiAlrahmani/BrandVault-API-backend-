namespace BrandVault.Api.Features.Clients;

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BrandVault.Api.Features.Clients.DTOs;

/// <summary>
/// CRUD endpoints for clients.
///
/// Express equivalent:
///   const router = express.Router();
///   router.use(requireAuth);  // all routes need login
///   router.get('/', clientController.getAll);
///   router.get('/:id', clientController.getById);
///   router.post('/', requireRole('Admin','Manager'), clientController.create);
///   router.put('/:id', requireRole('Admin','Manager'), clientController.update);
///   router.delete('/:id', requireRole('Admin'), clientController.delete);
///   app.use('/api/clients', router);
///
/// [Authorize] on the class = all endpoints require a valid JWT.
/// [Authorize(Roles = "...")] on individual methods = role-based access.
/// </summary>
[ApiController]
[Route("api/clients")]
[Authorize]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }

    /// <summary>
    /// GET /api/clients
    /// Returns all clients. Any authenticated user can view.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var clients = await _clientService.GetAllAsync();
        return Ok(clients);
    }

    /// <summary>
    /// GET /api/clients/:id
    /// Returns a single client by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var client = await _clientService.GetByIdAsync(id);
        return Ok(client);
    }

    /// <summary>
    /// POST /api/clients
    /// Creates a new client. Only Admin and Manager roles.
    /// Sets CreatedById to the currently logged-in user.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create([FromBody] CreateClientRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var client = await _clientService.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetById), new { id = client.Id }, client);
    }

    /// <summary>
    /// PUT /api/clients/:id
    /// Updates a client. Only Admin and Manager roles.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClientRequest request)
    {
        var client = await _clientService.UpdateAsync(id, request);
        return Ok(client);
    }

    /// <summary>
    /// DELETE /api/clients/:id
    /// Deletes a client and cascades to all related workspaces/assets.
    /// Only Admin role — this is destructive.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _clientService.DeleteAsync(id);
        return NoContent();
    }
}
