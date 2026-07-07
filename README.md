# Tindeq Progressor .NET

A small .NET library for communicating with the [Tindeq Progressor](https://tindeq.com/product/progressor/) climbing force measurement device over Bluetooth Low Energy (BLE).

Porting the [Progressor API](https://tindeq.com/progressor_api/) Python example to C#.

## Supported Commands

| Command | Description |
|---------|-------------|
| `CMD_TARE_SCALE` | Zero the weight when no load is applied |
| `CMD_START_WEIGHT_MEAS` | Start continuous weight measurement (80 Hz sample rate) |
| `CMD_STOP_WEIGHT_MEAS` | Stop weight measurement |
| `CMD_START_PEAK_RFD_MEAS` | Start peak Rate of Force Development measurement |
| `CMD_START_PEAK_RFD_MEAS_SERIES` | Start peak RFD measurement series |
| `CMD_ADD_CALIBRATION_POINT` | Add a calibration point |
| `CMD_SAVE_CALIBRATION` | Save calibration |
| `CMD_GET_APP_VERSION` | Get firmware version |
| `CMD_GET_ERROR_INFORMATION` | Get error information |
| `CMD_CLR_ERROR_INFORMATION` | Clear error information |
| `CMD_ENTER_SLEEP` | Turn off the Progressor |
| `CMD_GET_BATTERY_VOLTAGE` | Sample battery voltage |
