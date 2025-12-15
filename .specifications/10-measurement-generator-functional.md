# Measurement Generator Specification

## Purpose

Provide a C# measurement generator that produces an asynchronous stream of simulated measurements at a configurable rate.
The generator computes values using a user-provided function (or built-in sine/cosine shortcuts) and applies optional random variation.

## Inputs

1. Measurement frequency (Hz)
   * Type: double
   * Range: 1..100
   * Meaning: how many measurements per second to emit.
2. Signal frequency (Hz)
   * Type: double
   * Range: 0.1..10.0
   * Meaning: number of full cycles per second for sine/cosine helper overloads; not available to custom functions
3.	Value function
   * Type: `Func<double, double>` for the general overload.
   * Called with an t value (elapsed time in seconds since generator starts), returns raw y.
4.	Variation percentage
   * Type: double
   * Range: 0..100
   * Meaning: apply random multiplicative deviation to the raw y: `final = raw * (1 + r)` where r is uniform in [-variationPct/100, +variationPct/100]

## Overloads

Provide:

* A general method accepting Func<double,double>.
* Two convenience overloads/factories:
  * Sine generator (no function parameter needed).
  * Cosine generator (no function parameter needed).

## Mapping (time base)

* The stream advances in fixed time steps based on measurement frequency
* Let t be elapsed time in seconds since start.
* Recompute t from a stopwatch at each emission to avoid drift.
* For the general function overload, the component passes t
* For sin/cos overloads, `x = 2π * signalFrequencyHz * t`

## Output data

Each emitted item must include:

* A timestamp (`DateTimeOffset.UtcNow` at emission time).
* Either:
  * a numeric value, or
  * an indicator that no result exists for this measurement (e.g., division by zero, NaN, Infinity, or thrown exception in the function). After applying variation, re-check validity

## Device-replaceable design

Expose a reusable interface so the simulated generator can be replaced later by a real measurement device implementation without changing consuming code.

## Validation & error behavior

* If any input is out of range, throw `ArgumentOutOfRangeException`.
* If measurementFrequencyHz or signalFrequencyHz are invalid, fail fast during creation/start.
* If the value function produces an invalid number (NaN, Infinity) or throws, mark the sample as NoValue rather than failing the whole stream.
