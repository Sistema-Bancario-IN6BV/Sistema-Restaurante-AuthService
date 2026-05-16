using AuthService_GR.Application.Interfaces;

namespace AuthService_GR.Application.DTOs;

public class UpdateProfileDto
{
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? Username { get; set; }
    public string? Phone { get; set; }
    public IFileData? ProfilePicture { get; set; }
}
