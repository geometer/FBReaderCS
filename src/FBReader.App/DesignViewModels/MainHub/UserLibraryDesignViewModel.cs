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

namespace FBReader.App.DesignViewModels.MainHub
{
    public class UserLibraryDesignViewModel: Screen
    {
        public UserLibraryDesignViewModel()
        {
            DisplayName = UIStrings.UserLibraryView_Title;

            Books = new List<BookItemDesignViewModel>
                        {
                            new BookItemDesignViewModel
                                {
                                    Author = "Александр Ворошилов",
                                    Title = "ПЕРВАЯ ПОБЕДА",
                                    ImageSource = "DesignBookCover.jpg"
                                },
                            new BookItemDesignViewModel
                                {
                                    Author = "В. Чаплина",
                                    Title = "ЧЕТВЕРОНОГИЕ ДРУЗЬЯ",
                                    ImageSource = "DesignBookCover.jpg"
                                },
                            new BookItemDesignViewModel
                                {
                                    Author = "Оскар Уайлф",
                                    Title = "СКАЗКИ",
                                    ImageSource = "DesignBookCover.jpg"
                                },
                            new BookItemDesignViewModel
                            {
                                Author = "Jill Robi",
                                Title = "ANOTHER SAD LOVE STORY",
                                ImageSource = "DesignBookCover.jpg"
                            },
                            new BookItemDesignViewModel
                            {
                                Author = "J.R.R. Tolkien",
                                Title = "HOBBIT",
                                ImageSource = "DesignBookCover.jpg"
                            },
                            new BookItemDesignViewModel
                            {
                                Author = "И.П. Осадчий",
                                Title = "МЫ РОДОМ ИЗ СССР",
                                ImageSource = "DesignBookCover.jpg"
                            }
                        };

            IsEmpty = false;
        }

        public List<BookItemDesignViewModel> Books
        {
            get;
            set;
        }

        public bool IsEmpty
        {
            get;
            set;
        }
    }


    public class BookItemDesignViewModel
    {
        public string Author
        {
            get;
            set;
        }

        public string Title
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
