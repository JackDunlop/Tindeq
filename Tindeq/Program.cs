using System.Reflection.PortableExecutable;
using System.Threading.Channels;
using Tindeq.Progressor;
using Tindeq.Progressor.Enum;
using Tindeq.Progressor.Service;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

// Need to set up logging.

Guid progressorUuid = Guid.Parse("7e4e1701-1ea6-40c9-9dcc-13d34ffead57");

Channel<byte[]> packets = Channel.CreateUnbounded<byte[]>(); // size doesn't really matter it will be reset after data is parsed.
var progressor = new ProgressorService(packets);

using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
ulong address = await progressor.Discover(progressorUuid, cts.Token);

await progressor.ConnectServiceAsync(address, progressorUuid);

Guid DataServiceUuid = Guid.Parse("7e4e1702-1ea6-40c9-9dcc-13d34ffead57");
Guid ControlPointServiceUuid = Guid.Parse("7e4e1703-1ea6-40c9-9dcc-13d34ffead57");

GattCharacteristic dataCharacteristic = await progressor.GetCharacteristicAsync(DataServiceUuid);
GattCharacteristic controlCharacteristic = await progressor.GetCharacteristicAsync(ControlPointServiceUuid);

dataCharacteristic.ValueChanged += progressor.Characteristic_ValueChanged;
await progressor.EnableNotificationAsync(dataCharacteristic);
await progressor.WriteToControlPoint(controlCharacteristic, ProgressorCommands.CMD_GET_APP_VERSION);

await foreach (var packet in packets.Reader.ReadAllAsync())
{
    var data = progressor.ParseToResponse(packet);
    switch (data.Tag)
    {
        case 0x00: 
            progressor.HandleSampleBatteryVoltage(data);
            break;
        case 0x01: 
            progressor.HandleWeightSamples(data); 
            break;
        case 0x04: 
            progressor.HandleLowBattery(data);
            break;
        default:       
            break;
    }
}

//while (true)
//{
//    Console.WriteLine("------------- Enter a Command -------------");
//    Console.WriteLine("1.Start weight recording");
//    Console.WriteLine("2.Get app version");
//    Console.WriteLine("3.Turn off Tindep");
//}
