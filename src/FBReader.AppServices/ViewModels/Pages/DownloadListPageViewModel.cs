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

using Caliburn.Micro;
using FBReader.DataModel.Repositories;
using FBReader.Render.Downloading.Model;

namespace FBReader.AppServices.ViewModels.Pages
{
    public class DownloadListPageViewModel : Screen
    {
        private readonly IBookDownloadsRepository _bookDownloadsRepository;

        public IDownloadsContainer DownloadsContainer { get; private set; }

        public DownloadListPageViewModel(IDownloadsContainer downloadsContainer, IBookDownloadsRepository bookDownloadsRepository)
        {
            DownloadsContainer = downloadsContainer;
            _bookDownloadsRepository = bookDownloadsRepository;
        }

        public void Remove(DownloadItemDataModel item)
        {
            _bookDownloadsRepository.Remove(item.DownloadID);
        }

        public void Restart(DownloadItemDataModel item)
        {
            item.Status = DownloadStatus.Pending;
        }
    }
}
