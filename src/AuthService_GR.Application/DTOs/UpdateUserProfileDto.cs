using AuthService_GR.Application.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace AuthService_GR.Application.DTOs;

public class UpdateUserProfileDto
{
    [MaxLength(25, ErrorMessage = "El nombre no puede tener más de 25 caracteres")]
    public string? Name { get; set; }

    [MaxLength(25, ErrorMessage = "El apellido no puede tener más de 25 caracteres")]
    public string? Surname { get; set; }

    [MaxLength(25, ErrorMessage = "El nombre de usuario no puede tener más de 25 caracteres")]
    public string? Username { get; set; }

    [RegularExpression(@"^\d{8}$", ErrorMessage = "El teléfono debe contener exactamente 8 números")]
    public string? Phone { get; set; }

    public IFileData? ProfilePicture { get; set; }
}