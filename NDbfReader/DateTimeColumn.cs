﻿using System;
using System.Diagnostics;
using System.Text;

namespace NDbfReader
{
    /// <summary>
    /// Represents a <see cref="DateTime"/> column.
    /// </summary>
    [DebuggerDisplay("DateTime {Name}")]
    public class DateTimeColumn : Column<DateTime?>
    {
        /// <summary>
        /// Initializes a new instance with the specified name and offset.
        /// </summary>
        /// <param name="name">The column name.</param>
        /// <param name="offset">The column offset in a row in bytes.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c> or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> is &lt; 0.</exception>
        public DateTimeColumn(string name, int offset)
            : base(name, offset, size: 8)
        {
        }

        /// <summary>
        /// Loads a value from the specified buffer.
        /// </summary>
        /// <param name="buffer">The byte array from which a value should be loaded. The buffer length is always at least equal to the column size.</param>
        /// <param name="encoding">The encoding that should be used when loading a value. The encoding is never <c>null</c>.</param>
        /// <returns>A column value.</returns>
        protected override DateTime? DoLoad(byte[] buffer, Encoding encoding)
        {
            var stringValue = encoding.GetString(buffer, 0, buffer.Length);
            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return null;
            }
            return DateTime.ParseExact(stringValue, "yyyyMMdd", null);
        }
    }
}
