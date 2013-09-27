/*
 * Author: CactusSoft (http://cactussoft.biz/), 2013
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA
 * 02110-1301, USA.
 */

using System;
using System.IO;
using System.Threading;

namespace FBReader.IO
{
    public class FileWrapper : IDisposable {
        private readonly string _path;
        private readonly BinaryReader _reader;
        private readonly FileStorage _storage;
        private readonly Stream _stream;
        private readonly BinaryWriter _writer;
        private bool _disposed;
        private bool _locked;
        private long _position;

        internal FileWrapper(string path, Stream stream, FileStorage storage)
        {
            _path = path;
            _stream = stream;
            _storage = storage;
            _reader = new BinaryReader(stream);
            _writer = new BinaryWriter(stream);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _storage.Close(this);
                _disposed = true;
            }
        }

        public FileSync Lock()
        {
            if (_locked)
            {
                throw new InvalidOperationException("Can't lock locked file.");
            }
            Monitor.Enter(_stream);
            _locked = true;
            _stream.Position = _position;
            return new FileSync(this);
        }

        public void Seek(int offset, SeekOrigin origin)
        {
            _stream.Seek(offset, origin);
            _position = _stream.Position;
        }

        public string Path
        {
            get { return _path; }
        }

        public int Position
        {
            get { return (_locked ? ((int) _stream.Position) : ((int) _position)); }
        }

        public BinaryReader Reader
        {
            get { return _reader; }
        }

        public BinaryWriter Writer
        {
            get { return _writer; }
        }

        public void Unlock()
        {
            if (_locked)
            {
                _position = _stream.Position;
                _locked = false;
                Monitor.Exit(_stream);
            }
        }

        public override int GetHashCode()
        {
            return _path.GetHashCode();
        }
    }
}