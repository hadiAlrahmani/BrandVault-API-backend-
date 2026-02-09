namespace BrandVault.Api.Features.Clients;

using Microsoft.EntityFrameworkCore;
using BrandVault.Api.Common;
using BrandVault.Api.Data;
using BrandVault.Api.Features.Clients.DTOs;
using BrandVault.Api.Models;

/// <summary>
/// All client CRUD business logic.
///
/// Express equivalent — this is your clientService.ts:
///   class ClientService {
///     constructor(private prisma: PrismaClient) {}
///     async getAll() { return prisma.client.findMany({ include: { createdBy: true } }); }
///     async create(dto, userId) { return prisma.client.create({ data: { ...dto, createdById: userId } }); }
///   }
///
/// Same pattern as AuthService: inject AppDbContext via constructor, use LINQ for queries.
/// </summary>
public class ClientService : IClientService
{
    private readonly AppDbContext _context;

    public ClientService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all clients with their creator's name.
    /// Express: const clients = await prisma.client.findMany({ include: { createdBy: { select: { name: true } } } });
    /// </summary>
    public async Task<List<ClientResponse>> GetAllAsync()
    {
        var clients = await _context.Clients
            .Include(c => c.CreatedBy)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return clients.Select(MapToResponse).ToList();
    }

    /// <summary>
    /// Get a single client by ID.
    /// Express: const client = await prisma.client.findUnique({ where: { id }, include: { createdBy: true } });
    /// </summary>
    public async Task<ClientResponse> GetByIdAsync(Guid id)
    {
        var client = await _context.Clients
            .Include(c => c.CreatedBy)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (client is null)
        {
            throw new ApiException("Client not found", 404);
        }

        return MapToResponse(client);
    }

    /// <summary>
    /// Create a new client.
    /// Express: const client = await prisma.client.create({ data: { ...dto, createdById: userId } });
    /// </summary>
    public async Task<ClientResponse> CreateAsync(CreateClientRequest request, Guid userId)
    {
        var client = new Client
        {
            Name = request.Name,
            Company = request.Company,
            Email = request.Email.ToLower(),
            Phone = request.Phone,
            Industry = request.Industry,
            CreatedById = userId
        };

        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        // Reload with CreatedBy navigation for the response
        await _context.Entry(client).Reference(c => c.CreatedBy).LoadAsync();

        return MapToResponse(client);
    }

    /// <summary>
    /// Update an existing client.
    /// Express: const client = await prisma.client.update({ where: { id }, data: dto });
    /// </summary>
    public async Task<ClientResponse> UpdateAsync(Guid id, UpdateClientRequest request)
    {
        var client = await _context.Clients
            .Include(c => c.CreatedBy)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (client is null)
        {
            throw new ApiException("Client not found", 404);
        }

        client.Name = request.Name;
        client.Company = request.Company;
        client.Email = request.Email.ToLower();
        client.Phone = request.Phone;
        client.Industry = request.Industry;

        await _context.SaveChangesAsync();

        return MapToResponse(client);
    }

    /// <summary>
    /// Delete a client. Cascades to workspaces → assets → versions/comments/approvals.
    /// Express: await prisma.client.delete({ where: { id } });
    /// </summary>
    public async Task DeleteAsync(Guid id)
    {
        var client = await _context.Clients.FindAsync(id);

        if (client is null)
        {
            throw new ApiException("Client not found", 404);
        }

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Map Client entity → ClientResponse DTO.
    /// Never expose the raw entity — DTOs control what the API returns.
    /// </summary>
    private static ClientResponse MapToResponse(Client client)
    {
        return new ClientResponse
        {
            Id = client.Id,
            Name = client.Name,
            Company = client.Company,
            Email = client.Email,
            Phone = client.Phone,
            Industry = client.Industry,
            CreatedById = client.CreatedById,
            CreatedByName = client.CreatedBy.Name,
            CreatedAt = client.CreatedAt
        };
    }
}
