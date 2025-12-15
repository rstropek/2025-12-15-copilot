using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using WebApi.Measurements;
using Xunit;

namespace WebApiTests;

public sealed class SimulatedMeasurementGeneratorTests
{
    public static IEnumerable<object[]> GeneratorFactories()
    {
        yield return
        [
            "Sine",
            (Func<double, double, double, SimulatedMeasurementGenerator>)SimulatedMeasurementGenerator.CreateSine,
        ];

        yield return
        [
            "Cosine",
            (Func<double, double, double, SimulatedMeasurementGenerator>)SimulatedMeasurementGenerator.CreateCosine,
        ];
    }

    public static IEnumerable<object[]> InvalidFunctionResults()
    {
        yield return [double.NaN];
        yield return [double.PositiveInfinity];
        yield return [double.NegativeInfinity];
    }

    public static IEnumerable<object[]> WaveValueAtTimeCases()
    {
        // t=0
        yield return
        [
            "Sine",
            (Func<double, double, double, SimulatedMeasurementGenerator>)SimulatedMeasurementGenerator.CreateSine,
            1.0,
            0.0,
            0.0,
            1e-12,
        ];

        yield return
        [
            "Cosine",
            (Func<double, double, double, SimulatedMeasurementGenerator>)SimulatedMeasurementGenerator.CreateCosine,
            1.0,
            0.0,
            1.0,
            1e-6,
        ];

        // quarter period (t = 1 / (4f)) for f=2 => t=0.125
        const double f = 2.0;
        var quarterPeriod = 1.0 / (4.0 * f);

        yield return
        [
            "Sine",
            (Func<double, double, double, SimulatedMeasurementGenerator>)SimulatedMeasurementGenerator.CreateSine,
            f,
            quarterPeriod,
            1.0,
            1e-6,
        ];

        yield return
        [
            "Cosine",
            (Func<double, double, double, SimulatedMeasurementGenerator>)SimulatedMeasurementGenerator.CreateCosine,
            f,
            quarterPeriod,
            0.0,
            1e-6,
        ];
    }

    public static IEnumerable<object[]> FullCycleCases()
    {
        const double f = 1.5;
        yield return
        [
            "Sine",
            (Func<double, double, double, SimulatedMeasurementGenerator>)SimulatedMeasurementGenerator.CreateSine,
            f,
        ];
        yield return
        [
            "Cosine",
            (Func<double, double, double, SimulatedMeasurementGenerator>)SimulatedMeasurementGenerator.CreateCosine,
            f,
        ];
    }

    public static IEnumerable<object[]> VariationRangeCases()
    {
        yield return [20.0, 8.0, 12.0];
        yield return [100.0, 0.0, 20.0];
    }

    [Fact]
    public void Ctor_ValidParameters_Succeeds()
    {
        _ = new SimulatedMeasurementGenerator(10.0, _ => 1.23, 0.0);
    }

