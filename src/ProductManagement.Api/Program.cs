using System.Text.Json;

using ProductManagement.Api.ErrorHandling;
using ProductManagement.Api.Products;
using ProductManagement.Application;
using ProductManagement.Infrastructure;

using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddInfrastructure();
builder.Services.AddApplication();
builder.Services.AddOpenApi();
builder.Services.AddValidation();
builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
{
	if (context.ProblemDetails is HttpValidationProblemDetails validationProblemDetails)
	{
		var camelCasedErrors = validationProblemDetails.Errors.ToDictionary(
			pair => JsonNamingPolicy.CamelCase.ConvertName(pair.Key),
			pair => pair.Value);
		validationProblemDetails.Errors.Clear();
		foreach (var (key, value) in camelCasedErrors)
		{
			validationProblemDetails.Errors.Add(key, value);
		}
	}
});
builder.Services.AddExceptionHandler<BadHttpRequestExceptionHandler>();
builder.Services.AddExceptionHandler<UnhandledExceptionHandler>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	app.MapScalarApiReference();
}

app.MapDefaultEndpoints();
app.MapProductEndpoints();

app.Run();
