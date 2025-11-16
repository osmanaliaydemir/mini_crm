using System.ComponentModel.DataAnnotations;

namespace CRM.Application.Users;

public record CreateUserRequest(
    [Required(ErrorMessage = "Kullanıcı adı gereklidir.")]
    [MinLength(3, ErrorMessage = "Kullanıcı adı en az 3 karakter olmalıdır.")]
    [MaxLength(50, ErrorMessage = "Kullanıcı adı en fazla 50 karakter olabilir.")]
    string UserName,
    
    [Required(ErrorMessage = "E-posta gereklidir.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [MaxLength(256, ErrorMessage = "E-posta en fazla 256 karakter olabilir.")]
    string Email,
    
    [Required(ErrorMessage = "Parola gereklidir.")]
    [MinLength(6, ErrorMessage = "Parola en az 6 karakter olmalıdır.")]
    string Password,
    
    [MaxLength(100, ErrorMessage = "Ad en fazla 100 karakter olabilir.")]
    string? FirstName,
    
    [MaxLength(100, ErrorMessage = "Soyad en fazla 100 karakter olabilir.")]
    string? LastName,
    
    [MaxLength(10, ErrorMessage = "Yerel ayar en fazla 10 karakter olabilir.")]
    string? Locale,
    
    IReadOnlyList<string>? Roles);

