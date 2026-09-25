using System.Net.Http.Json;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ProductManagement.Testing;

public static class ProblemResponses
{
    public const string MediaType = "application/problem+json";

    public static async Task<ProblemDetails> ReadProblemAsync(HttpResponseMessage response, CancellationToken cancellationToken) =>
        (await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken))!;

    public static async Task<HttpValidationProblemDetails> ReadValidationProblemAsync(HttpResponseMessage response, CancellationToken cancellationToken) =>
        (await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(cancellationToken))!;
}
