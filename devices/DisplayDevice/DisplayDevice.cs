// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.DisplayDeviceShared.Commands;
using System;
using System.Device.I2c;

namespace Iot.Device.DisplayDeviceShared
{
    /// <summary>
    /// Base class for Displays.
    /// </summary>
    public abstract class DisplayDevice : IDisplayDevice
    {
        /// <summary>
        /// Page mode output command bytes.
        /// </summary>
        protected byte[] _pageCmd = new byte[]
        {
            0x00, // is command
            0xB0, // page address (B0-B7)
            0x00, // lower columns address =0
            0x10, // upper columns address =0
        };

        internal DisplayDevice(
            I2cDevice i2cDevice,
            DisplayResolution resolution = DisplayResolution.OLED128x64)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));

            switch (resolution)
            {
                case DisplayResolution.OLED128x64:
                    Width = 128;
                    Height = 64;
                    break;

                case DisplayResolution.OLED64x128:
                    Width = 64;
                    Height = 128;
                    break;

                case DisplayResolution.OLED128x32:
                    Width = 128;
                    Height = 32;
                    break;

                case DisplayResolution.OLED96x16:
                    Width = 96;
                    Height = 16;
                    break;
            }

            // reset dirty window to complete screen
            _dirtyWindowX2 = Width - 1;
            _dirtyWindowY2 = Height - 1;
        }

        /// <summary>
        /// Screen Resolution Width in Pixels
        /// </summary>
        public int Width
        {
            get
            {
                return _width;
            }
            protected set
            {
                _width = value;

                if (!_initialWidthSet)
                {
                    // set initial value, only ONCE
                    _initialWidth = value;
                }
            }
        }

        /// <summary>
        /// Screen Resolution Height in Pixels
        /// </summary>
        public int Height
        {
            get
            {
                return _height;
            }
            protected set
            {
                _height = value;

                if (!_initialHeightSet)
                {
                    // set initial value, only ONCE
                    _initialHeight = value;
                }
            }
        }

        /// <summary>
        /// Screen Rotation in in steps of 90degrees.
        /// </summary>
        /// <remarks>
        /// Possible values are 0, 90, 180 or 270 degrees.
        /// </remarks>
        public int Rotation
        {
            get
            {
                return _rotation;
            }
            set
            {
                _rotation = value;

                switch (_rotation)
                {
                    case 0:
                    case 2:
                        _width = _initialWidth;
                        _height = _initialHeight;
                        break;
                    case 1:
                    case 3:
                        _width = _initialHeight;
                        _height = _initialWidth;
                        break;
                }
            }
        }

        /// <summary>
        /// Screen data pages.
        /// </summary>
        public byte Pages { get; set; }

        /// <summary>
        /// Font to use.
        /// </summary>
        public IFont Font { get; set; }

        /// <summary>
        /// Verifies value is within a specific range.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <param name="start">Starting value of range.</param>
        /// <param name="end">Ending value of range.</param>
        /// <returns>Determines if value is within range.</returns>
        internal static bool InRange(uint value, uint start, uint end)
        {
            return (value - start) <= (end - start);
        }

        /// <summary>
        /// Clears the screen.
        /// </summary>
        public void ClearScreen()
        {
            Array.Clear(_genericBuffer, 0, _genericBuffer.Length);

            // reset dirty window to complete screen
            // use the initial display size
            _dirtyWindowX1 = 0;
            _dirtyWindowY1 = 0;
            _dirtyWindowX2 = _initialWidth - 1;
            _dirtyWindowY2 = _initialHeight - 1;

            Display();
        }

        /// <summary>
        /// Displays the information on the screen using page mode.
        /// </summary>
        public abstract void Display();

        /// <summary>
        /// Send a command to the display controller.
        /// </summary>
        /// <param name="command">The command to send to the display controller.</param>
        public abstract void SendCommand(ICommand command);

        /// <summary>
        /// Displays the  1 bit bit map.
        /// </summary>
        /// <param name="x">The x coordinate on the screen.</param>
        /// <param name="y">The y coordinate on the screen.</param>
        /// <param name="width">Width in bytes.</param>
        /// <param name="height">Height in bytes.</param>
        /// <param name="bitmap">Bitmap to display.</param>
        /// <param name="size">Drawing size, normal = 1, larger use 2,3 etc.</param>
        public void DrawBitmap(int x, int y, int width, int height, byte[] bitmap, byte size = 1)
        {
            if ((width * height) != bitmap.Length)
            {
                throw new ArgumentException("Width and height do not match the bitmap size.");
            }

            byte mask = 0x01;
            byte b;

            for (var yO = 0; yO < height; yO++)
            {
                for (var xA = 0; xA < width; xA++)
                {
                    b = bitmap[(yO * width) + xA];

                    for (var pixel = 0; pixel < 8; pixel++)
                    {
                        if (size == 1)
                        {
                            DrawPixel(x + (8 * xA) + pixel, y + yO, (b & mask) > 0);
                        }
                        else
                        {
                            DrawFilledRectangle((x + (8 * xA) + pixel) * size, (y / size + yO) * size, size, size, (b & mask) > 0);
                        }
                        mask <<= 1;
                    }

                    mask = 0x01;//reset each time to support SSH1106 OLEDs
                }
            }
        }

        /// <summary>
        /// Draws a rectangle that is solid/filled.
        /// </summary>
        /// <param name="x0">x coordinate starting of the top left.</param>
        /// <param name="y0">y coordinate starting of the top left.</param>
        /// <param name="width">Width of rectabgle in pixels.</param>
        /// <param name="height">Height of rectangle in pixels</param>
        /// <param name="inverted">Turn the pixel on (true) or off (false).</param>
        public void DrawFilledRectangle(int x0, int y0, int width, int height, bool inverted = true)
        {
            for (int i = 0; i <= height; i++)
            {
                DrawHorizontalLine(x0, y0 + i, width, inverted);
            }
        }

        /// <summary>
        /// Draws a horizontal line.
        /// </summary>
        /// <param name="x0">x coordinate starting of the line.</param>
        /// <param name="y0">y coordinate starting of line.</param>
        /// <param name="length">Line length.</param>
        /// <param name="inverted">Turn the pixel on (true) or off (false).</param>
        public void DrawHorizontalLine(int x0, int y0, int length, bool inverted = true)
        {
            for (var x = x0; (x - x0) < length; x++)
            {
                DrawPixel(x, y0, inverted);
            }
        }

        /// <summary>
        /// Draws a pixel on the screen.
        /// </summary>
        /// <param name="x">The x coordinate on the screen.</param>
        /// <param name="y">The y coordinate on the screen.</param>
        /// <param name="inverted">Indicates if color to be used turn the pixel on, or leave off.</param>
        public void DrawPixel(int x, int y, bool inverted = true)
        {
            if ((x >= Width) || (y >= Height))
            {
                return;
            }

            int t = x;

            // rotate coordinates, if needed
            switch (Rotation)
            {
                case 1:
                    // swap coords
                    x = y;
                    y = t;
                    x = Width - x - 1;
                    break;
                case 2:
                    x = Width - x - 1;
                    y = Height - y - 1;
                    break;
                case 3:
                    // swap coords
                    x = y;
                    y = t;
                    y = Height - y - 1;
                    break;
            }

            // adjust dirty window
            _dirtyWindowX1 = Math.Min(_dirtyWindowX1, x);
            _dirtyWindowY1 = Math.Min(_dirtyWindowY1, y);
            _dirtyWindowX2 = Math.Max(_dirtyWindowX2, x);
            _dirtyWindowY2 = Math.Max(_dirtyWindowY2, y);

            // x specifies the column
            int idx = x + (y / 8) * Width;

            if (inverted)
            {
                _genericBuffer[idx] |= (byte)(1 << (y & 7));
            }
            else
            {
                _genericBuffer[idx] &= (byte)~(1 << (y & 7));
            }
        }

        /// <summary>
        /// Writes a text message on the screen with font in use.
        /// </summary>
        /// <param name="x">The x pixel-coordinate on the screen.</param>
        /// <param name="y">The y pixel-coordinate on the screen.</param>
        /// <param name="str">Text string to display.</param>
        /// <param name="size">Text size, normal = 1, larger use 2,3, 4 etc.</param>
		/// <param name="center">Indicates if text should be centered if possible.</param>
        /// <seealso cref="Write"/>
        public void DrawString(int x, int y, string str, byte size = 1, bool center = false)
        {
            if (center && str != null)
            {
                int padSize = (Width / size / Font.Width - str.Length) / 2;
                if (padSize > 0)
                    str = str.PadLeft(str.Length + padSize);
            }

            byte[] bitMap = this.GetTextBytes(str);

            this.DrawBitmap(x, y, bitMap.Length / Font.Height, Font.Height, bitMap, size);
        }

        /// <summary>
        /// Draws a vertical line.
        /// </summary>
        /// <param name="x0">x coordinate starting of the line.</param>
        /// <param name="y0">y coordinate starting of line.</param>
        /// <param name="length">Line length.</param>
        /// <param name="inverted">Turn the pixel on (true) or off (false).</param>
        public void DrawVerticalLine(int x0, int y0, int length, bool inverted = true)
        {
            for (var y = y0; (y - y0) < length; y++)
            {
                DrawPixel(x0, y, inverted);
            }
        }

        /// <summary>
        /// Send a command to the display controller.
        /// </summary>
        /// <param name="command">The command to send to the display controller.</param>
        public abstract void SendCommand(ISharedCommand command);

        /// <summary>
        /// Send data to the display controller.
        /// </summary>
        /// <param name="data">The data to send to the display controller.</param>
        public virtual void SendData(SpanByte data)
        {
            if (data.IsEmpty)
            {
                throw new ArgumentNullException(nameof(data));
            }

            byte[] buffer = new byte[data.Length + 1];
            Array.Copy(data.ToArray(), 0, buffer, 1, data.Length);

            //            SpanByte writeBuffer = new SpanByte( SliceGenericBuffer(data.Length + 1);
            SpanByte writeBuffer = new SpanByte(buffer);

            writeBuffer[0] = 0x40; // Control byte.
            //data.CopyTo(writeBuffer.Slice(1));
            _i2cDevice.Write(writeBuffer);
        }

        /// <summary>
        /// Writes a text message on the screen with font in use.
        /// </summary>
        /// <param name="x">The x text-coordinate on the screen.</param>
        /// <param name="y">The y text-coordinate on the screen.</param>
        /// <param name="str">Text string to display.</param>
        /// <param name="size">Text size, normal = 1, larger use 2,3, 4 etc.</param>
		/// <param name="center">Indicates if text should be centered if possible.</param>
        /// <seealso cref="DrawString"/>
        public void Write(int x, int y, string str, byte size = 1, bool center = false)
        {
            DrawString(x * Font.Width, y * Font.Height, str, size, center);
        }

        /// <summary>
        /// Get the bytes to be drawn on the screen for text, from the font
        /// </summary>
        /// <param name="text">Strint to be shown on the screen.</param>
        /// <returns>The bytes to be drawn using current font.</returns>
        byte[] GetTextBytes(string text)
        {
            byte[] bitMap;

            if (Font.Width == 8)
            {
                bitMap = new byte[text.Length * Font.Height * Font.Width / 8];

                for (int i = 0; i < text.Length; i++)
                {
                    var characterMap = Font[text[i]];

                    //if(Rotation == 1)
                    //{
                    //    var rotatedCharacterMap = new byte[characterMap.Length];

                    //    for (int segment = 0; segment < Font.Height; segment++)
                    //    {
                    //        rotatedCharacterMap[segment] |= 
                    //        bitMap[i + (segment * text.Length)] = characterMap[segment];
                    //    }
                    //}

                    for (int segment = 0; segment < Font.Height; segment++)
                    {
                        bitMap[i + (segment * text.Length)] = characterMap[segment];
                    }
                }
            }
            else
            {
                throw new Exception("Font width must be 8");
            }

            return bitMap;
        }
    }
}