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
using System.Linq;
using System.Threading;
using FBReader.Common.Exceptions;
using FBReader.Common.ExtensionMethods;
using FBReader.DataModel.Model;
using FBReader.DataModel.Repositories;
using FBReader.Render.Downloading.Model;
using FBReader.Render.Parsing;
using FBReader.Render.Tools;
using FBReader.Tokenizer.Parsers;
using ICSharpCode.SharpZipLib;

namespace FBReader.Render.Downloading
{
    public class BookDownloader : IBookDownloader
    {
        private const string DATA_SOURCE_NOT_FOUND_MESSAGE = "Data Source not found.";
        private const string BOOK_HAS_NO_ID_MESSAGE = "Book has no ID.";
        
        private readonly IBookRepository _bookService;
        private readonly IFileLoadingFactory _fileLoadingFactory;
        private readonly IDownloadsContainer _container;
        private readonly IBookDownloadsRepository _bookDownloadsRepository;
        private Thread _downloadingThread;
        private volatile bool _shouldStop;

        public BookDownloader(IBookRepository bookService, IFileLoadingFactory fileLoadingFactory, IDownloadsContainer container, 
            IBookDownloadsRepository bookDownloadsRepository)
        {
            _bookService = bookService;
            _fileLoadingFactory = fileLoadingFactory;
            _container = container;
            _bookDownloadsRepository = bookDownloadsRepository;
        }

        public bool IsStarted
        {
            get; 
            private set;
        }

        public event EventHandler<DownloadItemDataModel> DownloadCompleted;

        public event EventHandler<DownloadItemDataModel> DownloadError;

        public void Start()
        {
            if (IsStarted)
            {
                return;
            }

            _shouldStop = false;
            IsStarted = true;

            if (_downloadingThread == null)
            {
                _downloadingThread = new Thread(DownloadBook);
                _downloadingThread.Start();
            }
        }

        public void Stop()
        {
            if (IsStarted)
            {
                _shouldStop = true;
                IsStarted = false;
            }
        }

