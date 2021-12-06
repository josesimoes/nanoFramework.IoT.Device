// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.DisplayDeviceShared;
using Iot.Device.Ssd13xx;
using Iot.Device.Ssd13xx.Samples;
using nanoFramework.Hardware.Esp32;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;


Debug.WriteLine("Hello Ssd1306 Sample!");

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus, for example
Configuration.SetPinFunction(8, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(9, DeviceFunction.I2C1_CLOCK);
//////////////////////////////////////////////////////////////////////


//Tested with Adafruit FeatherWing OLED - 128x64 https://www.adafruit.com/product/4650
using Sh110x device = new Sh110x(I2cDevice.Create(new I2cConnectionSettings(1, Sh110x.DefaultI2cAddress)), DisplayResolution.OLED64x128);

device.ClearScreen();
device.DrawPixel(0, 0);
device.DrawPixel(8, 0);
//device.DrawPixel(63, 127);
//device.DrawVerticalLine(0, 0, 128);
//device.DrawHorizontalLine(0, 0, 64);

//device.DrawHorizontalLine(0, 2, 32);
//device.DrawVerticalLine(2, 0, 64);

//device.DrawHorizontalLine(0, 4, 16);
//device.DrawVerticalLine(4, 0, 32);
//device.Display();

device.Font = new BasicFont();
//device.TextRotation = 1;
//device.Rotation = 1;

//device.DrawString(2, 2, "nF", 1);//large size 2 font
//device.DrawString(2, 32, "nanoFramework", 1, true);//centered text
//device.DrawString(0, 0, "A", 1);//centered text


device.Display();

Thread.Sleep(-1);
