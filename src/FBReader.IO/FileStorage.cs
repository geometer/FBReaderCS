/*
 * Author: Vitaly Leschenko, CactusSoft (http://cactussoft.biz/), 2013
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

using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;

namespace FBReader.IO
{
    public class FileStorage
    {
        private static FileStorage _instance;
        private readonly Dictionary<string, FileCounter> _files = new Dictionary<string, FileCounter>();
        private readonly IsolatedStorageFile _storage = IsolatedStorageFile.GetUserStoreForApplication();

        protected FileStorage()
        {
        }

        public static FileStorage Instance
        {
            get { return _instance ?? (_instance = new FileStorage()); }
        }

        public void Close(FileWrapper file)
        {
            lock (this)
            {
                if (_files.ContainsKey(file.Path))
                {
                    FileCounter counter = _files[file.Path];
                    if (--counter.Count == 0)
                    {
                        counter.Stream.Close();
                        _files.Remove(file.Path);
                    }
                }
            }
        }

        public FileWrapper GetFile(string path)
        {
            lock (this)
            {
                FileCounter counter;
                if (!_files.ContainsKey(path))
                {
                    var stream = new IsolatedStorageFileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, _storage);
                    counter = _files[path] = new FileCounter(stream);
                }
                else
                {
                    counter = _files[path];
                    counter.Count++;
                }
                return new FileWrapper(path, counter.Stream, this);
            }
        }
    }
}