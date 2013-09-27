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
using System.Windows.Controls;
using FBReader.AppServices.ViewModels.Pages;
using Microsoft.Phone.Controls;

namespace FBReader.App.Views.Pages
{
    public partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void PanoramaOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var viewModel = (MainPageViewModel) DataContext;
            viewModel.AppBarSelectedIndex = ((Panorama) sender).SelectedIndex;
        }

        private void OnAddClick(object sender, EventArgs e)
        {
            ((MainPageViewModel) DataContext).AddCatalog();
        }

        private void OnSearchCatalogsClick(object sender, EventArgs e)
        {
            ((MainPageViewModel) DataContext).NavigateToSearchCatalogs();
        }

        private void Downloads(object sender, EventArgs e)
        {
            ((MainPageViewModel)DataContext).NavigateToDownloads();
        }

        private void Settings(object sender, EventArgs e)
        {
            ((MainPageViewModel)DataContext).NavigateToSettings();
        }

        private void OnToFavouritesClick(object sender, EventArgs e)
        {
            ((MainPageViewModel)DataContext).NavigateToFavourites();
        }

        private void NavigateToSearchBooks(object sender, EventArgs e)
        {
            ((MainPageViewModel) DataContext).NavigateToSearch();
        }
    }
}