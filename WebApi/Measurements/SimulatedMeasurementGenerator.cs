using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace WebApi.Measurements;

public sealed class SimulatedMeasurementGenerator : IMeasurementSource<MeasurementSample>
{
    private readonly double _measurementFrequencyHz;
    private readonly double _variationPercent;
    private readonly Func<double, double> _valueFunction;
    private readonly Random _random;

    public SimulatedMeasurementGenerator(
        double measurementFrequencyHz,
        Func<double, double> valueFunction,
        double variationPercent = 0.0)
    {
        if (measurementFrequencyHz is < 1.0 or > 100.0)
        {
            throw new ArgumentOutOfRangeException(nameof(measurementFrequencyHz), "measurementFrequencyHz must be in range 1..100.");
        }

        if (variationPercent is < 0.0 or > 100.0)
        {
            throw new ArgumentOutOfRangeException(nameof(variationPercent), "variationPercent must be in range 0..100.");
        }

        _valueFunction = valueFunction ?? throw new ArgumentNullException(nameof(valueFunction));
        _measurementFrequencyHz = measurementFrequencyHz;
        _variationPercent = variationPercent;
        _random = Random.Shared;
    }

    public static SimulatedMeasurementGenerator CreateSine(
        double measurementFrequencyHz,
        double signalFrequencyHz,
        double variationPercent = 0.0)
    {
        ValidateSignalFrequency(signalFrequencyHz);

        return new SimulatedMeasurementGenerator(
            measurementFrequencyHz,
            t => Math.Sin(2.0 * Math.PI * signalFrequencyHz * t),
            variationPercent);
    }

    public static SimulatedMeasurementGenerator CreateCosine(
        double measurementFrequencyHz,
        double signalFrequencyHz,
        double variationPercent = 0.0)
    {
        ValidateSignalFrequency(signalFrequencyHz);

        return new SimulatedMeasurementGenerator(
            measurementFrequencyHz,
            t => Math.Cos(2.0 * Math.PI * signalFrequencyHz * t),
            variationPercent);
    }

    private static void ValidateSignalFrequency(double signalFrequencyHz)
    {
        if (signalFrequencyHz is < 0.1 or > 10.0)
        {
            throw new ArgumentOutOfRangeException(nameof(signalFrequencyHz), "signalFrequencyHz must be in range 0.1..10.");
        }
    }

    internal bool TryComputeValue(double t, out double value, double? random01 = null)
    {
        value = 0.0;

        double rawY;
        try
        {
            rawY = _valueFunction(t);
        }
        catch
        {
            return false;
        }

        if (double.IsNaN(rawY) || double.IsInfinity(rawY))
        {
            return false;
        }

        if (_variationPercent == 0.0)
        {
            value = rawY;
            return true;
        }

        var max = _variationPercent / 100.0;

        var u = random01 ?? _random.NextDouble();
        var r = (u * 2.0 - 1.0) * max; // uniform [-max, +max]
        var varied = rawY * (1.0 + r);

        if (double.IsNaN(varied) || double.IsInfinity(varied))
        {
            return false;
        }

        value = varied;
        return true;
    }

    public async IAsyncEnumerable<MeasurementSample> GetMeasurementsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var intervalSeconds = 1.0 / _measurementFrequencyHz;
        var stopwatch = Stopwatch.StartNew();

        long sampleIndex = 0;

        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                yield break;
            }

            var t = stopwatch.Elapsed.TotalSeconds;

            var hasValue = TryComputeValue(t, out var value);
            yield return new MeasurementSample(
                Timestamp: DateTimeOffset.UtcNow,
                HasValue: hasValue,
                Value: hasValue ? value : 0.0);

            var nextDueSeconds = (sampleIndex + 1) * intervalSeconds;
            sampleIndex++;

            var remaining = TimeSpan.FromSeconds(nextDueSeconds) - stopwatch.Elapsed;
            if (remaining > TimeSpan.Zero)
            {
                try
                {
                    await Task.Delay(remaining, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    yield break;
                }
            }
        }
    }
}
