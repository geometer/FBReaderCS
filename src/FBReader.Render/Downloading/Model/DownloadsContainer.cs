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
using System.Collections.ObjectModel;
using System.Linq;
using FBReader.Common;

namespace FBReader.Render.Downloading.Model
{
    public class DownloadsContainer : NotifyPropertyChangedBase, IDownloadsContainer
    {
        private static readonly object SyncObject = new object();
        private readonly ObservableCollection<DownloadItemDataModel> _items = new ObservableCollection<DownloadItemDataModel>();

        public int Count
        {
            get; 
            private set;
        }

        public ObservableCollection<DownloadItemDataModel> Items
        {
            get
            {
                return _items;
            }
        }

        public void Enqueue(DownloadItemDataModel dataModel)
        {
            lock (SyncObject)
            {
                if (GetDataModelIndex(dataModel) >= 0)
                {
                    return;
                }

                _items.Add(dataModel);
                Count = _items.Count;
            }
        }

        public int GetDataModelIndex(DownloadItemDataModel data)
        {
            for (var i = 0; i < _items.Count; i++)
            {
                var model = _items[i];
                if (((model.DataSourceID == data.DataSourceID) && string.Equals(model.Path, data.Path, StringComparison.CurrentCultureIgnoreCase)) &&
                    string.Equals(model.Name, data.Name, StringComparison.CurrentCultureIgnoreCase))
                {
                    return i;
                }
            }
            return -1;
        }

        public DownloadItemDataModel Peek()
        {
            lock (SyncObject)
            {
                return _items.FirstOrDefault(t => (t.Status != DownloadStatus.Error));
            }
        }

        public void Remove(DownloadItemDataModel dataModel)
        {
            lock (SyncObject)
            {
                int index = GetDataModelIndex(dataModel);
                if (index < 0)
                {
                    return;
                }

                _items.RemoveAt(index);
                Count = _items.Count;
            }
        }

        protected override void RaisePropertyChanged(string propertyName)
        {
            base.RaisePropertyChanged(propertyName);
            if (propertyName == "Count")
            {
                RaisePropertyChanged("Text");
            }
        }
    }
}