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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FBReader.Common.Exceptions;
using Microsoft.Phone.Storage;

namespace FBReader.PhoneServices
{
    public class SdCardStorage : ISdCardStorage
    {
        private ExternalStorageDevice _sdCardStorage;

        public async Task<bool> GetIsAvailableAsync()
        {
            _sdCardStorage = (await ExternalStorage.GetExternalStorageDevicesAsync()).FirstOrDefault();
            return _sdCardStorage != null;
        }

        public async Task<IEnumerable<ExternalStorageFile>> GetFilesAsync(params string[] extensions)
        {
            if (_sdCardStorage == null)
            {
                _sdCardStorage = (await ExternalStorage.GetExternalStorageDevicesAsync()).FirstOrDefault();
            }

            //suppose SD-card is null
            if (_sdCardStorage == null)
            {
                throw new SdCardNotSupportedException();    
            }
            
            //read all files recursively
            var files = new List<ExternalStorageFile>();
            await GetFilesAsync(_sdCardStorage.RootFolder, files);
            return files;
        }

        public async Task GetFilesAsync(ExternalStorageFolder folder, List<ExternalStorageFile> files)
        {
            var subFolders = await folder.GetFoldersAsync();
            foreach (var subFolder in subFolders)
            {
                await GetFilesAsync(subFolder, files);
            }
            

            files.AddRange(await folder.GetFilesAsync());
        }
    }
}
