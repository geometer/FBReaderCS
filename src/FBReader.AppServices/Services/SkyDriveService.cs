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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FBReader.Common;
using FBReader.Common.Exceptions;
using FBReader.DataModel.Model;
using FBReader.DataModel.Repositories;
using FBReader.Render.Tools;
using Microsoft.Live;

namespace FBReader.AppServices.Services
{
    public class SkyDriveService : ISkyDriveService
    {
        private const string SkyDriveFolder = "FBReaderBooks";
        private readonly ILiveLogin _liveLogin;
        private readonly BookTool _bookTool;
        private readonly ICatalogRepository _catalogRepository;

        public SkyDriveService(
            ILiveLogin liveLogin, 
            BookTool bookTool,
            ICatalogRepository catalogRepository)
        {
            _liveLogin = liveLogin;
            _bookTool = bookTool;
            _catalogRepository = catalogRepository;
        }

        public async Task<string> UploadAsync(BookModel book, CancellationToken cancellationToken)
        {

            LiveConnectClient skyDrive;

            try
            {
                skyDrive = await _liveLogin.Login();
                if (skyDrive == null)
                    return null;

                var catalog = _catalogRepository.GetAll().Single(c => c.Type == CatalogType.SkyDrive);
                catalog.AccessDenied = false;
                _catalogRepository.Save(catalog);

            }
            catch (LiveAuthException e)
            {
                if (e.ErrorCode != "access_denied")
                {
                    throw;
                }
                return null;
            }
            catch (Exception e)
            {
                throw new FileUploadException(e.Message, e);
            }

            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var root = (List<dynamic>) (await skyDrive.GetAsync("me/skydrive/files")).Result["data"];
                dynamic folder = root.SingleOrDefault(f => f.type == "folder" && f.name == SkyDriveFolder);
                string folderId;
                if (folder != null)
                {
                    folderId = folder.id;
                }
                else
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var folderData = new Dictionary<string, object> {{"name", SkyDriveFolder}};
                    var createFolderData = await skyDrive.PostAsync("me/skydrive", folderData);
                    var createFolderResult = createFolderData.Result;
                    folderId = (string) createFolderResult["id"];
                }

                cancellationToken.ThrowIfCancellationRequested();

                string fileId;

                var fileName = GetFileName(book);
                var files = (List<dynamic>) (await skyDrive.GetAsync(folderId + "/files")).Result["data"];
                dynamic file = files.SingleOrDefault(f => f.type == "file" && f.name == fileName);
                if (file != null)
                {
                    fileId = file.id;
                }
                else
                {
                    using (Stream fileStream = _bookTool.GetOriginalBook(book))
                    {
                        dynamic fileUploadResult = (await skyDrive.UploadAsync(folderId, GetFileName(book), fileStream,
                                                        OverwriteOption.DoNotOverwrite, cancellationToken, null)).Result;
                        fileId = fileUploadResult.id;
                    }
                }

                cancellationToken.ThrowIfCancellationRequested();

                LiveOperationResult operationResult = await skyDrive.GetAsync(fileId + "/shared_read_link");
                dynamic result = operationResult.Result;




                return GetDirectLink(result.link);

            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new FileUploadException(e.Message, e);
            }
        }

        private string GetDirectLink(string uri)
        {
            return uri.Replace("redir.aspx", "download.aspx");
        }

        private string GetFileName(BookModel book)
        {
            var name = string.Format("{0} - {1}.{2}", book.Author, book.Title, book.BookID);
            var invalidChars = Path.GetInvalidFileNameChars();
            var cleanFileName = new string(name.Where(m => !invalidChars.Contains(m)).ToArray());
            return string.Format("{0}.{1}", cleanFileName, book.Type);
        }
    }
}
