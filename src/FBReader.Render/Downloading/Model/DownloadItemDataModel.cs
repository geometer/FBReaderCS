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

using System;
using FBReader.Common;
using FBReader.DataModel.Model;

namespace FBReader.Render.Downloading.Model
{
    public class DownloadItemDataModel : NotifyPropertyChangedBase
    {
        private DownloadStatus _status;

        public DownloadItemDataModel(BookDownloadModel model)
        {
            DownloadID = model.DownloadID;
            DataSourceID = model.DataSourceID;
            Path = model.Path;
            Name = model.Name;
            Type = model.Type;
            IsZip = model.IsZip;
            Status = DownloadStatus.Pending;
            Canceled = false;
            BookID = Guid.Empty;
            IsTrial = model.IsTrial;
            CatalogItemId = model.CatalogItemId;
            Author = model.Author;
            Description = model.Description;
            AcquisitionUrl = model.AcquisitionUrl;
            AcquisitionType = model.AcquisitionType;
            AcquisitionPrices = model.AcquisitionPrices;
        }

        public string AcquisitionPrices { get; set; }

        public string AcquisitionType { get; set; }

        public string AcquisitionUrl { get; set; }

        public string Description { get; set; }

        public string Author { get; set; }

        public Guid BookID { get; set; }

        public bool Canceled { get; private set; }

        public int DataSourceID { get; set; }

        public int DownloadID { get; set; }

        public bool IsZip { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }

        public bool IsTrial { get; set; }

        public string CatalogItemId { get; set; }

        public DownloadStatus Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
                RaisePropertyChanged("Status");
            }
        }

        public string Type { get; set; }

        public void Cancel()
        {
            Canceled = true;
        }
    }
}