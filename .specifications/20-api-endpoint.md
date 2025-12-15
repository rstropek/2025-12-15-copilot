
# API Endpoint Specification: Simulated Measurements via SSE

## Goal

Expose the asynchronous measurement stream produced by `SimulatedMeasurementGenerator` as a Server-Sent Events (SSE) endpoint.

The endpoint offers a fixed set of built-in signals (sine/cosine). Custom functions are out of scope.

The endpoint continuously emits `MeasurementSample` items until the client disconnects or cancels the request.

## Endpoint

- **Method:** `GET`
- **Route:** `/measurements/simulated`
- **Response content type:** `text/event-stream`

## Query Parameters

All parameters are optional unless stated otherwise. Defaults are chosen to provide a sensible demo signal.

- `measurementFrequencyHz` (double)
	- **Meaning:** Sampling rate (how many samples per second are produced).
	- **Valid range:** 1.0 .. 100.0
	- **Default:** 20.0

- `signal` (string)
	- **Meaning:** Which periodic function to generate.
	- **Allowed values:** `sine` | `cosine`
	- **Matching:** Case-insensitive (e.g. `Sine`, `COSINE`)
	- **Default:** `sine`

- `signalFrequencyHz` (double)
	- **Meaning:** Frequency of the generated sine/cosine wave.
	- **Valid range:** 0.1 .. 10.0
	- **Default:** 1.0

- `variationPercent` (double)
	- **Meaning:** Adds uniform multiplicative noise to the raw signal value.
	- **Valid range:** 0.0 .. 100.0
	- **Default:** 0.0

Notes:

- Validation rules must match the generator:
	- `measurementFrequencyHz` must be in 1..100
	- `signalFrequencyHz` must be in 0.1..10
	- `variationPercent` must be in 0..100
	- `signal` must be `sine` or `cosine` (any other value returns 400)

## Behavior

- On successful connection, the server starts streaming measurements immediately.
- The stream is **infinite** (no explicit end-of-stream) and terminates only when:
	- The client disconnects, or
	- The request is cancelled (propagated via `CancellationToken`), or
	- The server shuts down.

- Sample timing:
	- Samples are produced at approximately `measurementFrequencyHz`.
	- The generator uses a stopwatch-based schedule (best-effort pacing; not hard real-time).

## SSE Event Format

The server uses ASP.NET Core’s built-in SSE support (`TypedResults.ServerSentEvents(...)`) with an `IAsyncEnumerable<MeasurementSample>` source (see `.guides/dotnet-sse.md`).

- **Event type:** `measurement`
- **Event data:** JSON representation of a `MeasurementSample` record.

The SSE payload follows standard SSE framing, e.g.:

```
event: measurement
data: {"timestamp":"2025-12-15T12:34:56.7890123+00:00","hasValue":true,"value":0.1234}

```

Notes:

- The endpoint should not buffer the full stream; it must flush events as they are produced.
- The server may emit occasional comment frames (`: keep-alive`) if required by hosting/proxies, but no keep-alive is required by this spec.

## Data Contract

### MeasurementSample

`MeasurementSample` is defined in `WebApi.Measurements` as:

- `timestamp` (string, ISO-8601 with offset)
- `hasValue` (boolean)
- `value` (number)

Semantics:

- `timestamp` is the server-side time when the sample is produced (`DateTimeOffset.UtcNow`).
- `hasValue` indicates whether the signal computation succeeded.
	- When `hasValue=false`, `value` is `0.0`.

Example object:

```json
{
	"timestamp": "2025-12-15T12:34:56.7890123+00:00",
	"hasValue": true,
	"value": -0.9876
}
```

## Responses

### 200 OK

- Streaming response of SSE events as described above.

### 400 Bad Request

Returned when query parameters are invalid (outside allowed ranges or unsupported `signal`).

- Response content type: `application/problem+json`
- Problem details should identify the invalid parameter(s).

### 500 Internal Server Error

Returned for unexpected server failures.

- Response content type: `application/problem+json`

## CORS

The server is configured with permissive CORS (`AllowAnyOrigin/AllowAnyHeader/AllowAnyMethod`). The SSE endpoint must be covered by the default CORS policy.

## Non-Goals

- No authentication/authorization requirements are specified.
- No persistence or replay is required (no `Last-Event-ID` handling mandated).
- No client-driven start/stop commands beyond request cancellation.

