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
using System.Threading;
using Caliburn.Micro;
using FBReader.AppServices.Events;
using FBReader.AppServices.Services;
using FBReader.AppServices.ViewModels.Base;
using FBReader.DataModel.Model;
using FBReader.DataModel.Repositories;
using FBReader.Localization;
using FBReader.Render.Downloading.Model;

namespace FBReader.AppServices.ViewModels.Pages
{
    public class UserLibraryPageViewModel : BusyViewModel, IHandle<BookDownloaded>
    {
        private readonly Dictionary<SortMode, string> _sortModesCollection = new Dictionary<SortMode, string>
            {
                {SortMode.Name, UIStrings.UserLibraryPage_NameSortMode},
                {SortMode.Author, UIStrings.UserLibraryPage_AuthorSortMode}
            };

        private readonly INavigationService _navigationService;
        private readonly IEventAggregator _eventAggregator;
        private readonly ITileManager _tileManager;
        private readonly IBookRepository _bookRepository;
        private KeyValuePair<SortMode, string> _selectedSortMode;

        public UserLibraryPageViewModel(
            IBookRepository bookRepository, 
            INavigationService navigationService,
            IEventAggregator eventAggregator,
            ITileManager tileManager)
        {
            _bookRepository = bookRepository;
            _navigationService = navigationService;
            _eventAggregator = eventAggregator;
            _tileManager = tileManager;
            DisplayName = UIStrings.UserLibraryPage_Title;
            PageTitle = UIStrings.UserLibraryPage_Title;

            _selectedSortMode = _sortModesCollection.SingleOrDefault(s => s.Key == SortMode.Name);

            eventAggregator.Subscribe(this);
        }

        public bool ShowOnlyFavourites
        {
            get; 
            set;
        }

        public string PageTitle { get; set; }

        public Dictionary<SortMode, string> SortModes
        {
            get { return _sortModesCollection; }
        }

        public bool IsOpenSortModes { get; set; }

        public string EmptyContent { get; set; }

        public KeyValuePair<SortMode, string> SelectedSortMode
        {
            get { return _selectedSortMode; }
            set
            {
                if (value.Key != _selectedSortMode.Key)
                {
                    _selectedSortMode = value;
                    NotifyOfPropertyChange(() => SelectedSortMode);
                }

                SortBooks(Books);
                IsOpenSortModes = false;
            }
        }

        public IObservableCollection<BookModel> Books { get; set; }

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);

            EmptyContent = ShowOnlyFavourites
                               ? UIStrings.UserLibraryPage_Favs_Empty
                               : UIStrings.UserLibraryView_NoItemsText;

        }

        protected override void OnActivate()
        {
            base.OnActivate();
            LoadData();
        }

        public void NavigateToItem(BookModel model)
        {
            _navigationService.UriFor<ReadPageViewModel>()
                              .WithParam(vm => vm.BookId, model.BookID)
                              .WithParam(vm => vm.ToLastReadPage, true)
                              .Navigate();
        }

        private void LoadData()
        {
            if (Books != null)
            {
                return;
            }

            StartBusiness();

            ThreadPool.QueueUserWorkItem(callback =>
                {
                    var books = new BindableCollection<BookModel>(ShowOnlyFavourites
                                    ? _bookRepository.GetFavourites()
                                    : _bookRepository.GetAll());

                    SortBooks(books);
                    StopBusiness();
                });
        }

        private void SortBooks(IEnumerable<BookModel> books)
        {
            if(books == null)
                return;

            switch (_selectedSortMode.Key)
            {
                case SortMode.Author:
                    Books = new BindableCollection<BookModel>(books.OrderBy(b => b.Author));
                    break;
                case SortMode.Name:
                    Books = new BindableCollection<BookModel>(books.OrderBy(b => b.Title));
                    break;
            }
        }

        public void Handle(BookDownloaded message)
        {
            if(message.Book.Status == DownloadStatus.Error)
                return;

            LoadData();
        }

        public void RemoveBook(BookModel book)
        {
            Books.Remove(book);
            _bookRepository.Remove(book.BookID);
            _tileManager.DeleteTile(book.BookID);
            _eventAggregator.Publish(new BookRemoved(book));
        }

        public void NavigateToSearch()
        {
            _navigationService.UriFor<SearchPageViewModel>().Navigate();
        }

        public void RemoveFromFavourites(BookModel bookModel)
        {
            bookModel.IsFavourite = false;
            _bookRepository.Save(bookModel);
            Books.Remove(bookModel);
        }

        public bool CanPinToStart(BookModel bookModel)
        {
            return !_tileManager.IsPinnedToDesktop(bookModel.BookID);
        }

        public void PinToStart(BookModel bookModel)
        {
            _tileManager.CreateTileForBook(bookModel.BookID);
        }
    }

    public enum SortMode
    {
        None,
        Author,
        Name,
    }
}