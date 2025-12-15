# .NET Support for SSE

Based on IAsyncEnumerable<T>:

```csharp
public record StockPriceEvent(string Id, string Symbol, decimal Price, DateTime Timestamp);

public class StockService
{
    public async IAsyncEnumerable<StockPriceEvent> GenerateStockPrices(
       [EnumeratorCancellation] CancellationToken cancellationToken)
    {
       var symbols = new[] { "MSFT", "AAPL", "GOOG", "AMZN" };

       while (!cancellationToken.IsCancellationRequested)
       {
          // Pick a random symbol and price
          var symbol = symbols[Random.Shared.Next(symbols.Length)];
          var price  = Math.Round((decimal)(100 + Random.Shared.NextDouble() * 50), 2);

          var id = DateTime.UtcNow.ToString("o");

          yield return new StockPriceEvent(id, symbol, price, DateTime.UtcNow);

          // Wait 2 seconds before sending the next update
          await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
       }
    }
}
```

Implement the SSE endpoint:

```csharp
builder.Services.AddSingleton<StockService>();

app.MapGet("/stocks", (StockService stockService, CancellationToken ct) =>
{
    return TypedResults.ServerSentEvents(
       stockService.GenerateStockPrices(ct),
       eventType: "stockUpdate"
    );
});
```
