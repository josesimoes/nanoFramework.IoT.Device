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


//Tested with 128x64 and 128x32 OLEDs
using Ssd1306 device = new Ssd1306(I2cDevice.Create(new I2cConnectionSettings(1, Ssd1306.DefaultI2cAddress)), DisplayResolution.OLED128x64);
device.ClearScreen();
device.DrawPixel(0, 0);
device.Font = new BasicFont();
device.DrawString(2, 2, "nF IOT!", 2);//large size 2 font
//device.DrawString(2, 32, "nanoFramework", 1, true);//centered text

device.Display();

Thread.Sleep(-1);
