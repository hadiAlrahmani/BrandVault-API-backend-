namespace BrandVault.Api.Features.Clients;

using BrandVault.Api.Features.Clients.DTOs;

/// <summary>
/// Contract for the client service.
///
/// TypeScript equivalent:
///   interface IClientService {
///     getAll(): Promise&lt;ClientResponse[]&gt;;
///     getById(id: string): Promise&lt;ClientResponse&gt;;
///     create(dto: CreateClientRequest, userId: string): Promise&lt;ClientResponse&gt;;
///     update(id: string, dto: UpdateClientRequest): Promise&lt;ClientResponse&gt;;
///     delete(id: string): Promise&lt;void&gt;;
///   }
/// </summary>
public interface IClientService
{
    Task<List<ClientResponse>> GetAllAsync();
    Task<ClientResponse> GetByIdAsync(Guid id);
    Task<ClientResponse> CreateAsync(CreateClientRequest request, Guid userId);
    Task<ClientResponse> UpdateAsync(Guid id, UpdateClientRequest request);
    Task DeleteAsync(Guid id);
}
