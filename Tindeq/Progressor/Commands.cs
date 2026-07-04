using System;
using System.Collections.Generic;
using System.Text;

namespace Tindeq.Progressor
{
    public enum Commands
    {
        CMD_TARE_SCALE = 100,
        CMD_START_WEIGHT_MEAS = 101,
        CMD_STOP_WEIGHT_MEAS = 102,
        CMD_START_PEAK_RFD_MEAS = 103,
        CMD_START_PEAK_RFD_MEAS_SERIES = 104,
        CMD_ADD_CALIBRATION_POINT = 105,
        CMD_SAVE_CALIBRATION = 106,
        CMD_GET_APP_VERSION = 107,
        CMD_GET_ERROR_INFORMATION = 108,
        CMD_CLR_ERROR_INFORMATION = 109,
        CMD_ENTER_SLEEP = 110,
        CMD_GET_BATTERY_VOLTAGE = 111,
    }
}
