using System.ComponentModel.DataAnnotations;

namespace CRM.Application.Users;

public record ChangePasswordRequest(
    [Required(ErrorMessage = "Kullanıcı ID gereklidir.")]
    Guid UserId,
    
    [Required(ErrorMessage = "Yeni parola gereklidir.")]
    [MinLength(6, ErrorMessage = "Parola en az 6 karakter olmalıdır.")]
    string NewPassword);

