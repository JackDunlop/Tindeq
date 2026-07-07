using System.Buffers.Binary;
using System.Threading.Channels;
using Tindeq.Progressor.Data;
using Tindeq.Progressor.Enum;
using Tindeq.Progressor.Service.Interfaces;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace Tindeq.Progressor.Service // https://learn.microsoft.com/en-us/windows/apps/develop/devices-sensors/gatt-client
{
    public class ProgressorService : IProgressorService
    {
        private BluetoothLEAdvertisementWatcher _watcher;
        private BluetoothLEDevice _device;
        private GattDeviceService _service;

        private GattCharacteristic _dataCharacteristic;
        private GattCharacteristic _controlCharacteristic;

        private Guid DataCharacteristicUuid = Guid.Parse("7e4e1702-1ea6-40c9-9dcc-13d34ffead57");
        private Guid ControlPointCharacteristicUuid = Guid.Parse("7e4e1703-1ea6-40c9-9dcc-13d34ffead57");

        private Channel<byte[]> _packets { get; set; }

        public ProgressorService(Channel<byte[]> packets)
        {
            _packets = packets;
        }


        /// <summary>
        /// Discovers a Bluetooth LE device with the specified service UUID.
        /// </summary>
        /// <param name="serviceUuid"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<ulong> Discover(Guid serviceUuid, CancellationToken cancellationToken = default)
        {
            var found = new TaskCompletionSource<ulong>();

            var filter = new BluetoothLEAdvertisementFilter();
            filter.Advertisement.ServiceUuids.Add(serviceUuid);

            _watcher = new BluetoothLEAdvertisementWatcher(filter) { ScanningMode = BluetoothLEScanningMode.Active };

            _watcher.Received += (sender, args) =>
            {
                Console.WriteLine($"Match at {args.BluetoothAddress:x}");
                sender.Stop();
                found.TrySetResult(args.BluetoothAddress);
            };

            cancellationToken.Register(() =>
            {
                _watcher.Stop();
                found.TrySetCanceled(cancellationToken);
            });

            _watcher.Start();
            return found.Task;
        }

        /// <summary>
        /// This method connects to the device and retrieves the GATT service.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="serviceUuid"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task ConnectServiceAsync(ulong address, Guid serviceUuid)
        {
            _device = await BluetoothLEDevice.FromBluetoothAddressAsync(address);
            if (_device is null)
                throw new InvalidOperationException("Could not resolve device from address.");

            var result = await _device.GetGattServicesForUuidAsync(serviceUuid);
            if (result.Status != GattCommunicationStatus.Success)
                throw new InvalidOperationException($"Service query failed: {result.Status}");

            _service = result.Services.FirstOrDefault();
            if (_service is null)
                throw new InvalidOperationException("Progressor service not found on device.");

            Console.WriteLine($"Service UUID is {_service.Uuid.ToString()}");
        }

        /// <summary>
        /// This method retrieves the characteristics for the service.
        /// </summary>
        /// <returns></returns>
        public async Task GetCharacteristicsAsync()
        {
            _dataCharacteristic = await GetCharacteristicAsync(DataCharacteristicUuid);
            _controlCharacteristic = await GetCharacteristicAsync(ControlPointCharacteristicUuid);
        }

        /// <summary>
        /// This method retrieves a specific characteristic from a provide characteristic UUID
        /// </summary>
        /// <param name="characteristicUuid"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<GattCharacteristic> GetCharacteristicAsync(Guid characteristicUuid)
        {
            if (_service is null)
                throw new InvalidOperationException("Service not connected.");

            var result = await _service.GetCharacteristicsForUuidAsync(characteristicUuid);
            if (result.Status != GattCommunicationStatus.Success)
                throw new InvalidOperationException($"Characteristic query failed: {result.Status}");

            var characteristic = result.Characteristics.FirstOrDefault();
            if (characteristic is null)
                throw new InvalidOperationException($"Characteristic {characteristicUuid} not found on service.");

            return characteristic;
        }

        /// <summary>
        /// THis method enables notifications for a any GattCharacteristic.
        /// </summary>
        /// <param name="gattCharacteristic"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task EnableNotificationAsync(GattCharacteristic gattCharacteristic)
        {
            var result = await gattCharacteristic.WriteClientCharacteristicConfigurationDescriptorWithResultAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);

            if (result.Status != GattCommunicationStatus.Success)
                throw new InvalidOperationException($"Could not enable notifications: {result.Status}");
        }

        /// <summary>
        /// This method enables notifications for _dataCharacteristic.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task EnableNotificationForDataAsync()
        {
            if (_dataCharacteristic is null)
                throw new InvalidOperationException("Characteristics not retrieved yet.");

            var result = await _dataCharacteristic.WriteClientCharacteristicConfigurationDescriptorWithResultAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);

            if (result.Status != GattCommunicationStatus.Success)
                throw new InvalidOperationException($"Could not enable notifications: {result.Status}");
        }

        /// <summary>
        /// This writes commands to _controlCharacteristic
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task WriteToControlPoint(ProgressorCommands command)
        {
            if (_controlCharacteristic == null)
                throw new InvalidOperationException("_controlCharacteristic is null");

            GattCharacteristicProperties properties = _controlCharacteristic.CharacteristicProperties;
            if (!properties.HasFlag(GattCharacteristicProperties.Write))
                throw new InvalidOperationException("This characterisitc does not support writting");

            var writer = new DataWriter();

            writer.WriteByte((byte)command);
            writer.WriteByte(0x00);

            GattCommunicationStatus result = await _controlCharacteristic.WriteValueAsync(writer.DetachBuffer());
            if (result != GattCommunicationStatus.Success)
                throw new InvalidOperationException($"Failed to write to {_controlCharacteristic.Uuid}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gattCharacteristic"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task WriteToControlPoint(GattCharacteristic gattCharacteristic, ProgressorCommands command)
        {
            if (gattCharacteristic == null)
                throw new InvalidOperationException("Provided characterisitc is null");

            GattCharacteristicProperties properties = gattCharacteristic.CharacteristicProperties;
            if (!properties.HasFlag(GattCharacteristicProperties.Write))
                throw new InvalidOperationException("This characterisitc does not support writting");

            var writer = new DataWriter();

            writer.WriteByte((byte)command);
            writer.WriteByte(0x00);

            GattCommunicationStatus result = await gattCharacteristic.WriteValueAsync(writer.DetachBuffer());
            if (result != GattCommunicationStatus.Success)
                throw new InvalidOperationException($"Failed to write to {gattCharacteristic.Uuid}");
        }

        public void Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            var reader = DataReader.FromBuffer(args.CharacteristicValue);
            reader.ByteOrder = ByteOrder.LittleEndian;
            byte[] bytes = new byte[reader.UnconsumedBufferLength];
            reader.ReadBytes(bytes);
            _packets.Writer.TryWrite(bytes);
        }

        // TLV
        public DataResponse ParseToResponse(byte[] response)
        {
            var tag = response[0];
            var length = response[1];
            var value = response[2..(2 + length)];

            return new DataResponse(tag, length, value);
        }

        public uint HandleSampleBatteryVoltage(DataResponse data)
        {
            if (data.Tag != 0x00) // mmm
                throw new InvalidOperationException("You have used the wrong method to read data");

            if (data.Value.Length != 4)
                throw new InvalidDataException($"Battery response should be 4 bytes, got {data.Value.Length}.");

            return BinaryPrimitives.ReadUInt32LittleEndian(data.Value.Span);
        }

        public void HandleWeightSamples(DataResponse data)
        {
            if (data.Tag != 0x01)
                throw new InvalidOperationException("You have used the wrong method to read data");

        }

        public void HandleLowBattery(DataResponse data)
        {
            if (data.Tag != 0x04)
                throw new InvalidOperationException("You have used the wrong method to read data");

        }
    }
}
