namespace CursorWebApi.Domain.Exceptions;

public class ProductNotFoundException : Exception
{
    public ProductNotFoundException(int id)
        : base($"Product with ID {id} was not found.")
    {
        Id = id;
    }

    public int Id { get; }
}
