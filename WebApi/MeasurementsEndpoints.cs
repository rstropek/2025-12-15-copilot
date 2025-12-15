using Microsoft.AspNetCore.Http;
using WebApi.Measurements;

namespace WebApi;

public static class MeasurementsEndpoints
{
    public static IEndpointRouteBuilder MapMeasurementsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/measurements/simulated",
                (
                    double? measurementFrequencyHz,
                    string? signal,
                    double? signalFrequencyHz,
                    double? variationPercent,
                    CancellationToken ct) =>
                {
                    var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);

                    var effectiveMeasurementFrequencyHz = measurementFrequencyHz ?? 20.0;
                    var effectiveSignal = signal ?? "sine";
                    var effectiveSignalFrequencyHz = signalFrequencyHz ?? 1.0;
                    var effectiveVariationPercent = variationPercent ?? 0.0;

                    if (!IsFiniteInRange(effectiveMeasurementFrequencyHz, 1.0, 100.0))
                    {
                        errors[nameof(measurementFrequencyHz)] =
                            ["measurementFrequencyHz must be in range 1.0..100.0."];
                    }

                    if (!IsFiniteInRange(effectiveSignalFrequencyHz, 0.1, 10.0))
                    {
                        errors[nameof(signalFrequencyHz)] =
                            ["signalFrequencyHz must be in range 0.1..10.0."];
                    }

                    if (!IsFiniteInRange(effectiveVariationPercent, 0.0, 100.0))
                    {
                        errors[nameof(variationPercent)] =
                            ["variationPercent must be in range 0.0..100.0."];
                    }

                    if (!effectiveSignal.Equals("sine", StringComparison.OrdinalIgnoreCase)
                        && !effectiveSignal.Equals("cosine", StringComparison.OrdinalIgnoreCase))
                    {
                        errors[nameof(signal)] = ["signal must be either 'sine' or 'cosine'."];
                    }

                    if (errors.Count > 0)
                    {
                        return Results.ValidationProblem(errors);
                    }

                    var generator = effectiveSignal.Equals("sine", StringComparison.OrdinalIgnoreCase)
                        ? SimulatedMeasurementGenerator.CreateSine(
                            effectiveMeasurementFrequencyHz,
                            effectiveSignalFrequencyHz,
                            effectiveVariationPercent)
                        : SimulatedMeasurementGenerator.CreateCosine(
                            effectiveMeasurementFrequencyHz,
                            effectiveSignalFrequencyHz,
                            effectiveVariationPercent);

                    return TypedResults.ServerSentEvents(
                        generator.GetMeasurementsAsync(ct),
                        eventType: "measurement");
                })
            .WithDescription("Streams simulated measurements (sine/cosine) as Server-Sent Events (SSE).")
            .Produces(StatusCodes.Status200OK, contentType: "text/event-stream")
            .ProducesValidationProblem();

        return app;
    }

    private static bool IsFiniteInRange(double value, double minInclusive, double maxInclusive)
    {
        return !double.IsNaN(value) && !double.IsInfinity(value) && value >= minInclusive && value <= maxInclusive;
    }
}
