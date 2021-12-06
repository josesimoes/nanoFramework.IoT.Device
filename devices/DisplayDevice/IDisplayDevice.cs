// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.I2c;

namespace Iot.Device.DisplayDeviceShared
{
    /// <summary>
    /// Interface for DisplayDevice
    /// </summary>
    public class IDisplayDevice
    {
        /// <summary>
        /// Underlying I2C device
        /// </summary>
        internal I2cDevice _i2cDevice;

        internal byte[] _genericBuffer;
        internal byte[] _pageData;

        internal bool _initialWidthSet;
        internal bool _initialHeightSet;

        internal int _initialWidth;
        internal int _initialHeight;

        // backing fields for the display size
        internal int _width;
        internal int _height;

        // rotation parameter
        internal int _rotation = 0;

        // fields with dirty screen
        internal int _dirtyWindowX1 = 0;
        internal int _dirtyWindowX2 = 0;
        internal int _dirtyWindowY1;
        internal int _dirtyWindowY2;

    }
}
