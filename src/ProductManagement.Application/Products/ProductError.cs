namespace ProductManagement.Application.Products;

public enum ProductError
{
    NotFound,
    DuplicateSku,
    ConcurrencyConflict,
    InsufficientStock,
    StockOverflow,
    IdSpaceExhausted,
}
