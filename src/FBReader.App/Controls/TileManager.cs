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
using System.IO.IsolatedStorage;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Caliburn.Micro;
using FBReader.App.Views.Controls;
using FBReader.AppServices.Services;
using FBReader.AppServices.ViewModels.Pages;
using FBReader.DataModel.Model;
using FBReader.DataModel.Repositories;
using Telerik.Windows.Controls;

namespace FBReader.App.Controls
{
    public class TileManager : ITileManager
    {
        private readonly IBookRepository _bookRepository;
        private readonly INavigationService _navigationService;

        public TileManager(
            IBookRepository bookRepository,
            INavigationService navigationService)
        {
            _bookRepository = bookRepository;
            _navigationService = navigationService;
        }

        public void CreateTileForBook(string bookId)
        {
            if (string.IsNullOrWhiteSpace(bookId))
                throw new ArgumentException("bookId is invalid");

            var book = _bookRepository.Get(bookId);

            var uri = _navigationService.UriFor<ReadPageViewModel>()
                .WithParam(vm => vm.BookId, bookId)
                .WithParam(vm => vm.ToLastReadPage, true)
                .BuildUri();


            string title = book.Title;
            BitmapImage bmp;
            using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
            {

                if (storage.FileExists(ModelExtensions.GetBookCoverPath(book.BookID)))
                {
                    bmp = new BitmapImage() { CreateOptions = BitmapCreateOptions.None };
                    using (var file = storage.OpenFile(ModelExtensions.GetBookCoverPath(book.BookID), FileMode.Open))
                    {
                        bmp.SetSource(file);
                        title = string.Empty;
                    }
                }
                else
                    bmp = null;//.UriSource = new Uri("/Resources/Icons/tile.png", UriKind.RelativeOrAbsolute);
            }

            BookTile tile = new BookTile();
            tile.ImageSource = bmp;
            tile.Title = title;
            tile.UpdateLayout();

            LiveTileHelper.CreateOrUpdateTile(new RadExtendedTileData() {VisualElement = tile}, uri);
        }

        public void UpdateTileIfExists(string bookId)
        {
            if(IsPinnedToDesktop(bookId))
                CreateTileForBook(bookId);
        }

        public void DeleteTile(string bookId)
        {
            if (string.IsNullOrWhiteSpace(bookId))
                throw new ArgumentException("bookId is invalid");

            var uri = _navigationService.UriFor<ReadPageViewModel>()
                .WithParam(vm => vm.BookId, bookId)
                .WithParam(vm => vm.ToLastReadPage, true)
                .BuildUri();

            var tile = LiveTileHelper.GetTile(uri);
            if(tile == null)
                return;

            tile.Delete();
        }

        public bool IsPinnedToDesktop(string bookId)
        {
            if (string.IsNullOrWhiteSpace(bookId))
                throw new ArgumentException("bookId is invalid");

            var uri = _navigationService.UriFor<ReadPageViewModel>()
                .WithParam(vm => vm.BookId, bookId)
                .WithParam(vm => vm.ToLastReadPage, true)
                .BuildUri();

            bool tileExists = LiveTileHelper.GetTile(uri) != null;

            return tileExists;
        }
    }
}
