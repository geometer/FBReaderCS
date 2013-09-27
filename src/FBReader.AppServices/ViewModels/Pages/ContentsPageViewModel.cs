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
using System.Threading.Tasks;
using Caliburn.Micro;
using FBReader.AppServices.DataModels;
using FBReader.DataModel.Model;

namespace FBReader.AppServices.ViewModels.Pages
{
    public class ContentsPageViewModel : Screen
    {
        private readonly INavigationService _navigationService;

        public ContentsPageViewModel(
            INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        public string BookId { get; set; }

        public List<ChapterDataModel> Chapters { get; set; }

        public bool IsLoading { get; set; }

        public int CatalogId { get; set; }

        public CatalogBookItemModel CatalogBookItemModel { get; set; }

        public string CatalogBookItemKey { get; set; }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (TransientStorage.Contains(CatalogBookItemKey))
                CatalogBookItemModel = TransientStorage.Get<CatalogBookItemModel>(CatalogBookItemKey);

            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            try
            {
                IsLoading = true;

                Chapters = await new TaskFactory<List<ChapterDataModel>>().StartNew(LoadChapters);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private List<ChapterDataModel> LoadChapters()
        {
            using (BookDataContext context = BookDataContext.Connect())
            {
                var chaptersInBook =
                    from chapter in context.Chapters
                    where chapter.BookID == BookId
                    orderby chapter.TokenID
                    select chapter;

                var source = new List<ChapterDataModel>();
                foreach (ChapterModel chapterModel in chaptersInBook)
                {
                    int tokenID = chapterModel.TokenID;
                    var chapter = new ChapterDataModel
                                    {
                                        Depth = chapterModel.Level,
                                        Title = chapterModel.Title.Trim(),
                                        TokenId = tokenID
                                    };

                    source.Add(chapter);
                }

                return source;
            }
        }

        public void GoToChapter(ChapterDataModel chapter)
        {
            _navigationService.UriFor<ReadPageViewModel>()
                .WithParam(vm => vm.BookId, BookId)
                .WithParam(vm => vm.TokenOffset, chapter.TokenId)
                .WithParam(vm => vm.CatalogId, CatalogId)
                .WithParam(vm => vm.CatalogBookItemKey, TransientStorage.Put(CatalogBookItemModel))
                .Navigate();
        }
    }
}
