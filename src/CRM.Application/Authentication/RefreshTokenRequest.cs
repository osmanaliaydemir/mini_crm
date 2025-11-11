using System.ComponentModel.DataAnnotations;

namespace CRM.Application.Authentication;

public sealed class RefreshTokenRequest
{
    [Required]
    public Guid UserId { get; init; }

    [Required]
    [MaxLength(512)]
    public string RefreshToken { get; init; } = string.Empty;
}

