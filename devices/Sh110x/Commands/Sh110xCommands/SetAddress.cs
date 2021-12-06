// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Sh110x.Commands.Sh110xCommands
{
    /// <summary>
    /// Page address
    /// </summary>
    public enum PageAddress : byte
    {
        /// <summary>Page0</summary>
        Page0 = 0,

        /// <summary>Page1</summary>
        Page1,

        /// <summary>Page2</summary>
        Page2,

        /// <summary>Page3</summary>
        Page3,

        /// <summary>Page4</summary>
        Page4,

        /// <summary>Page5</summary>
        Page5,

        /// <summary>Page6</summary>
        Page6,

        /// <summary>Page6</summary>
        Page7,

        /// <summary>Page8</summary>
        Page8,

        /// <summary>Page9</summary>
        Page9,

        /// <summary>Page10</summary>
        Page10,

        /// <summary>Page11</summary>
        Page11,

        /// <summary>Page12</summary>
        Page12,

        /// <summary>Page13</summary>
        Page13,

        /// <summary>Page14</summary>
        Page14,

        /// <summary>Page15</summary>
        Page15,
    }

    /// <summary>
    /// Represents SetPageAddress command
    /// </summary>
    public class SetAddress : ISh110xCommand
    {
        /// <summary>
        /// This command specifies the start page and column address to load display RAM data to page address register.
        /// Any RAM data bit can be accessed when its page address and column address are specified.The display remains unchanged even when the page address is changed.
        /// </summary>
        /// <param name="pageStartAddress">Page start address with a range of 0-15.</param>
        /// <param name="columnStartAddress">Column start address with a range of 0-127.</param>
        public SetAddress(PageAddress pageStartAddress, sbyte columnStartAddress = 0)
        {
            if (columnStartAddress > 0x7F)
            {
                throw new ArgumentOutOfRangeException(nameof(pageStartAddress));
            }

            PageStartAddress = pageStartAddress;
            ColumnStartAddress = columnStartAddress;
        }

        /// <summary>
        /// The value that represents the command.
        /// </summary>
        public byte Id => 0xB0;

        /// <summary>
        /// Page start address with a range of 0-15.
        /// </summary>
        public PageAddress PageStartAddress { get; set; }
        
        /// <summary>
        /// Column start address with a range of 0-127.
        /// </summary>
        public sbyte ColumnStartAddress { get; set; }

        /// <summary>
        /// Gets the byte that represents the command.
        /// </summary>
        /// <returns>The bytes that represent the command.</returns>
        public byte[] GetBytes()
        {
            // compose command sequence
            var buffer = new byte[3];

            buffer[0] = (byte)(Id + PageStartAddress);
            buffer[1] = (byte)(0x10 + ((ColumnStartAddress) >> 4));
            buffer[2] = (byte)(ColumnStartAddress & 0xF);

            return buffer;
        }
    }
}
