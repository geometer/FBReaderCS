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
using FBReader.AppServices.ViewModels.Pages;
using FBReader.AppServices.ViewModels.Pages.Catalogs;

namespace FBReader.AppServices.Tombstone
{
    public class CatalogPageViewModelStorageHandler : StorageHandler<CatalogPageViewModel>
    {
        public override void Configure()
        {
            Property(vm => vm.CatalogId).InPhoneState().RestoreAfterActivation();
            Property(vm => vm.IsSearchEnabled).InPhoneState().RestoreAfterActivation();
            Property(vm => vm.SavedInTombstone).InPhoneState().RestoreAfterActivation();
            Property(vm => vm.CanRefresh).InPhoneState();
        }

        public override void Save(CatalogPageViewModel instance, StorageMode mode)
        {
            instance.SavedInTombstone = true;
            instance.SaveState(instance.ToString());

            base.Save(instance, mode);
        }
    }
}