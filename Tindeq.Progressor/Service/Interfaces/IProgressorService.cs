using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace Tindeq.Progressor.Service.Interfaces
{
    internal interface IProgressorService
    {
        Task<ulong> Discover(Guid serviceUuid, CancellationToken cancellationToken);
        Task ConnectServiceAsync(ulong address, Guid serviceUuid);
        Task GetCharacteristicsAsync();
        Task EnableNotificationAsync(GattCharacteristic gattCharacteristic);
        Task EnableNotificationForDataAsync();
    }
}
