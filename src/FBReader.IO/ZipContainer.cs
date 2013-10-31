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
using System.Linq;
using FBReader.Common.ExtensionMethods;
using ICSharpCode.SharpZipLib.Zip;

namespace FBReader.IO
{
    public class ZipContainer
    {
        private ZipContainer(IList<ZippedFile> zippedFiles)
        {
            Files = zippedFiles;
        }

        public IList<ZippedFile> Files { get; private set; }

        public Stream GetFileStream(string path, bool throwException = true, bool convertEncoding = false)
        {
            ZippedFile file = Files.FirstOrDefault(t => (t.FileName == path.Replace('\\', '/')));
            if (file == null)
            {
                if (throwException)
                {
                    throw new FileNotFoundException("File '" + path + "' not found.");
                }
                return null;
            }
            Stream stream = file.Stream;
            stream.Position = 0L;
            if (convertEncoding && !stream.CheckUTF8())
            {
                stream = stream.Win1251ToUTF8();
            }
            return stream;
        }

        public static ZipContainer Unzip(Stream stream)
        {
            ZipEntry entry;
            var zippedFiles = new List<ZippedFile>();
            var source = new ZipInputStream(stream);
            while ((entry = source.GetNextEntry()) != null)
            {
                if (entry.IsFile)
                {
                    var destination = new MemoryStream();
                    source.CopyTo(destination);
                    zippedFiles.Add(new ZippedFile(destination, entry.Name.Replace('\\', '/')));
                }
            }
            return new ZipContainer(zippedFiles);
        }
    }
}