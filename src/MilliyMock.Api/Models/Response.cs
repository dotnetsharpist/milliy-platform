namespace MilliyMock.Models;

public class Response
{
    public int Code { get; set; } = 200;
    public string Message { get; set; } = "Ok👍🏿";
    public object Data { get; set; } = string.Empty;
}
