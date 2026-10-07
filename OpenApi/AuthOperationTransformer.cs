using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace EmployeeMGR.OpenApi;

internal sealed class AuthOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var metadata = context.Description.ActionDescriptor.EndpointMetadata;

        var allowAnonymous = metadata.OfType<AllowAnonymousAttribute>().Any();

        if (allowAnonymous)
        {
            return Task.CompletedTask;
        }

        var requiresAuthorization = metadata.OfType<AuthorizeAttribute>().Any();

        if (!requiresAuthorization)
        {
            return Task.CompletedTask;
        }

        operation.Security ??= [];

        operation.Security.Add(
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = []
            });

        return Task.CompletedTask;
    }
}