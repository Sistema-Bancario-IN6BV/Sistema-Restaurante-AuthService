using AuthService_GR.Application.DTOs;
using AuthService_GR.Application.Interfaces;
using AuthService_GR.Application.Exceptions;
using AuthService_GR.Application.Extensions;
using AuthService_GR.Application.Validators;
using AuthService_GR.Domain.Constants;
using AuthService_GR.Domain.Entities;
using AuthService_GR.Domain.Interfaces;
using AuthService_GR.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AuthService_GR.Application.DTOs.Email;
namespace AuthService_GR.Application.Services;
    
public class AuthService(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPasswordHashService passwordHashService,
    IJwtTokenService jwtTokenService,
    ICloudinaryService cloudinaryService,
    IEmailService emailService,
    IConfiguration configuration,
    ILogger<AuthService> logger) : IAuthService
{
    private readonly ICloudinaryService _cloudinaryService = cloudinaryService;
    public async Task<RegisterResponseDto> RegisterAsync(RegisterDto registerDto, string? frontendBaseUrl = null)
    {
        // Verificar si el email ya existe
        if (await userRepository.ExistsByEmailAsync(registerDto.Email))
        {
            logger.LogRegistrationWithExistingEmail();
            throw new BusinessException(ErrorCodes.EMAIL_ALREADY_EXISTS, "Email already exists");
        }

        // Verificar si el username ya existe
        if (await userRepository.ExistsByUsernameAsync(registerDto.Username))
        {
            logger.LogRegistrationWithExistingUsername();
            throw new BusinessException(ErrorCodes.USERNAME_ALREADY_EXISTS, "Username already exists");
        }

        // Validar y manejar la imagen de perfil
        string profilePicturePath;

        if (registerDto.ProfilePicture != null && registerDto.ProfilePicture.Size > 0)
        {
            var (isValid, errorMessage) = FileValidator.ValidateImage(registerDto.ProfilePicture);
            if (!isValid)
            {
                logger.LogWarning($"File validation failed: {errorMessage}");
                throw new BusinessException(ErrorCodes.INVALID_FILE_FORMAT, errorMessage!);
            }

            try
            {
                var fileName = FileValidator.GenerateSecureFileName(registerDto.ProfilePicture.FileName);
                profilePicturePath = await _cloudinaryService.UploadImageAsync(registerDto.ProfilePicture, fileName);
            }
           catch (Exception ex)
            {
                logger.LogImageUploadError();
                logger.LogError(ex, "DEBUG - Cloudinary real error: {Message}", ex.Message);
                throw new BusinessException(ErrorCodes.IMAGE_UPLOAD_FAILED, "Failed to upload profile image");
            }
        }
        else
        {
            profilePicturePath = _cloudinaryService.GetDefaultAvatarUrl();
        }

        // Crear nuevo usuario y entidades relacionadas
        var emailVerificationToken = TokenGeneratorService.GenerateEmailVerificationToken();

        var userId = UuidGenerator.GenerateUserId();
        var userProfileId = UuidGenerator.GenerateUserId();
        var userEmailId = UuidGenerator.GenerateUserId();
        var userRoleId = UuidGenerator.GenerateUserId();

        // Obtener el rol por defecto (USER_ROLE) ya seedado en DB
        var defaultRole = await roleRepository.GetByNameAsync(RoleConstants.CUSTOMER);
        if (defaultRole == null)
        {
            throw new InvalidOperationException($"Default role '{RoleConstants.CUSTOMER}' not found. Ensure seeding runs before registration.");
        }

        var user = new User
        {
            Id = userId,
            Name = registerDto.Name,
            Surname = registerDto.Surname,
            Username = registerDto.Username,
            Email = registerDto.Email.ToLowerInvariant(),
            Password = passwordHashService.HashPassword(registerDto.Password),
            Status = false,
            UserProfile = new UserProfile
            {
                Id = userProfileId,
                UserId = userId,
                ProfilePicture = profilePicturePath,
                Phone = registerDto.Phone
            },
            UserEmail = new UserEmail
            {
                Id = userEmailId,
                UserId = userId,
                EmailVerified = false,
                EmailVerificationToken = emailVerificationToken,
                EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24)
            },
            UserRoles =
            [
                new Domain.Entities.UserRole
                {
                    Id = userRoleId,
                    UserId = userId,
                    RoleId = defaultRole.Id
                }
            ]
        };

        // Guardar usuario y entidades relacionadas
        var createdUser = await userRepository.CreateAsync(user);

        logger.LogUserRegistered(createdUser.Username);

        // Enviar email de verificación en background
        _ = Task.Run(async () =>
        {
            try
            {
                await emailService.SendEmailVerificationAsync(createdUser.Email, createdUser.Username, emailVerificationToken, frontendBaseUrl);
                logger.LogInformation("Verification email sent");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send verification email");
            }
        });

        // Crear respuesta sin JWT - solo confirmación de registro
        return new RegisterResponseDto
        {
            Success = true,
            User = MapToUserResponseDto(createdUser),
            Message = "Usuario registrado exitosamente. Por favor, verifica tu email para activar la cuenta.",
            EmailVerificationRequired = true
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        // Buscar usuario por email o username
        User? user = null;

        if (loginDto.EmailOrUsername.Contains('@'))
        {
            // Es un email
            user = await userRepository.GetByEmailAsync(loginDto.EmailOrUsername.ToLowerInvariant());
        }
        else
        {
            // Es un username
            user = await userRepository.GetByUsernameAsync(loginDto.EmailOrUsername);
        }

        // Verificar si el usuario existe
        if (user == null)
        {
            logger.LogFailedLoginAttempt();
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        // Verificar si el usuario está activo
        if (!user.Status)
        {
            logger.LogFailedLoginAttempt();
            throw new UnauthorizedAccessException("User account is disabled");
        }

        // Verificar contraseña
        if (!passwordHashService.VerifyPassword(loginDto.Password, user.Password))
        {
            logger.LogFailedLoginAttempt();
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        logger.LogUserLoggedIn();

        // Generar token JWT + refresh token
        var token = jwtTokenService.GenerateToken(user);
        var expiryMinutes = int.Parse(configuration["JwtSettings:ExpiryInMinutes"] ?? "30");
        var refreshToken = await CreateRefreshTokenAsync(user.Id);

        // Crear respuesta compacta
        return new AuthResponseDto
        {
            Success = true,
            Message = "Login exitoso",
            Token = token,
            RefreshToken = refreshToken.Token,
            UserDetails = MapToUserDetailsDto(user),
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes)
        };
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto refreshTokenDto)
    {
        var existingToken = await refreshTokenRepository.GetByTokenAsync(refreshTokenDto.RefreshToken);

        if (existingToken == null || !existingToken.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token");
        }

        var user = existingToken.User;

        if (!user.Status)
        {
            throw new UnauthorizedAccessException("User account is disabled");
        }

        // Rotación: revocar el token usado y emitir uno nuevo
        var newRefreshToken = await CreateRefreshTokenAsync(user.Id);
        existingToken.RevokedAt = DateTime.UtcNow;
        existingToken.ReplacedByToken = newRefreshToken.Token;
        await refreshTokenRepository.UpdateAsync(existingToken);

        var token = jwtTokenService.GenerateToken(user);
        var expiryMinutes = int.Parse(configuration["JwtSettings:ExpiryInMinutes"] ?? "30");

        logger.LogInformation("Access token refreshed for user {UserId}", user.Id);

        return new AuthResponseDto
        {
            Success = true,
            Message = "Token renovado exitosamente",
            Token = token,
            RefreshToken = newRefreshToken.Token,
            UserDetails = MapToUserDetailsDto(user),
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes)
        };
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        var existingToken = await refreshTokenRepository.GetByTokenAsync(refreshToken);
        if (existingToken == null || existingToken.RevokedAt != null)
        {
            return;
        }

        existingToken.RevokedAt = DateTime.UtcNow;
        await refreshTokenRepository.UpdateAsync(existingToken);
    }

    private async Task<RefreshToken> CreateRefreshTokenAsync(string userId)
    {
        var refreshTokenExpiryDays = int.Parse(configuration["JwtSettings:RefreshTokenExpiryDays"] ?? "30");

        var refreshToken = new RefreshToken
        {
            Id = UuidGenerator.GenerateRefreshTokenId(),
            UserId = userId,
            Token = jwtTokenService.GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpiryDays),
            CreatedAt = DateTime.UtcNow
        };

        return await refreshTokenRepository.CreateAsync(refreshToken);
    }

    private UserResponseDto MapToUserResponseDto(User user)
    {
        var userRole = user.UserRoles.FirstOrDefault()?.Role?.Name ?? RoleConstants.CUSTOMER;
        return new UserResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            Surname = user.Surname,
            Username = user.Username,
            Email = user.Email,
            ProfilePicture = _cloudinaryService.GetFullImageUrl(user.UserProfile?.ProfilePicture ?? string.Empty),
            Phone = user.UserProfile?.Phone ?? string.Empty,
            Role = userRole,
            Status = user.Status,
            IsEmailVerified = user.UserEmail?.EmailVerified ?? false,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }

    private UserDetailsDto MapToUserDetailsDto(User user)
    {
        return new UserDetailsDto
        {
            Id = user.Id,
            Username = user.Username,
            ProfilePicture = _cloudinaryService.GetFullImageUrl(user.UserProfile?.ProfilePicture ?? string.Empty),
            Role = user.UserRoles.FirstOrDefault()?.Role?.Name ?? RoleConstants.CUSTOMER
        };
    }

    public async Task<EmailResponseDto> VerifyEmailAsync(VerifyEmailDto verifyEmailDto)
    {
        var user = await userRepository.GetByEmailVerificationTokenAsync(verifyEmailDto.Token);
        if (user == null || user.UserEmail == null)
        {
            return new EmailResponseDto
            {
                Success = false,
                Message = "Invalid or expired verification token"
            };
        }

        user.UserEmail.EmailVerified = true;
        user.Status = true;
        user.UserEmail.EmailVerificationToken = null;
        user.UserEmail.EmailVerificationTokenExpiry = null;

        await userRepository.UpdateAsync(user);

        // Enviar email de bienvenida
        try
        {
            await emailService.SendWelcomeEmailAsync(user.Email, user.Username);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send welcome email to {Email}", user.Email);
        }

        logger.LogInformation("Email verified successfully for user {Username}", user.Username);

        return new EmailResponseDto
        {
            Success = true,
            Message = "Email verificado exitosamente",
            Data = new
            {
                email = user.Email,
                verified = true
            }
        };
    }

    public async Task<EmailResponseDto> ResendVerificationEmailAsync(ResendVerificationDto resendDto, string? frontendBaseUrl = null)
    {
        var user = await userRepository.GetByEmailAsync(resendDto.Email);
        if (user == null || user.UserEmail == null)
        {
            return new EmailResponseDto
            {
                Success = false,
                Message = "Usuario no encontrado",
                Data = new { email = resendDto.Email, sent = false }
            };
        }

        if (user.UserEmail.EmailVerified)
        {
            return new EmailResponseDto
            {
                Success = false,
                Message = "El email ya ha sido verificado",
                Data = new { email = user.Email, verified = true }
            };
        }

        // Generar nuevo token
        var newToken = TokenGeneratorService.GenerateEmailVerificationToken();
        user.UserEmail.EmailVerificationToken = newToken;
        user.UserEmail.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);

        await userRepository.UpdateAsync(user);

        // Enviar email
        try
        {
            await emailService.SendEmailVerificationAsync(user.Email, user.Username, newToken, frontendBaseUrl);
            return new EmailResponseDto
            {
                Success = true,
                Message = "Email de verificación enviado exitosamente",
                Data = new { email = user.Email, sent = true }
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to resend verification email to {Email}", user.Email);
            return new EmailResponseDto
            {
                Success = false,
                Message = "Error al enviar el email de verificación",
                Data = new { email = user.Email, sent = false }
            };
        }
    }

    public async Task<EmailResponseDto> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto, string? frontendBaseUrl = null)
    {
        var user = await userRepository.GetByEmailAsync(forgotPasswordDto.Email);
        if (user == null)
        {
            // Por seguridad, siempre devolvemos éxito aunque el usuario no exista
            return new EmailResponseDto
            {
                Success = true,
                Message = "Si el email existe, se ha enviado un enlace de recuperación",
                Data = new { email = forgotPasswordDto.Email, initiated = true }
            };
        }

        // Generar token de reset
        var resetToken = TokenGeneratorService.GeneratePasswordResetToken();

        if (user.UserPasswordReset == null)
        {
            // INSERT explícito: agregar el dependiente nuevo a un principal ya rastreado
            // por navegación haría que EF lo trate como Modified (UPDATE de 0 filas).
            await userRepository.AddPasswordResetAsync(new UserPasswordReset
            {
                Id = UuidGenerator.GenerateUserId(),
                UserId = user.Id,
                PasswordResetToken = resetToken,
                PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1)
            });
        }
        else
        {
            user.UserPasswordReset.PasswordResetToken = resetToken;
            user.UserPasswordReset.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1); // 1 hora para resetear
            await userRepository.UpdateAsync(user);
        }

        // Enviar email
        try
        {
            await emailService.SendPasswordResetAsync(user.Email, user.Username, resetToken, frontendBaseUrl);
            logger.LogInformation("Password reset email sent to {Email}", user.Email);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send password reset email to {Email}", user.Email);
        }

        return new EmailResponseDto
        {
            Success = true,
            Message = "Si el email existe, se ha enviado un enlace de recuperación",
            Data = new { email = forgotPasswordDto.Email, initiated = true }
        };
    }

    public async Task<EmailResponseDto> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
    {
        var user = await userRepository.GetByPasswordResetTokenAsync(resetPasswordDto.Token);
        if (user == null || user.UserPasswordReset == null)
        {
            return new EmailResponseDto
            {
                Success = false,
                Message = "Token de reset inválido o expirado",
                Data = new { token = resetPasswordDto.Token, reset = false }
            };
        }

        // Actualizar contraseña
        user.Password = passwordHashService.HashPassword(resetPasswordDto.NewPassword);
        user.UserPasswordReset.PasswordResetToken = null;
        user.UserPasswordReset.PasswordResetTokenExpiry = null;

        await userRepository.UpdateAsync(user);

        logger.LogInformation("Password reset successfully for user {Username}", user.Username);

        return new EmailResponseDto
        {
            Success = true,
            Message = "Contraseña actualizada exitosamente",
            Data = new { email = user.Email, reset = true }
        };
    }

    public async Task<UserResponseDto?> GetUserByIdAsync(string userId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return null;
        }

        return MapToUserResponseDto(user);
    }

    public async Task<UserResponseDto?> UpdateProfileAsync(string userId, UpdateProfileDto dto)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Name)) user.Name = dto.Name!;
        if (!string.IsNullOrWhiteSpace(dto.Surname)) user.Surname = dto.Surname!;
        if (!string.IsNullOrWhiteSpace(dto.Username)) user.Username = dto.Username!;

        if (!string.IsNullOrWhiteSpace(dto.Phone))
        {
            if (user.UserProfile == null) user.UserProfile = new Domain.Entities.UserProfile { UserId = user.Id, Id = UuidGenerator.GenerateUserId() };
            user.UserProfile.Phone = dto.Phone!;
        }

        if (dto.ProfilePicture != null && dto.ProfilePicture.Size > 0)
        {
            var (isValid, errorMessage) = FileValidator.ValidateImage(dto.ProfilePicture);
            if (!isValid)
            {
                logger.LogWarning("Invalid profile image: {Error}", errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            try
            {
                var fileName = FileValidator.GenerateSecureFileName(dto.ProfilePicture.FileName);
                var uploadedUrl = await _cloudinaryService.UploadImageAsync(dto.ProfilePicture, fileName);
                if (user.UserProfile == null) user.UserProfile = new Domain.Entities.UserProfile { UserId = user.Id, Id = UuidGenerator.GenerateUserId() };
                user.UserProfile.ProfilePicture = uploadedUrl;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to upload profile image for user {UserId}", userId);
                throw;
            }
        }

        var updated = await userRepository.UpdateAsync(user);
        return MapToUserResponseDto(updated);
    }

    public async Task<bool> DeactivateAccountAsync(string userId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        user.Status = false;
        await userRepository.UpdateAsync(user);

        logger.LogInformation("Account deactivated for user {Username}", user.Username);
        return true;
    }
}

