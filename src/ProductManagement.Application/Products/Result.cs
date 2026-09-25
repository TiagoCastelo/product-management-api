namespace ProductManagement.Application.Products;

public readonly record struct Result
{
    private Result(bool isSuccess, ProductError error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public ProductError Error { get; }

    public static Result Success { get; } = new(true, default);

    public static implicit operator Result(ProductError error) => new(false, error);
}

public readonly record struct Result<T>
{
    private readonly T? _value;

    private Result(bool isSuccess, T? value, ProductError error)
    {
        IsSuccess = isSuccess;
        _value = value;
        Error = error;
    }

    public bool IsSuccess { get; }

    public ProductError Error { get; }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException($"Result has no value; error is {Error}.");

    public static implicit operator Result<T>(T value) => new(true, value, default);

    public static implicit operator Result<T>(ProductError error) => new(false, default, error);
}
