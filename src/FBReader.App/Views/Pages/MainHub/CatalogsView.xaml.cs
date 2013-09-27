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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using FBReader.AppServices.DataModels;
using FBReader.AppServices.ViewModels.Pages.MainHub;
using FBReader.DataModel.Model;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Telerik.Windows.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace FBReader.App.Views.Pages.MainHub
{
    public partial class CatalogsView : UserControl
    {
        private RadDataBoundListBoxItem _focusedItem;

        private CatalogsViewModel ViewModel
        {
            get { return (CatalogsViewModel) DataContext; }
        }

        public CatalogsView()
        {
            InitializeComponent();
        }

        private void RadContextMenu_OnOpening(object sender, ContextMenuOpeningEventArgs e)
        {
            _focusedItem = e.FocusedElement as RadDataBoundListBoxItem;
            if (_focusedItem == null)
            {
                // We don't want to open the menu if the focused element is not a list box item.
                // If the list box is empty focusedItem will be null.
                e.Cancel = true;
                return;
            }
            e.Cancel = !ViewModel.CanRemoveCatalog((CatalogDataModel) _focusedItem.DataContext);
        }

        private void UIElement_OnTap(object sender, GestureEventArgs e)
        {
            ViewModel.RemoveCatalog((CatalogDataModel)_focusedItem.DataContext);
        }
    }
}
