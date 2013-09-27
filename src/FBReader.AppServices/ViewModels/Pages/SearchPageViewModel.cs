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
using Caliburn.Micro;
using FBReader.AppServices.ViewModels.Base;
using FBReader.DataModel.Model;
using FBReader.DataModel.Repositories;

namespace FBReader.AppServices.ViewModels.Pages
{
    public class SearchPageViewModel : BusyViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IBookRepository _bookRepository;
        private IEnumerable<BookModel> _booksSource;
        private IEnumerable<BookModel> _books;
        private string _searchQuery;
        private bool _isEmpty;

        public SearchPageViewModel(INavigationService navigationService, IBookRepository bookRepository)
        {
            _navigationService = navigationService;
            _bookRepository = bookRepository;
        }

        public string SearchQuery
        {
            get
            {
                return _searchQuery;
            }
            set
            {
                _searchQuery = value;
                NotifyOfPropertyChange(() => SearchQuery);
            }
        }

        public bool IsEmpty
        {
            get { return _isEmpty; }
            set
            {
                _isEmpty = value;
                NotifyOfPropertyChange(() => IsEmpty);
            }
        }

        public IEnumerable<BookModel> Books
        {
            get
            {
                return _books;
            }
            set
            {
                _books = value;
                NotifyOfPropertyChange(() => Books);
            }
        }

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);

            StartBusiness();

            _booksSource = _bookRepository.GetAll();
            if (!string.IsNullOrEmpty(_searchQuery))
            {
                Search();
            }

            StopBusiness();
        }

        public void Search()
        {
            if (IsBusy || string.IsNullOrEmpty(_searchQuery))
            {
                return;
            }

            StartBusiness();
            IsEmpty = false;

            Books = _booksSource.Where(b => b.Title.ToLower().Contains(_searchQuery.ToLower()));
            IsEmpty = Books == null || !Books.Any();

            StopBusiness();
        }

        public void NavigateToItem(BookModel model)
        {
            _navigationService.UriFor<ReadPageViewModel>()
                              .WithParam(vm => vm.BookId, model.BookID)
                              .WithParam(vm => vm.TokenOffset, model.CurrentTokenID)
                              .Navigate();
        }
    }
}