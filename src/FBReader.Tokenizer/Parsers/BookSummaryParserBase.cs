/*
 * Author: Vitaly Leschenko, CactusSoft (http://cactussoft.biz/), 2013
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
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading;
using System.Windows.Media.Imaging;
using FBReader.DataModel.Model;
using FBReader.Tokenizer.Data;
using FBReader.Tokenizer.Extensions;
using FBReader.Common.ExtensionMethods;

namespace FBReader.Tokenizer.Parsers
{
    public abstract class BookSummaryParserBase : IBookSummaryParser
    {
        protected BookSummaryParserBase()
        {
            Anchors = new Dictionary<string, int>();
            Chapters = new List<BookChapter>();
        }

        public Dictionary<string, int> Anchors { get; private set; }

        public List<BookChapter> Chapters { get; private set; }

        public virtual void BuildChapters()
        {
        }

        public abstract void SaveImages(Stream stream);

        public abstract bool SaveCover(string bookID);

        public abstract BookSummary GetBookPreview();

        public abstract ITokenParser GetTokenParser();

        protected bool SaveCoverImages(string bookID, Stream imageStream)
        {
            var @event = new AutoResetEvent(false);
            bool result = false;
            ((Action)(() => 
            {
                try
                {
                    var bitmapImage = new BitmapImage();
                    bitmapImage.SetSource(imageStream);

                    using (var isoStorage = IsolatedStorageFile.GetUserStoreForApplication())
                    {

                        using (var iconImageFile = isoStorage.CreateFile(ModelExtensions.GetBookCoverPath(bookID)))
                        {
                            bitmapImage.SaveJpeg(iconImageFile, 300, 300, true);
                        }

                        using (var coverImageFile = isoStorage.CreateFile(ModelExtensions.GetBookFullCoverPath(bookID)))
                        {
                            bitmapImage.SaveJpeg(coverImageFile, bitmapImage.PixelWidth, bitmapImage.PixelHeight, false);
                        }
                    }
                    result = true;
                }
                catch (Exception)
                {
                    result = false;
                }
                finally
                {
                    @event.Set();
                }
            })).OnUIThread();
            
            @event.WaitOne();
            return result;
        }
    }
}