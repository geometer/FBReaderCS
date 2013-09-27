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
using Caliburn.Micro;
using FBReader.Localization;

namespace FBReader.App.DesignViewModels.Pages
{
    public class UserLibraryPageDesignViewModel : Screen
    {
        public UserLibraryPageDesignViewModel()
        {
            DisplayName = UIStrings.UserLibraryPage_Title;

            Books = new List<BookItemViewModel>
                {
                    new BookItemViewModel {Author = "Jill Robi Jill Robi Jill Robi Jill Robi Jill Robi Jill Robi Jill Robi", Name = "JUST ANOTHER SAD, LOVE POEM ABOUT LOVE JUST ANOTHER SAD, LOVE POEM ABOUT LOVE", ImageSource = "DesignBookCover.jpg"},
                    new BookItemViewModel {Author = "В. Чаплина", Name = "ЧЕТВЕРОНОГИЕ ДРУЗЬЯ", ImageSource = "DesignBookCover.jpg"},
                    new BookItemViewModel {Author = "Oscar Wilde", Name = "PICTURE OF DORIAN GRAY", ImageSource = "DesignBookCover.jpg"},
                    new BookItemViewModel {Author = "М. Булгаков", Name = "Мастер и Маргарита", ImageSource = "DesignBookCover.jpg"},
                    new BookItemViewModel {Author = "Erich Maria Remarque", Name = "LIFE ON LOAN", ImageSource = "DesignBookCover.jpg"},
                    new BookItemViewModel {Author = "Charles Dickens", Name = "THE ADVENTURES OF OLIVER TWIST", ImageSource = "DesignBookCover.jpg"},
                    new BookItemViewModel {Author = "М. Булгаков", Name = "Мастер и Маргарита", ImageSource = "DesignBookCover.jpg"},
                    new BookItemViewModel {Author = "Erich Maria Remarque", Name = "LIFE ON LOAN", ImageSource = "DesignBookCover.jpg"},
                    new BookItemViewModel {Author = "Charles Dickens", Name = "THE ADVENTURES OF OLIVER TWIST", ImageSource = "DesignBookCover.jpg"}
                };

            IsOpenSortModes = false;
        }

        public List<BookItemViewModel> Books
        {
            get; 
            set;
        }

        public bool IsEmpty
        {
            get; 
            set;
        }

        public bool IsOpenSortModes
        {
            get; 
            set;
        }
    }

    public class BookItemViewModel
    {
        public string Author
        {
            get; 
            set;
        }

        public string Name
        {
            get; 
            set;
        }

        public string ImageSource
        {
            get; 
            set;
        }
    }
}