        private void DownloadBook()
        {
            try
            {
                foreach (var model in _bookDownloadsRepository.GetItems())
                {
                    _container.Enqueue(new DownloadItemDataModel(model));
                }

                for (;;)
                {
                    var timeOut = 100;
                    if (_container.Count > 0)
                    {
                        var item = _container.Peek();
                        if (item != null)
                        {
                            timeOut = 1;
                            HandleBookDownloading(item, _container);
                        }
                    }

                    Thread.Sleep(timeOut);

                    if (_shouldStop)
                    {
                        break;
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void HandleBookDownloading(DownloadItemDataModel downloadItemDataModel, IDownloadsContainer instance)
        {
            if (ProcessBook(downloadItemDataModel))
            {
                RemoveItem(downloadItemDataModel, instance);
                _bookDownloadsRepository.Remove(downloadItemDataModel.DownloadID);

                OnDownloadCompleted(downloadItemDataModel);
            }
            else if (downloadItemDataModel.Status == DownloadStatus.Error)
            {
                OnDownloadError(downloadItemDataModel);
            }
        }

        private bool ProcessBook(DownloadItemDataModel item)
        {
            if (item.Canceled)
            {
                return false;
            }
            
            if (item.Status == DownloadStatus.Error)
            {
                return false;
            }

            if (item.Status == DownloadStatus.Pending)
            {
                SetItemStatus(item, DownloadStatus.Downloading);
                return ProcessBook(item);
            }

            if (item.Status == DownloadStatus.Downloading)
            {
                DownloadBook(item);
                return ProcessBook(item);
            }

            if (item.Status == DownloadStatus.Parsing)
            {
                ParseBook(item);
                return ProcessBook(item);
            }

            return item.Status == DownloadStatus.Completed;
        }

        private void DownloadBook(DownloadItemDataModel item)
        {
            try
            {
                using (var isostore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    for (;;)
                    {
                        var fileLoader = _fileLoadingFactory.GetFileLoader(item.DataSourceID, item.IsTrial);
                        if (fileLoader == null)
                        {
                            throw new Exception(DATA_SOURCE_NOT_FOUND_MESSAGE);
                        }
                        try
                        {
                            var file = fileLoader.LoadFile(item.Path, item.IsZip);

                            SaveToFile(PrepareFilePath(item, isostore), file);
                            SetItemStatus(item, DownloadStatus.Parsing);
                            break;
                        }
                        catch (RestartException)
                        {
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                HandleDownloadException(item, exception);
            }
        }

        private static void SaveToFile(string path, Stream stream)
        {
            using (var storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
            {
                stream.Position = 0L;
                using (var storageFileStream = storeForApplication.OpenFile(path, FileMode.CreateNew, FileAccess.Write))
                {
                    stream.CopyTo(storageFileStream);
                }
            }
        }

        private static void HandleDownloadException(DownloadItemDataModel item, Exception exception)
        {
            if (exception is ThreadAbortException)
            {
                throw exception;
            }

            SetItemStatus(item, DownloadStatus.Error);
        }

        private void ParseBook(DownloadItemDataModel item)
        {
            try
            {
                if (item.BookID == Guid.Empty)
                {
                    throw new Exception(BOOK_HAS_NO_ID_MESSAGE);
                }

                using (var storeForApplication = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (var bookStorageFileStream = new IsolatedStorageFileStream(CreateBookPath(item), FileMode.Open, storeForApplication))
                    {
                        var previewGenerator = BookFactory.GetPreviewGenerator(item.Type, item.Name, bookStorageFileStream);
                        var bookSummary = previewGenerator.GetBookPreview();

                        var trialBook = _bookService.GetTrials().SingleOrDefault(t => t.CatalogItemId == item.CatalogItemId);
                        if (trialBook != null)
                        {
                            _bookService.Remove(trialBook.BookID);
                        }

                        if (string.IsNullOrEmpty(bookSummary.UniqueID) || _bookService.CheckUniqueId(bookSummary.UniqueID))
                        {
                            SaveBook(item, bookSummary, previewGenerator, storeForApplication);
                        }
                        else
                        {
                            DeleteUnnecessaryInfo(item, bookSummary);
                        }

                        SetItemStatus(item, DownloadStatus.Completed);
                    }
                }
            }
            catch (SharpZipBaseException)
            {
                SetItemStatus(item, DownloadStatus.Error);
            }
            catch (Exception)
            {
                SetItemStatus(item, DownloadStatus.Error);
            }
        }

        private void SaveBook(DownloadItemDataModel item, BookSummary bookSummary, IBookSummaryParser previewGenerator, IsolatedStorageFile storeForApplication)
        {
            using (var imageStorageFileStream = new IsolatedStorageFileStream(CreateImagesPath(item), FileMode.Create, storeForApplication))
            {
                previewGenerator.SaveImages(imageStorageFileStream);
            }

            previewGenerator.SaveCover(item.BookID.ToString());

            var book = CreateBook(item, bookSummary);

            try
            {
                _bookService.Add(book);
                TokensTool.SaveTokens(book, previewGenerator);
                book.Hidden = book.Trial;
                _bookService.Save(book);
                IsolatedStorageSettings.ApplicationSettings.Save();
            }
            catch (Exception)
            {
                _bookService.Remove(book.BookID);
                throw;
            }
        }

        private static BookModel CreateBook(DownloadItemDataModel item, BookSummary bookSummary)
        {
            var book = new BookModel
                {
                    BookID = item.BookID.ToString(),
                    Title = bookSummary.Title.SafeSubstring(1024),
                    Author = bookSummary.AuthorName.SafeSubstring(1024),
                    Type = item.Type,
                    Hidden = true,
                    Trial = item.IsTrial,
                    Deleted = false,
                    CreatedDate = DateTime.Now.ToFileTimeUtc(),
                    UniqueID = bookSummary.UniqueID.SafeSubstring(1024),
                    Description = bookSummary.Description,
                    Language = bookSummary.Language,
                    Url = item.Path,
                    CatalogItemId = item.CatalogItemId
                };

            return book;
        }

        private void DeleteUnnecessaryInfo(DownloadItemDataModel item, BookSummary bookSummary)
        {
            _bookService.Remove(item.BookID.ToString());
            if (string.IsNullOrEmpty(bookSummary.UniqueID))
            {
                return;
            }

            var bookModel = _bookService.GetBookByUniqueId(bookSummary.UniqueID);
            if (bookModel == null)
            {
                return;
            }

            try
            {
                item.BookID = Guid.Parse(bookModel.BookID);
            }
            catch (Exception)
            {
            }
        }

        private static void SetItemStatus(DownloadItemDataModel item, DownloadStatus status)
        {
            var wait = new AutoResetEvent(false);

            Execute.OnUIThread(() =>
                {
                    item.Status = status;
                    wait.Set();
                });
            wait.WaitOne();
        }

        private static void RemoveItem(DownloadItemDataModel item, IDownloadsContainer downloads)
        {
            var wait = new AutoResetEvent(false);
            Execute.OnUIThread((() =>
                                        {
                                            downloads.Remove(item);
                                            wait.Set();
                                        }));
            wait.WaitOne();
        }

        private static string PrepareFilePath(DownloadItemDataModel item, IsolatedStorageFile storage)
        {
            if (item.BookID == Guid.Empty || !storage.DirectoryExists(item.BookID.ToString()))
            {
                item.BookID = CreateFolder(storage);
            }

            var bookPath = CreateBookPath(item);
            if (storage.FileExists(bookPath))
            {
                storage.DeleteFile(bookPath);
            }

            return bookPath;
        }

        private static Guid CreateFolder(IsolatedStorageFile storage)
        {
            var guid = Guid.NewGuid();
            storage.CreateDirectory(guid.ToString());
            return guid;
        }

        private static string CreateBookPath(DownloadItemDataModel item)
        {
            return Path.Combine(item.BookID.ToString(), ModelConstants.BOOK_FILE_DATA_PATH);
        }

        private static string CreateImagesPath(DownloadItemDataModel item)
        {
            return Path.Combine(item.BookID.ToString(), ModelConstants.BOOK_IMAGES_FILE_NAME);
        }

        private void OnDownloadCompleted(DownloadItemDataModel book)
        {
            var handler = DownloadCompleted;
            if (handler != null)
            {
                handler(this, book);
            }
        }

        private void OnDownloadError(DownloadItemDataModel book)
        {
            var handler = DownloadError;
            if (handler != null)
            {
                handler(this, book);
            }
        }
    }
}