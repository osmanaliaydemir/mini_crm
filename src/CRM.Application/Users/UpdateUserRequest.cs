using System.ComponentModel.DataAnnotations;

namespace CRM.Application.Users;

public record UpdateUserRequest(
    [Required(ErrorMessage = "Kullanıcı ID gereklidir.")]
    Guid Id,
    
    [Required(ErrorMessage = "E-posta gereklidir.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [MaxLength(256, ErrorMessage = "E-posta en fazla 256 karakter olabilir.")]
    string Email,
    
    [MaxLength(100, ErrorMessage = "Ad en fazla 100 karakter olabilir.")]
    string? FirstName,
    
    [MaxLength(100, ErrorMessage = "Soyad en fazla 100 karakter olabilir.")]
    string? LastName,
    
    [MaxLength(10, ErrorMessage = "Yerel ayar en fazla 10 karakter olabilir.")]
    string? Locale,
    
    bool EmailConfirmed,
    
    bool LockoutEnabled,
    
    DateTimeOffset? LockoutEnd,
    
    IReadOnlyList<string>? Roles);

