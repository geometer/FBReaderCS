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
using FBReader.AppServices.Services;
using FBReader.AppServices.ViewModels.Base;
using FBReader.DataModel.Repositories;

namespace FBReader.AppServices.ViewModels.Pages
{
    public class EditBookPageViewModel : BusyViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly ITileManager _tileManager;
        private readonly IBookRepository _bookRepository;

        public EditBookPageViewModel(
            IBookRepository bookRepository, 
            INavigationService navigationService,
            ITileManager tileManager)
        {
            _bookRepository = bookRepository;
            _navigationService = navigationService;
            _tileManager = tileManager;
        }

        public string PageTitle
        {
            get; 
            set;
        }

        public string BookId
        {
            get;
            set;
        }

        public string Title { get; set; }

        public void MakeNewTitle()
        {
            IsBusy = true;

            var book = _bookRepository.Get(BookId, false);
            book.Title = Title;
            _bookRepository.Save(book);
            _tileManager.UpdateTileIfExists(BookId);

            IsBusy = false;

            _navigationService.GoBack();
        }
    }
}