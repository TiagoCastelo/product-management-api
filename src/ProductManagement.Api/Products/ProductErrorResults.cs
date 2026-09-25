using Microsoft.AspNetCore.Http.HttpResults;

using ProductManagement.Application.Products;

namespace ProductManagement.Api.Products;

public static partial class ProductErrorResults
{
    public static ProblemHttpResult ToProblem(this ProductError error, ILogger logger) => error switch
    {
        ProductError.NotFound => TypedResults.Problem(statusCode: StatusCodes.Status404NotFound, title: "Product not found"),
        ProductError.DuplicateSku => TypedResults.Problem(statusCode: StatusCodes.Status409Conflict, title: "Duplicate SKU"),
        ProductError.ConcurrencyConflict => TypedResults.Problem(
            statusCode: StatusCodes.Status409Conflict,
            title: "Concurrency conflict",
            detail: "The product was modified by another request; reload and retry."),
        ProductError.InsufficientStock => TypedResults.Problem(statusCode: StatusCodes.Status409Conflict, title: "Insufficient stock"),
        ProductError.StockOverflow => TypedResults.Problem(statusCode: StatusCodes.Status409Conflict, title: "Stock overflow"),
        ProductError.IdSpaceExhausted => IdSpaceExhausted(logger),
        _ => TypedResults.Problem(statusCode: StatusCodes.Status500InternalServerError, title: "An unexpected error occurred."),
    };

    private static ProblemHttpResult IdSpaceExhausted(ILogger logger)
    {
        LogIdSpaceExhausted(logger);
        return TypedResults.Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Product ID space exhausted");
    }

    [LoggerMessage(Level = LogLevel.Critical, Message = "Product ID space exhausted")]
    private static partial void LogIdSpaceExhausted(ILogger logger);
}
