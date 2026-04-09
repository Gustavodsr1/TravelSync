using FluentValidation;
using MediatR;
using TravelSync.SharedKernel.Results;

namespace TravelSync.Trip.API.Infrastructure.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var failures = (await Task.WhenAll(
                validators.Select(v => v.ValidateAsync(context, cancellationToken))))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
            return await next();

        var error = new Error("Validation.Failed", string.Join("; ", failures.Select(f => f.ErrorMessage)));

        return (TResponse)CreateFailureResult(typeof(TResponse), error);
    }

    private static object CreateFailureResult(Type responseType, Error error)
    {
        if (responseType == typeof(Result))
            return Result.Failure(error);

        var valueType = responseType.GetGenericArguments()[0];
        var method = typeof(Result).GetMethod(nameof(Result.Failure), 1, [typeof(Error)])!
            .MakeGenericMethod(valueType);

        return method.Invoke(null, [error])!;
    }
}
