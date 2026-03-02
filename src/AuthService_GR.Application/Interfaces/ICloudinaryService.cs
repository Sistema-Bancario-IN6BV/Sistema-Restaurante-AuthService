namespace AuthService_GR.Application.Interfaces;

public interface ICloudinaryService
{
    Task<String> UploadImageAsync(IFileData imageFile, string fileName);
    Task<bool> DeleteImageAsync (string publicId);
    string GetDefaultAvatarUrl();
    string GetFullImageUrl(string imagePath);
}