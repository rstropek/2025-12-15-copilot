# Unit test specification

## General Requirements

The tests must be comprehensive and deterministic.

## Testing Framework

xUnit

## Test Coverage

### Constructor Validation
- Valid parameters succeed
- MeasurementFrequencyHz below min (1.0) throws `ArgumentOutOfRangeException`
- MeasurementFrequencyHz above max (100.0) throws `ArgumentOutOfRangeException`
- MeasurementFrequencyHz at boundaries (1.0, 100.0) succeed
- Null valueFunction throws `ArgumentNullException`
- VariationPercent negative throws `ArgumentOutOfRangeException`
- VariationPercent above 100 throws `ArgumentOutOfRangeException`
- VariationPercent at boundaries (0.0, 100.0) succeed

### Factory Method Validation
- CreateSine with valid parameters succeeds
- CreateSine with signalFrequencyHz below min (0.1) throws `ArgumentOutOfRangeException`
- CreateSine with signalFrequencyHz above max (10.0) throws `ArgumentOutOfRangeException`
- CreateSine with signalFrequencyHz at boundaries (0.1, 10.0) succeed
- CreateCosine with valid parameters succeeds
- CreateCosine with signalFrequencyHz below min throws `ArgumentOutOfRangeException`
- CreateCosine with signalFrequencyHz above max throws `ArgumentOutOfRangeException`
- CreateCosine with signalFrequencyHz at boundaries (0.1, 10.0) succeed

### Measurement Stream Behavior
- Emits correct number of samples
- Timestamps are increasing
- Timestamps are UTC
- Respects cancellation token

### Timing Accuracy
- Emits at correct measurement rate
- Maintains consistent intervals between samples (within reasonable tolerance)

### Value Function Behavior
- Constant function produces constant values (with zero variation)
- Linear function produces increasing values
- Function returning NaN emits sample with HasValue=false
- Function returning positive infinity emits HasValue=false
- Function returning negative infinity emits HasValue=false
- Function throwing exception emits HasValue=false and continues enumeration
- Function that throws after some calls continues enumeration

### Sine Wave Generation
- At time zero, value is approximately 0 (sin(0) = 0)
- At quarter period, value is approximately 1 (sin(π/2) = 1)
- Completes full cycle over one period

### Cosine Wave Generation
- At time zero, value is approximately 1 (cos(0) = 1)
- At quarter period, value is approximately 0 (cos(π/2) = 0)
- Completes full cycle over one period

### Variation Application
- Zero variation produces exact values
- With variation, values are within expected range (base ± variation%)
- With variation, values actually vary (not all identical)
- Maximum variation (100%) produces values in valid range
- Variation that would produce infinity/NaN results in HasValue=false
