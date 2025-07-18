namespace CursorWebApi.Domain.Exceptions;

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message)
    {
        Errors = new List<string> { message };
    }

    public ValidationException(IEnumerable<string> errors) : base("One or more validation errors occurred.")
    {
        Errors = errors.ToList();
    }

    public List<string> Errors { get; }
}
