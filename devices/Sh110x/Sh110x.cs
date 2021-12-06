// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.DisplayDeviceShared;
using Iot.Device.DisplayDeviceShared.Commands;
using Iot.Device.Sh110x.Commands;
using Iot.Device.Sh110x.Commands.Sh110xCommands;

namespace Iot.Device.Ssd13xx
{
    /// <summary>
    /// A single-chip CMOS OLED/PLED driver with controller for organic/polymer
    /// light emitting diode dot-matrix graphic display system.
    /// </summary>
    public class Sh110x : DisplayDevice, IDisposable
    {
        /// <summary>
        /// Default I2C bus address.
        /// </summary>
        public const byte DefaultI2cAddress = 0x3C;

        /// <summary>
        /// Initializes new instance of <see cref="Sh110x"/> device that will communicate using I2C bus.
        /// A single-chip CMOS OLED/PLED driver with controller for organic/polymer
        /// light emitting diode dot-matrix graphic display system.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        /// <param name="resolution">Display resolution</param>
        public Sh110x(I2cDevice i2cDevice, DisplayResolution resolution = DisplayResolution.OLED64x128) : base(i2cDevice, resolution)
        {
            Pages = (byte)((Height +7) / 8);

            // setup buffers
            _genericBuffer = new byte[Width * (Height + 7) / 8];

            _pageData = new byte[Width + 2];

            switch (resolution)
            {
                case DisplayResolution.OLED64x128:
                    _i2cDevice.Write(_oled64x128Init);

                    Thread.Sleep(100);

                    SendCommand(new SetDisplayOn());

                    // this display is rotated
                    //Rotation = 3;
                    break;

                // all other resolutions aren't supported at this time
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null!;
        }

        /// <summary>
        /// Sends command to the device
        /// </summary>
        /// <param name="command">Command being send</param>
        public void SendCommand(ISh110xCommand command) => SendCommand((ICommand)command);

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

        /// <summary>
        /// Displays the information on the screen using page mode.
        /// </summary>
        public override void Display()
        {
            int bytes_per_page = _initialWidth;
            bool hasData = _dirtyWindowY1 > -1 && _dirtyWindowY2 > -1;
            
            int page_index = _dirtyWindowY1 / 8;
            int last_page = (_dirtyWindowY2 + 7) / 8;
            int page_start = Math.Min(bytes_per_page, _dirtyWindowX1);
            int page_end = Math.Max(0, _dirtyWindowX2);

            while (hasData || page_index < last_page)
            {
                int bytes_remaining = bytes_per_page;
                int bufferIndex = page_index * bytes_per_page;

                // fast forward to dirty rectangle start page
                bufferIndex += page_start;

                bytes_remaining -= page_start;
                
                // cut off at end of dirty rectangle
                bytes_remaining -= (_initialWidth - 1) - page_end;

                // set page number
                SendCommand(new SetAddress((PageAddress)page_index, (sbyte)page_start));

                SendData(new SpanByte(_genericBuffer, bufferIndex, bytes_remaining));

                if (page_index == last_page)
                {
                    break;
                }

                hasData = false;
                page_index++;
            }

            // reset dirty window
            _dirtyWindowX1 = 1024;
            _dirtyWindowY1 = 1024;
            _dirtyWindowX2 = -1;
            _dirtyWindowY2 = -1;
        }

        /// <summary>
        /// Sequence of bytes that should be sent to a 64x128 OLED display to setup the device.
        /// First byte is the command byte 0x00.
        /// </summary>
        private readonly byte[] _oled64x128Init =
        {
            0x00,       // is command
            0xae,       // turn display off
            0xd5, 0x51, // set display clock divide ratio/oscillator,  set ratio = 0x80
            0x20,       // 0x20 set memory address mode,  page addressing
            0x81, 0x4f, // set contrast control 
            0xad, 0x8a, // set charge pump,  
            0xa0,       // set segment re-map
            0xc0,       // set com output scan direction
            0xdc, 0x00, // set display start line 0x40-0x7F
            0xd3, 0x60, // set display offset 0x00-0x3f, no offset = 0x00
            0xd9, 0x22, // set pre-charge period 
            0xdb, 0x35, // set VCOMH deselect level
            0xa8, 0x3f, // set multiplex ratio 0x00-0x3f        
            0xa4,       // set display ON
            0xa6,       // set normal display
        };
    }
}