    [Theory]
    [InlineData(0.999)]
    [InlineData(0.0)]
    [InlineData(-1.0)]
    [InlineData(100.0001)]
    [InlineData(101.0)]
    public void Ctor_MeasurementFrequency_OutOfRange_Throws(double measurementFrequencyHz)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new SimulatedMeasurementGenerator(measurementFrequencyHz, _ => 0.0));
    }

    [Theory]
    [InlineData(1.0)]
    [InlineData(100.0)]
    public void Ctor_MeasurementFrequency_AtBoundaries_Succeeds(double measurementFrequencyHz)
    {
        _ = new SimulatedMeasurementGenerator(measurementFrequencyHz, _ => 0.0);
    }

    [Fact]
    public void Ctor_NullValueFunction_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new SimulatedMeasurementGenerator(10.0, null!));
    }

    [Theory]
    [InlineData(-0.0001)]
    [InlineData(-1.0)]
    [InlineData(100.0001)]
    [InlineData(101.0)]
    public void Ctor_VariationPercent_OutOfRange_Throws(double variationPercent)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new SimulatedMeasurementGenerator(10.0, _ => 0.0, variationPercent));
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(100.0)]
    public void Ctor_VariationPercent_AtBoundaries_Succeeds(double variationPercent)
    {
        _ = new SimulatedMeasurementGenerator(10.0, _ => 0.0, variationPercent);
    }

    [Theory]
    [MemberData(nameof(GeneratorFactories))]
    public void CreateWaveGenerator_ValidParameters_Succeeds(
        string kind,
        Func<double, double, double, SimulatedMeasurementGenerator> create)
    {
        _ = kind;
        create(10.0, 1.0, 0.0);
    }

    [Theory]
    [MemberData(nameof(GeneratorFactories))]
    public void CreateWaveGenerator_SignalFrequency_OutOfRange_Throws(
        string kind,
        Func<double, double, double, SimulatedMeasurementGenerator> create)
    {
        _ = kind;
        Assert.Throws<ArgumentOutOfRangeException>(() => create(10.0, 0.0999, 0.0));
        Assert.Throws<ArgumentOutOfRangeException>(() => create(10.0, 0.0, 0.0));
        Assert.Throws<ArgumentOutOfRangeException>(() => create(10.0, -1.0, 0.0));
        Assert.Throws<ArgumentOutOfRangeException>(() => create(10.0, 10.0001, 0.0));
        Assert.Throws<ArgumentOutOfRangeException>(() => create(10.0, 11.0, 0.0));
    }

    [Theory]
    [MemberData(nameof(GeneratorFactories))]
    public void CreateWaveGenerator_SignalFrequency_AtBoundaries_Succeeds(
        string kind,
        Func<double, double, double, SimulatedMeasurementGenerator> create)
    {
        _ = kind;
        create(10.0, 0.1, 0.0);
        create(10.0, 10.0, 0.0);
    }

    [Fact]
    public void TryComputeValue_ConstantFunction_ZeroVariation_ProducesConstantValues()
    {
        var gen = new SimulatedMeasurementGenerator(10.0, _ => 42.0, 0.0);

        Assert.True(gen.TryComputeValue(0.0, out var v0));
        Assert.True(gen.TryComputeValue(1.0, out var v1));

        Assert.Equal(42.0, v0);
        Assert.Equal(42.0, v1);
    }

    [Fact]
    public void TryComputeValue_LinearFunction_ProducesIncreasingValues()
    {
        var gen = new SimulatedMeasurementGenerator(10.0, t => t, 0.0);

        Assert.True(gen.TryComputeValue(0.1, out var v0));
        Assert.True(gen.TryComputeValue(0.2, out var v1));

        Assert.True(v1 > v0);
    }

    [Theory]
    [MemberData(nameof(InvalidFunctionResults))]
    public void TryComputeValue_FunctionReturningInvalidNumber_ReportsNoValue(double invalid)
    {
        var gen = new SimulatedMeasurementGenerator(10.0, _ => invalid, 0.0);
        Assert.False(gen.TryComputeValue(0.0, out _));
    }

    [Fact]
    public void TryComputeValue_FunctionThrowingException_ReportsNoValue_AndDoesNotThrow()
    {
        var gen = new SimulatedMeasurementGenerator(10.0, _ => throw new InvalidOperationException("boom"), 0.0);
        Assert.False(gen.TryComputeValue(0.0, out _));
        Assert.False(gen.TryComputeValue(1.0, out _));
    }

    [Fact]
    public void TryComputeValue_FunctionThrowsAfterSomeCalls_ContinuesReturningNoValue()
    {
        var calls = 0;
        var gen = new SimulatedMeasurementGenerator(
            10.0,
            _ =>
            {
                calls++;
                return calls <= 2 ? 1.0 : throw new InvalidOperationException("boom");
            },
            0.0);

        Assert.True(gen.TryComputeValue(0.0, out var v0));
        Assert.Equal(1.0, v0);

        Assert.True(gen.TryComputeValue(0.1, out var v1));
        Assert.Equal(1.0, v1);

        Assert.False(gen.TryComputeValue(0.2, out _));
        Assert.False(gen.TryComputeValue(0.3, out _));
    }

    [Theory]
    [MemberData(nameof(WaveValueAtTimeCases))]
    public void WaveGeneration_ValueAtTime_IsApproximatelyExpected(
        string kind,
        Func<double, double, double, SimulatedMeasurementGenerator> create,
        double signalFrequencyHz,
        double t,
        double expected,
        double tolerance)
    {
        _ = kind;
        var gen = create(10.0, signalFrequencyHz, 0.0);
        Assert.True(gen.TryComputeValue(t, out var v));
        Assert.InRange(v, expected - tolerance, expected + tolerance);
    }

    [Theory]
    [MemberData(nameof(FullCycleCases))]
    public void WaveGeneration_CompletesFullCycle_OverOnePeriod(
        string kind,
        Func<double, double, double, SimulatedMeasurementGenerator> create,
        double signalFrequencyHz)
    {
        _ = kind;
        var gen = create(10.0, signalFrequencyHz, 0.0);
        var period = 1.0 / signalFrequencyHz;

        Assert.True(gen.TryComputeValue(0.0, out var v0));
        Assert.True(gen.TryComputeValue(period, out var v1));

        Assert.InRange(v1 - v0, -1e-10, 1e-10);
    }

    [Fact]
    public void Variation_ZeroVariation_ProducesExactValues()
    {
        var gen = new SimulatedMeasurementGenerator(10.0, _ => 10.0, 0.0);
        Assert.True(gen.TryComputeValue(0.0, out var v));
        Assert.Equal(10.0, v);
    }

    [Theory]
    [MemberData(nameof(VariationRangeCases))]
    public void Variation_WithVariation_StaysWithinExpectedRange(
        double variationPercent,
        double expectedMin,
        double expectedMax)
    {
        var gen = new SimulatedMeasurementGenerator(10.0, _ => 10.0, variationPercent);

        Assert.True(gen.TryComputeValue(0.0, out var vMin, random01: 0.0));
        Assert.True(gen.TryComputeValue(0.0, out var vMax, random01: 1.0));

        Assert.Equal(expectedMin, vMin, precision: 12);
        Assert.Equal(expectedMax, vMax, precision: 12);
    }

    [Fact]
    public void Variation_WithVariation_ValuesActuallyVary()
    {
        var gen = new SimulatedMeasurementGenerator(10.0, _ => 10.0, 50.0);

        Assert.True(gen.TryComputeValue(0.0, out var v0, random01: 0.1));
        Assert.True(gen.TryComputeValue(0.0, out var v1, random01: 0.9));

        Assert.NotEqual(v0, v1);
    }

    [Fact]
    public void Variation_ThatWouldProduceInfinity_ReportsNoValue()
    {
        var gen = new SimulatedMeasurementGenerator(10.0, _ => double.MaxValue, 100.0);

        // random01=1 => r=+1 => multiply by 2 => overflow => Infinity => HasValue=false
        Assert.False(gen.TryComputeValue(0.0, out _, random01: 1.0));
    }

    [Fact]
    public async Task Stream_EmitsCorrectNumberOfSamples()
    {
        var gen = new SimulatedMeasurementGenerator(50.0, _ => 1.0, 0.0);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        var samples = await TakeAsync(gen.GetMeasurementsAsync(cts.Token), count: 10, cts.Token);

        Assert.Equal(10, samples.Count);
    }

    [Fact]
    public async Task Stream_TimestampsAreIncreasing_AndUtc()
    {
        var gen = new SimulatedMeasurementGenerator(25.0, _ => 1.0, 0.0);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        var samples = await TakeAsync(gen.GetMeasurementsAsync(cts.Token), count: 10, cts.Token);

        for (var i = 1; i < samples.Count; i++)
        {
            Assert.True(samples[i].Timestamp > samples[i - 1].Timestamp);
            Assert.Equal(TimeSpan.Zero, samples[i].Timestamp.Offset);
        }

        Assert.Equal(TimeSpan.Zero, samples[0].Timestamp.Offset);
    }

    [Fact]
    public async Task Stream_RespectsCancellationToken_WhenAlreadyCanceled()
    {
        var gen = new SimulatedMeasurementGenerator(10.0, _ => 1.0, 0.0);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var receivedAny = false;
        await foreach (var _ in gen.GetMeasurementsAsync(cts.Token))
        {
            receivedAny = true;
            break;
        }

        Assert.False(receivedAny);
    }

    [Fact]
    public async Task Stream_RespectsCancellationToken_DuringEnumeration()
    {
        var gen = new SimulatedMeasurementGenerator(50.0, _ => 1.0, 0.0);

        using var cts = new CancellationTokenSource();

        var count = 0;
        await foreach (var _ in gen.GetMeasurementsAsync(cts.Token))
        {
            count++;
            if (count >= 5)
            {
                cts.Cancel();
            }
        }

        Assert.InRange(count, 5, 20);
    }

    [Fact]
    public async Task Timing_EmitsAtCorrectMeasurementRate_WithinTolerance()
    {
        const double measurementFrequencyHz = 20.0; // 50ms
        var intervalMs = 1000.0 / measurementFrequencyHz;

        var gen = new SimulatedMeasurementGenerator(measurementFrequencyHz, _ => 1.0, 0.0);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        var receiptTimes = new List<double>();
        var sw = Stopwatch.StartNew();

        await foreach (var _ in gen.GetMeasurementsAsync(cts.Token))
        {
            receiptTimes.Add(sw.Elapsed.TotalMilliseconds);
            if (receiptTimes.Count >= 10)
            {
                break;
            }
        }

        // total time between first and last should be ~ (n-1) * interval
        var total = receiptTimes[^1] - receiptTimes[0];
        var expected = (receiptTimes.Count - 1) * intervalMs;

        Assert.InRange(total, expected - 200.0, expected + 400.0);
    }

    [Fact]
    public async Task Timing_MaintainsConsistentIntervals_WithinTolerance()
    {
        const double measurementFrequencyHz = 25.0; // 40ms
        var intervalMs = 1000.0 / measurementFrequencyHz;

        var gen = new SimulatedMeasurementGenerator(measurementFrequencyHz, _ => 1.0, 0.0);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        var receiptTimes = new List<double>();
        var sw = Stopwatch.StartNew();

        await foreach (var _ in gen.GetMeasurementsAsync(cts.Token))
        {
            receiptTimes.Add(sw.Elapsed.TotalMilliseconds);
            if (receiptTimes.Count >= 12)
            {
                break;
            }
        }

        for (var i = 1; i < receiptTimes.Count; i++)
        {
            var delta = receiptTimes[i] - receiptTimes[i - 1];
            Assert.InRange(delta, intervalMs - 25.0, intervalMs + 80.0);
        }
    }

    private static async Task<List<MeasurementSample>> TakeAsync(
        IAsyncEnumerable<MeasurementSample> source,
        int count,
        CancellationToken cancellationToken)
    {
        var result = new List<MeasurementSample>(capacity: count);

        await foreach (var item in source.WithCancellation(cancellationToken))
        {
            result.Add(item);
            if (result.Count >= count)
            {
                break;
            }
        }

        return result;
    }
}
