using Tindeq.Progressor;

// Need to set up logging.

Guid progressorUuid = Guid.Parse("7e4e1701-1ea6-40c9-9dcc-13d34ffead57");
var progressor = new ProgressorService();

using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
ulong address = await progressor.Discover(progressorUuid, cts.Token);

await progressor.ConnectServiceAsync(address, progressorUuid);
await progressor.GetCharacteristicsAsync();
await progressor.EnableNotificationForDataAsync();