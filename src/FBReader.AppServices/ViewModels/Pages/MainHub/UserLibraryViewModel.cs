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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Caliburn.Micro;
using FBReader.AppServices.Events;
using FBReader.DataModel.Model;
using FBReader.DataModel.Repositories;
using FBReader.Localization;
using FBReader.Render.Downloading.Model;

namespace FBReader.AppServices.ViewModels.Pages.MainHub
{
    public class UserLibraryViewModel : Screen, IHandle<UILanguageChanged>, IHandle<BookDownloaded>, IHandle<BookRemoved>
    {
        private readonly INavigationService _navigationService;
        private readonly IBookRepository _bookRepository;

        public UserLibraryViewModel(IBookRepository bookRepository, IEventAggregator eventAggregator, INavigationService navigationService)
        {
            _bookRepository = bookRepository;
            _navigationService = navigationService;
            DisplayName = UIStrings.UserLibraryView_Title;

            eventAggregator.Subscribe(this);
        }

        public bool IsEmpty { get; set; }

        public IEnumerable<BookModel> Books { get; set; }

        public void LoadData()
        {
            ThreadPool.QueueUserWorkItem(callback =>
                {
                    Books = new ObservableCollection<BookModel>((_bookRepository.GetRecent(6)));
                    IsEmpty = Books == null || !Books.Any();
                });
        }

        public void Handle(UILanguageChanged message)
        {
            DisplayName = UIStrings.UserLibraryView_Title;
        }

        public void Handle(BookDownloaded message)
        {
            if(message.Book.Status == DownloadStatus.Error)
                return;

            Books = null;
            LoadData();
        }

        public void Handle(BookRemoved message)
        {
            Books = null;
            LoadData();
        }

        public void NavigateToItem(BookModel model)
        {
            _navigationService.UriFor<ReadPageViewModel>()
                              .WithParam(vm => vm.BookId, model.BookID)
                              .WithParam(vm => vm.ToLastReadPage, true)
                              .Navigate();
        }

        
    }
}