using System;
using System.Collections.Generic;
using System.Text;

namespace Tindeq.Progressor
{
    public enum Response
    {
        RES_CMD_RESPONSE = 0, // Responmse to sample battery voltage 0x6F
        RES_WEIGHT_MEAS = 1, // Weight measurement. Each measurement is sent together with a teimestamp where the timestamp in the number of microseconds since the measurement was started.
        RES_RFD_PEAK = 2,
        RES_RFD_PEAK_SERIES = 3,
        RES_LOW_PWR_WARNING = 4, // Low power warning indicating that the battery is empty.
    }
}
