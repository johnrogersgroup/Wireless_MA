using System;

namespace UHMS.Core.Models.Data
{
    /// <summary>
    /// Type of data that can be acquired from the hardware sensors. 
    /// </summary>
    /// <remarks>
    /// <list type="table">
    /// <listheader>
    /// <term>Type</term>
    /// <term>Description</term>
    /// <term>Source</term>
    /// </listheader>
    /// <item>
    /// <term>ecg</term>
    /// <term>electrocardiograph</term>
    /// <term>Chest</term>
    /// </item>
    /// <item>
    /// <term>scg</term>
    /// <term>seismocardiograph</term>
    /// <term>Chest</term>
    /// </item>
    /// <item>
    /// <term>ppg</term>
    /// <term>photoplethysmograph</term>
    /// <term>Foot</term>
    /// </item>
    /// <item>
    /// <term>red</term>
    /// <term>ppg red</term>
    /// <term>Foot</term>
    /// </item>
    /// <item>
    /// <term>ir</term>
    /// <term>ppg infrared</term>
    /// <term>Foot</term>
    /// </item>
    /// <item>
    /// <term>foot_temp</term>
    /// <term>foot temperature</term>
    /// <term>Foot</term>
    /// </item>
    /// <item>
    /// <term>chest_temp</term>
    /// <term>chest temperature</term>
    /// <term>Foot</term>
    /// </item>
    /// <item>
    /// <term>accl</term>
    /// <term>accelerometer</term>
    /// <term>Chest</term>
    /// </item>
    /// </list>
    /// </remarks>
    public enum DataType
    {
        timestamp,
        ecg,        // electrocardiograph
        scg,        // seismocardiograph
        ppg,        // photoplethysmograph
        red,        // ppg red
        temp,       // temperature
        foot_temp,  // foot temperature
        chest_temp, // chest temperature
        ir,         // infrared
        accl,       // accelerometer
        accl_x,
        accl_y,
        accl_z,
        gyro,
        gyro_x,
        gyro_y,
        gyro_z
    }
}
