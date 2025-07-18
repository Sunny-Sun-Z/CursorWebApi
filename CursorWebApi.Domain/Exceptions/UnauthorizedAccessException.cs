namespace CursorWebApi.Domain.Exceptions;

public class UnauthorizedAccessException : Exception
{
    public UnauthorizedAccessException(string message = "Access denied.") : base(message)
    {
    }
}
