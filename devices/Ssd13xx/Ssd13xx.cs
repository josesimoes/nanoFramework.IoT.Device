// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using Iot.Device.DisplayDeviceShared;
using Iot.Device.Ssd13xx.Commands;
using Iot.Device.Ssd13xx.Commands.Ssd1306Commands;

namespace Iot.Device.Ssd13xx
{
    /// <summary>
    /// Represents base class for SSD13xx OLED displays
    /// </summary>
    public abstract class Ssd13xx : DisplayDevice, IDisposable
    {
        /// <summary>
        /// Constructs instance of Ssd13xx
        /// </summary>
        /// <param name="i2cDevice">I2C device used to communicate with the device</param>
        /// <param name="resolution">Screen resolution to use for device init.</param>
        public Ssd13xx(
            I2cDevice i2cDevice,
            DisplayResolution resolution = DisplayResolution.OLED128x64) : base(
                i2cDevice,
                resolution)
        {
            switch (resolution)
            {
                case DisplayResolution.OLED128x64:
                    _i2cDevice.Write(_oled128x64Init);
                    break;

                case DisplayResolution.OLED128x32:
                    _i2cDevice.Write(_oled128x32Init);
                    break;

                case DisplayResolution.OLED96x16:
                    _i2cDevice.Write(_oled96x16Init);
                    break;
            }

            Pages = (byte)(Height / 8);

            //adding 4 bytes make it SSH1106 IC OLED compatible
            _genericBuffer = new byte[Pages * Width + 4];

            _pageData = new byte[Width + 1];
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null!;
        }

        /// <summary>
        /// Displays the information on the screen using page mode.
        /// </summary>
        public override void Display()
        {
            for (byte i = 0; i < Pages; i++)
            {
                _pageCmd[1] = (byte)(PageAddress.Page0 + i); // page number
                _i2cDevice.Write(_pageCmd);

                _pageData[0] = 0x40; // is data
                Array.Copy(_genericBuffer, i * Height, _pageData, 1, Height);
                _i2cDevice.Write(_pageData);
            }
        }

        /// <summary>
        /// Sequence of bytes that should be sent to a 128x64 OLED display to setup the device.
        /// First byte is the command byte 0x00.
        /// </summary>
        private readonly byte[] _oled128x64Init =
        {
            0x00,       // is command
            0xae,       // turn display off
            0xd5, 0x80, // set display clock divide ratio/oscillator,  set ratio = 0x80
            0xa8, 0x3f, // set multiplex ratio 0x00-0x3f        
            0xd3, 0x00, // set display offset 0x00-0x3f, no offset = 0x00
            0x40 | 0x0, // set display start line 0x40-0x7F
            0x8d, 0x14, // set charge pump,  enable  = 0x14  disable = 0x10
            0x20, 0x00, // 0x20 set memory address mode,  0x0 = horizontal addressing mode
            0xa0 | 0x1, // set segment re-map
            0xc8,       // set com output scan direction
            0xda, 0x12, // set COM pins HW configuration
            0x81, 0xcf, // set contrast control for BANK0, extVcc = 0x9F,  internal = 0xcf
            0xd9, 0xf1, // set pre-charge period  to 0xf1,  if extVcc then set to 0x22
            0xdb,       // set VCOMH deselect level
            0x40,       // set display start line
            0xa4,       // set display ON
            0xa6,       // set normal display
            0xaf        // turn display on 0xaf
        };

        /// <summary>
		/// Sequence of bytes that should be sent to a 128x32 OLED display to setup the device.
		/// First byte is the command byte 0x00.
		/// </summary>
		private readonly byte[] _oled128x32Init =
        {
            0x00,       // is command
            0xae,       // turn display off
            0xd5, 0x80, // set display clock divide ratio/oscillator,  set ratio = 0x80
            0xa8, 0x1f, // set multiplex ratio 0x00-0x1f        
            0xd3, 0x00, // set display offset 0x00-0x3f, no offset = 0x00
            0x40 | 0x0, // set display start line 0x40-0x7F
            0x8d, 0x14, // set charge pump,  enable  = 0x14  disable = 0x10
            0x20, 0x00, // 0x20 set memory address mode,  0x0 = horizontal addressing mode
            0xa0 | 0x1, // set segment re-map
            0xc8,       // set com output scan direction
            0xda, 0x02, // set COM pins HW configuration
            0x81, 0x8f, // set contrast control for BANK0, extVcc = 0x9F,  internal = 0xcf
            0xd9, 0xf1, // set pre-charge period  to 0xf1,  if extVcc then set to 0x22
            0xdb,       // set VCOMH deselect level
            0x40,       // set display start line
            0xa4,       // set display ON
            0xa6,       // set normal display
            0xaf        // turn display on 0xaf
        };

        /// <summary>
		/// Sequence of bytes that should be sent to a 96x16 OLED display to setup the device.
		///	First byte is the command byte 0x00.
		/// </summary>
		private readonly byte[] _oled96x16Init =
        {
            0x00,       // is command
            0xae,       // turn display off
            0xd5, 0x80, // set display clock divide ratio/oscillator,  set ratio = 0x80
            0xa8, 0x1f, // set multiplex ratio 0x00-0x1f        
            0xd3, 0x00, // set display offset 0x00-0x3f, no offset = 0x00
            0x40 | 0x0, // set display start line 0x40-0x7F
            0x8d, 0x14, // set charge pump,  enable  = 0x14  disable = 0x10
            0x20, 0x00, // 0x20 set memory address mode,  0x0 = horizontal addressing mode
            0xa0 | 0x1, // set segment re-map
            0xc8,       // set com output scan direction
            0xda, 0x02, // set COM pins HW configuration
            0x81, 0xaf, // set contrast control for BANK0, extVcc = 0x9F,  internal = 0xcf
            0xd9, 0xf1, // set pre-charge period  to 0xf1,  if extVcc then set to 0x22
            0xdb,       // set VCOMH deselect level
            0x40,       // set display start line
            0xa4,       // set display ON
            0xa6,       // set normal display
            0xaf        // turn display on 0xaf
        };
    }
}