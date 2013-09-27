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
    public class CatalogsDesignViewModel : Screen
    {
        public CatalogsDesignViewModel()
        {
            DisplayName = UIStrings.CatalogsView_Title;

            Libraries = new List<LibraryItemViewModel>
                {
                    new LibraryItemViewModel
                        {
                            Name = "SMASHWORDS SMASHWORDS SMASHWORDS SMASHWORDS SMASHWORDS",
                            Description = "Ebooks from independent authors and publishers",
                            ImageSource = "DesignBookCover.jpg"
                        },
                    new LibraryItemViewModel
                        {
                            Name = "FEEDBOOKS OPDS CATALOG",
                            Description = "Feedbooks: Food for the mind. A place to discover and publish e-books.",
                            ImageSource = "DesignBookCover.jpg"
                        },
                    new LibraryItemViewModel
                        {
                            Name = "MANYBOOKS CATALOG",
                            Description = "ManyBooks.net: The best ebooks at the best price: free!",
                            ImageSource = "DesignBookCover.jpg"
                        },
                    new LibraryItemViewModel
                        {
                            Name = "КАТАЛОГ LITRES",
                            Description = "Продажа электронных книг",
                            ImageSource = "DesignBookCover.jpg"
                        },
                        new LibraryItemViewModel
                        {
                            Name = "SMASHWORDS",
                            Description = "Ebooks from independent authors and publishers",
                            ImageSource = "DesignBookCover.jpg"
                        },
                    new LibraryItemViewModel
                        {
                            Name = "FEEDBOOKS OPDS CATALOG",
                            Description = "Feedbooks: Food for the mind. A place to discover and publish e-books.",
                            ImageSource = "DesignBookCover.jpg"
                        },
                    new LibraryItemViewModel
                        {
                            Name = "MANYBOOKS CATALOG",
                            Description = "ManyBooks.net: The best ebooks at the best price: free!",
                            ImageSource = "DesignBookCover.jpg"
                        },
                    new LibraryItemViewModel
                        {
                            Name = "КАТАЛОГ LITRES",
                            Description = "Продажа электронных книг",
                            ImageSource = "DesignBookCover.jpg"
                        },
                };
        }

        public List<LibraryItemViewModel> Libraries
        {
            get; 
            set;
        }
    }

    public class LibraryItemViewModel
    {
        public string Name
        {
            get; 
            set;
        }

        public string Description
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