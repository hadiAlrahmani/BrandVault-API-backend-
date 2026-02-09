namespace BrandVault.Api.Models.Enums;

/// <summary>
/// Distinguishes whether a comment or approval action came from
/// an agency team member or an external client via a review link.
/// </summary>
public enum AuthorType
{
    Agency,
    Client
}
