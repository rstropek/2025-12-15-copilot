
# Technical specification

## Public API surface

Interface

```cs
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

public interface IMeasurementSource<TMeasurement>
{
    IAsyncEnumerable<TMeasurement> GetMeasurementsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default);
}
```

POCO measurement record / struct

```cs
public readonly record struct MeasurementSample(
    DateTimeOffset Timestamp,
    bool HasValue,
    double Value
);
```

* `HasValue=false` indicates “no result exists”.
* Value can be left as 0 when HasValue=false (consumer must check HasValue).

Generator class

```cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

public sealed class SimulatedMeasurementGenerator : IMeasurementSource<MeasurementSample>
{
    // ctor for general function
    public SimulatedMeasurementGenerator(
        double measurementFrequencyHz,
        Func<double, double> valueFunction,
        double variationPercent = 0.0);

    // convenience sin
    public static SimulatedMeasurementGenerator CreateSine(
        double measurementFrequencyHz,
        double signalFrequencyHz,
        double variationPercent = 0.0);

    // convenience cos
    public static SimulatedMeasurementGenerator CreateCosine(
        double measurementFrequencyHz,
        double signalFrequencyHz,
        double variationPercent = 0.0);

    public IAsyncEnumerable<MeasurementSample> GetMeasurementsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default);
}
```

To facilitate unit testing, the implementation should include an internal method (internals-visible-to with test project) that takes a time value `t` (double) and returns the calculated measurement value (or indicates invalid result). This allows testing the calculation logic directly without needing to enumerate the async stream, making tests faster and more deterministic.

`CreateSine`/`CreateCosine` are static factories.

## Internal behavior details

Parameter validation

* measurementFrequencyHz must be 1..100
* signalFrequencyHz must be 0.1..10
* variationPercent must be 0..100
* valueFunction must not be null

Throw `ArgumentOutOfRangeException` / `ArgumentNullException` immediately.

## Timing / scheduling

* Compute interval: `TimeSpan interval = TimeSpan.FromSeconds(1.0 / measurementFrequencyHz)`
* Use a monotonic clock (Stopwatch) to compute t robustly.
* Use a drift-correcting loop:
  * Track next due time based on start time + n * interval.
  * `await Task.Delay(remaining, cancellationToken)` (if remaining > 0).
  * This keeps long-running enumerations aligned even if some iterations take longer.

## Computing t and y

* t = stopwatch.Elapsed.TotalSeconds
* rawY:
  * try call valueFunction(t)
  * if exception => emit HasValue=false
  * else if double.IsNaN(rawY) => HasValue=false
  * else if double.IsInfinity(rawY) => HasValue=false
  * else apply variation. 

## Applying variation

* if variationPercent == 0: skip.
* else:
  * max = variationPercent / 100.0
  * r = (random.NextDouble() * 2.0 - 1.0) * max  // uniform [-max, +max]
  * varied = rawY * (1.0 + r)
  * if varied becomes NaN/Infinity (unlikely unless rawY huge) => mark NoValue.

## Output values

* Timestamp = DateTimeOffset.UtcNow at calculation time

## Dependencies

No external dependencies, only BCL types: System, System.Threading, System.Collections.Generic, etc.
