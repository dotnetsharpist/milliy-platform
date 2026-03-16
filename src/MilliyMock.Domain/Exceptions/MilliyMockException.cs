namespace MilliyMock.Domain.Exceptions
{
    public class MilliyMockException(int code = 500, string message = "Something went wrong") : Exception(message)
    {
        public int Code { get; set; } = code;
    }
}