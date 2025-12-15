namespace WebApi.Measurements;

public readonly record struct MeasurementSample(
    DateTimeOffset Timestamp,
    bool HasValue,
    double Value
);
