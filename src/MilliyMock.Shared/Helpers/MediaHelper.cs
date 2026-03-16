namespace MilliyMock.Shared.Helpers;

public class MediaHelper
{
    public static string MakeFileName(string filename)
    {
        var fileInfo = new FileInfo(filename);
        var extension = fileInfo.Extension;
        var name = "FILE_" + Guid.NewGuid() + extension;
        return name;
    }

    public static string[] GetImageExtensions()
    {
        return new string[]
        {
            // JPG files
            ".jpg", ".jpeg",
            // Png files
            ".png",
            // Bmp files
            ".bmp",
            // Svg files
            ".svg"
        };
    }

    public static string[] GetFileExtensions()
    {
        return
        [
            ".pdf", "docx"
        ];
    }
}
