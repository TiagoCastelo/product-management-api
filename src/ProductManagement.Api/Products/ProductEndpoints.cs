using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Http.HttpResults;

using ProductManagement.Application.Products;
using ProductManagement.Domain.Products;

namespace ProductManagement.Api.Products;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/products").WithTags("Products");

        group.MapGet("/", ListAll);
        group.MapPost("/", Create);
        group.MapGet("/{id:int}", GetById);
        group.MapPut("/{id:int}", Update);
        group.MapDelete("/{id:int}", Delete);
        group.MapPost("/{id:int}/decrement-stock/{quantity}", DecrementStock);
        group.MapPost("/{id:int}/add-to-stock/{quantity}", AddStock);
        group.MapGet("/search", Search);
        group.MapGet("/stock-level", StockLevel);

        return endpoints;
    }

    private static async Task<Ok<IReadOnlyList<ProductDto>>> ListAll(ProductService service, CancellationToken cancellationToken) =>
        TypedResults.Ok(await service.ListAllAsync(cancellationToken));

    private static async Task<Results<Created<ProductDto>, ValidationProblem, ProblemHttpResult>> Create(
        CreateProductRequest request, ProductService service, ILogger<Program> logger, CancellationToken cancellationToken)
    {
        var command = new CreateProductCommand(request.Sku!, request.Name!, request.Description, request.Price!.Value, request.Stock!.Value);
        var result = await service.CreateAsync(command, cancellationToken);
        return result.IsSuccess
            ? TypedResults.Created($"/api/products/{result.Value.Id}", result.Value)
            : result.Error.ToProblem(logger);
    }

    private static async Task<Results<Ok<ProductDto>, ProblemHttpResult>> GetById(int id, ProductService service, ILogger<Program> logger, CancellationToken cancellationToken)
    {
        var result = await service.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.Error.ToProblem(logger);
    }

    private static async Task<Results<Ok<ProductDto>, ValidationProblem, ProblemHttpResult>> Update(
        int id, UpdateProductRequest request, ProductService service, ILogger<Program> logger, CancellationToken cancellationToken)
    {
        var command = new UpdateProductDetailsCommand(request.Name!, request.Description, request.Price!.Value, request.RowVersion!);
        var result = await service.UpdateDetailsAsync(id, command, cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.Error.ToProblem(logger);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> Delete(int id, ProductService service, ILogger<Program> logger, CancellationToken cancellationToken)
    {
        var result = await service.DeleteAsync(id, cancellationToken);
        return result.IsSuccess ? TypedResults.NoContent() : result.Error.ToProblem(logger);
    }

    private static async Task<Results<Ok<ProductDto>, ValidationProblem, ProblemHttpResult>> DecrementStock(
        int id, [Range(1, int.MaxValue)] int quantity, ProductService service, ILogger<Program> logger, CancellationToken cancellationToken)
    {
        var result = await service.DecrementStockAsync(id, quantity, cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.Error.ToProblem(logger);
    }

    private static async Task<Results<Ok<ProductDto>, ValidationProblem, ProblemHttpResult>> AddStock(
        int id, [Range(1, int.MaxValue)] int quantity, ProductService service, ILogger<Program> logger, CancellationToken cancellationToken)
    {
        var result = await service.AddStockAsync(id, quantity, cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.Error.ToProblem(logger);
    }

    private static async Task<Results<Ok<IReadOnlyList<ProductDto>>, ValidationProblem>> Search(string? name, ProductService service, CancellationToken cancellationToken)
    {
        var term = name?.Trim();
        if (string.IsNullOrEmpty(term) || term.Length > ProductRules.NameMaxLength)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["name"] = [$"Name is required and must not exceed {ProductRules.NameMaxLength} characters."],
            });
        }

        return TypedResults.Ok(await service.SearchByNameAsync(term, cancellationToken));
    }

    private static async Task<Results<Ok<IReadOnlyList<ProductDto>>, ValidationProblem>> StockLevel(
        [Range(0, int.MaxValue)] int? min, [Range(0, int.MaxValue)] int? max, ProductService service, CancellationToken cancellationToken)
    {
        if (min is null || max is null)
        {
            var missing = new Dictionary<string, string[]>();
            if (min is null)
            {
                missing["min"] = ["Min is required."];
            }

            if (max is null)
            {
                missing["max"] = ["Max is required."];
            }

            return TypedResults.ValidationProblem(missing);
        }

        if (min > max)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["min"] = ["Min must not be greater than max."],
            });
        }

        return TypedResults.Ok(await service.ListByStockRangeAsync(min.Value, max.Value, cancellationToken));
    }
}
