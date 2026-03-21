using Microsoft.AspNetCore.Http;

namespace MilliyMock.Service.Interfaces;

public interface IFileService
{
    public Task<string> UploadImage(IFormFile image);
    public bool Delete(string subpath);
}