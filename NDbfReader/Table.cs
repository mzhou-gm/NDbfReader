﻿using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace NDbfReader
{
    /// <summary>
    /// Represents a dBASE table.  Use one of the Open static methods to create a new instance.
    /// </summary>
    /// <example>
    /// <code>
    /// using(var table = Table.Open(@"D:\Example\table.dbf"))
    /// {
    ///     ...
    /// }
    /// </code>
    /// </example>
    public class Table : IParentTable, IDisposable
    {
        private readonly BinaryReader _reader;
        private readonly Header _header;

        private bool _isReaderOpened;
        private bool _disposed;

        /// <summary>
        /// Opens a table from the specified file.
        /// </summary>
        /// <param name="path">The file to be opened.</param>
        /// <returns>A table instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <c>null</c> or empty.</exception>
        /// <exception cref="NotSupportedException">The dBASE table constains one or more columns of unsupported type.</exception>
        public static Table Open(string path)
        {
            if(string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }

            return Open(File.OpenRead(path));
        }

        /// <summary>
        /// Opens a table from the specified stream.
        /// </summary>
        /// <param name="stream">The stream of dBASE table to open. The stream is closed when the returned table instance is disposed.</param>
        /// <returns>A table instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="stream"/> does not allow reading.</exception>
        /// <exception cref="NotSupportedException">The dBASE table constains one or more columns of unsupported type.</exception>
        public static Table Open(Stream stream)
        {
            return Open(stream, HeaderLoader.Default);
        }

        /// <summary>
        /// Opens a table from the specified stream with the specified header loader.
        /// </summary>
        /// <param name="stream">The stream of dBASE table to open. The stream is closed when the returned table instance is disposed.</param>
        /// <param name="headerLoader">The header loader.</param>
        /// <returns>A table instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <c>null</c> or <paramref name="headerLoader"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="stream"/> does not allow reading.</exception>
        /// <exception cref="NotSupportedException">The dBASE table constains one or more columns of unsupported type.</exception>
        public static Table Open(Stream stream, HeaderLoader headerLoader)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (!stream.CanRead)
            {
                throw ExceptionFactory.CreateArgumentException("stream", "The stream does not allow reading (CanRead property returns false).");
            }
            if(headerLoader == null)
            {
                throw new ArgumentNullException("headerLoader");
            }

            var binaryReader = new BinaryReader(stream);
            var header = headerLoader.Load(binaryReader);
            return new Table(header, binaryReader);
        }

        /// <summary>
        /// Initializes a new instance from the specified header and binary reader.
        /// </summary>
        /// <param name="header">The dBASE header.</param>
        /// <param name="reader">The binary reader positioned at the firsh byte of the first row.</param>
        /// <exception cref="ArgumentNullException"><paramref name="header"/> is <c>null</c> or <paramref name="reader"/> is <c>null</c>.</exception>
        protected Table(Header header, BinaryReader reader)
        {
            if (header == null)
            {
                throw new ArgumentNullException("header");
            }
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            _reader = reader;
            _header = header;
        }

        /// <summary>
        /// Gets a list of all columns in the table.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The table is disposed.</exception>
        public ReadOnlyCollection<IColumn> Columns
        {
            get
            {
                ThrowIfDisposed();

                return _header.Columns;
            }
        }

        /// <summary>
        /// Gets a date the table was last modified.
        /// </summary>
        public DateTime LastModified
        {
            get
            {
                ThrowIfDisposed();

                return _header.LastModified;
            }
        }

        /// <summary>
        /// Opens a reader of the table with the default <c>ASCII</c> encoding. Only one reader per table can be opened.
        /// </summary>
        /// <returns>A reader of the table.</returns>
        /// <exception cref="InvalidOperationException">Another reader of the table is opened.</exception>
        /// <exception cref="ObjectDisposedException">The table is disposed.</exception>
        public Reader OpenReader()
        {
            return OpenReader(Encoding.ASCII);
        }

        /// <summary>
        /// Opens a reader of the table with the specified encoding. Only one reader per table can be opened.
        /// </summary>
        /// <param name="encoding">The encoding that is used to load the rows content.</param>
        /// <returns>A reader of the table.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="encoding"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Another reader of the table is opened.</exception>
        /// <exception cref="ObjectDisposedException">The table is disposed.</exception>
        public virtual Reader OpenReader(Encoding encoding)
        {
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            ThrowIfDisposed();
            if (_isReaderOpened)
            {
                throw new InvalidOperationException("The table can open only one reader.");
            }

            _isReaderOpened = true;

            return CreateReader(encoding);
        }

        /// <summary>
        /// Creates a <see cref="Reader"/> instance.
        /// </summary>
        /// <param name="encoding">The encoding that is passed to the new <see cref="Reader"/> instance.</param>
        /// <returns>A <see cref="Reader"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="encoding"/> is <c>null</c>.</exception>
        protected virtual Reader CreateReader(Encoding encoding)
        {
            return new Reader(this, encoding);
        }

        /// <summary>
        /// Closes the underlying stream.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            Disposing();
            _disposed = true;
        }

        /// <summary>
        /// Releases the underlying stream.
        /// <remarks>
        /// The method is called only when the <see cref="Dispose"/> method is called for the first time.
        /// You MUST always call the base implementation.
        /// </remarks>
        /// </summary>
        protected virtual void Disposing()
        {
            _reader.Dispose();
        }

        /// <summary>
        /// Gets a dBASE header.
        /// </summary>
        protected Header Header
        {
            get
            {
                return _header;
            }
        }

        /// <summary>
        /// Throws a <see cref="ObjectDisposedException"/> if the table is already disposed.
        /// </summary>
        protected void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        Header IParentTable.Header
        {
            get { return Header; }
        }

        BinaryReader IParentTable.BinaryReader
        {
            get { return _reader; }
        }

        void IParentTable.ThrowIfDisposed()
        {
            ThrowIfDisposed();
        }
    }
}
