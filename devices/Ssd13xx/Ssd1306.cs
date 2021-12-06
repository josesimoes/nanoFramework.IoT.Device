// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using Iot.Device.DisplayDeviceShared;
using Iot.Device.DisplayDeviceShared.Commands;
using Iot.Device.Ssd13xx.Commands;

namespace Iot.Device.Ssd13xx
{
    /// <summary>
    /// A single-chip CMOS OLED/PLED driver with controller for organic/polymer
    /// light emitting diode dot-matrix graphic display system.
    /// </summary>
    public class Ssd1306 : Ssd13xx
    {
        /// <summary>
        /// Default I2C bus address.
        /// </summary>
        public const byte DefaultI2cAddress = 0x3C;

        /// <summary>
        /// Initializes new instance of Ssd1306 device that will communicate using I2C bus.
        /// A single-chip CMOS OLED/PLED driver with controller for organic/polymer
        /// light emitting diode dot-matrix graphic display system.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        public Ssd1306(I2cDevice i2cDevice) : base(i2cDevice)
        {
        }

        /// <summary>
        /// Initializes new instance of Ssd1306 device that will communicate using I2C bus.
        /// A single-chip CMOS OLED/PLED driver with controller for organic/polymer
        /// light emitting diode dot-matrix graphic display system.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        /// <param name="res">Display resolution</param>
        public Ssd1306(I2cDevice i2cDevice, DisplayResolution res) : base(i2cDevice, res)
        {
        }

        /// <summary>
        /// Sends command to the device
        /// </summary>
        /// <param name="command">Command being send</param>
        public void SendCommand(ISsd1306Command command) => SendCommand((ICommand)command);

        /// <summary>
        /// Sends command to the device
        /// </summary>
        /// <param name="command">Command being send</param>
        public override void SendCommand(ISharedCommand command) => SendCommand((ICommand)command);

        /// <inheritdoc/>
        public override void SendCommand(ICommand command)
        {
            var commandBytes = command.GetBytes();

            if (commandBytes is not { Length: > 0 })
            {
                throw new ArgumentNullException(nameof(command), "Argument is either null or there were no bytes to send.");
            }
            byte[] buffer = new byte[commandBytes.Length + 1];
            Array.Copy(commandBytes, 0, buffer, 1, commandBytes.Length);

            // Control byte
            buffer[0] = 0x00;

            // Be aware there is a Continuation Bit in the Control byte and can be used
            // to state (logic LOW) if there is only data bytes to follow.
            // This binding separates commands and data by using SendCommand and SendData.
            _i2cDevice.Write(buffer);
        }
    }
}
