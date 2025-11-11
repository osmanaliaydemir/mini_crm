using System.ComponentModel.DataAnnotations;

namespace CRM.Application.Authentication;

public sealed class LoginRequest
{
    [Required]
    [MaxLength(256)]
    public string UserNameOrEmail { get; init; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string Password { get; init; } = string.Empty;
}

