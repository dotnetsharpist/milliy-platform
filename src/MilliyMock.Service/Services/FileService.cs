using Microsoft.AspNetCore.Http;
using MilliyMock.Domain.Exceptions;
using MilliyMock.Service.Interfaces;
using MilliyMock.Shared.Helpers;

namespace MilliyMock.Service.Services;

public class FileService : IFileService
{
    private const string IMAGES = "images";
    private readonly string ROOTPATH = EnvironmentHelper.WebRootPath!;

    private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

    /* -------------------- COMMON -------------------- */

    public bool Delete(string subpath)
    {
        var path = Path.Combine(ROOTPATH, subpath);

        if (!File.Exists(path))
            return false;

        File.Delete(path);
        return true;
    }

    private async Task<string> SaveFileAsync(
        IFormFile file,
        string folder,
        string[] allowedExtensions)
    {
        var extension = Path.GetExtension(file.FileName).ToLower();

        if (!allowedExtensions.Contains(extension))
            throw new MilliyMockException(400, "Invalid file type");

        var fileName = MediaHelper.MakeFileName(file.FileName);
        var subpath = Path.Combine(folder, fileName);
        var fullPath = Path.Combine(ROOTPATH, subpath);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return subpath;

    }

    /* -------------------- IMAGES -------------------- */

    public Task<string> UploadImage(IFormFile image)
        => SaveFileAsync(image, IMAGES, ImageExtensions);
}
