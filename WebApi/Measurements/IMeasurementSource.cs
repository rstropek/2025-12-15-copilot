using System.Collections.Generic;
using System.Threading;

namespace WebApi.Measurements;

public interface IMeasurementSource<TMeasurement>
{
    IAsyncEnumerable<TMeasurement> GetMeasurementsAsync(
    CancellationToken cancellationToken = default);
}
