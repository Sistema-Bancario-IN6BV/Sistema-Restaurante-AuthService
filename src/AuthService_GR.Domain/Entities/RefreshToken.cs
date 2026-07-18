using System.ComponentModel.DataAnnotations;

namespace AuthService_GR.Domain.Entities;

public class RefreshToken
{
    [Key]
    [MaxLength(16)]
    public string Id { get; set; } = string.Empty;

    [Required]
    [MaxLength(16)]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string Token { get; set; } = string.Empty;

    [Required]
    public DateTime ExpiresAt { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    [MaxLength(256)]
    public string? ReplacedByToken { get; set; }

    [Required]
    public User User { get; set; } = null!;

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => RevokedAt == null && !IsExpired;
}
