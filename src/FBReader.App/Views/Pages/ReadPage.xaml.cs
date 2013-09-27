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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using FBReader.App.Controls;
using FBReader.App.Controls.ApplicationBar;
using FBReader.App.Controls.Manipulations;
using FBReader.App.Interaction;
using FBReader.AppServices;
using FBReader.AppServices.Controller;
using FBReader.AppServices.Controller.Translation;
using FBReader.AppServices.ViewModels.Pages;
using FBReader.Common;
using FBReader.Common.ExtensionMethods;
using FBReader.DataModel.Model;
using FBReader.DataModel.Repositories;
using FBReader.Localization;
using FBReader.Render.RenderData;
using FBReader.Render.Tools;
using FBReader.Settings;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using Color = System.Windows.Media.Color;
using MathHelper = FBReader.App.Utils.MathHelper;
using Point = System.Windows.Point;

namespace FBReader.App.Views.Pages
{
    public partial class ReadPage : IReadView
    {
        private const string IS_FAVOURITE_ICON = "/Resources/Icons/appbar_favorites_del.png";
        private const string IS_NOT_FAVOURITE_ICON = "/Resources/Icons/appbar_favorites_add.png";
        private const string SEARCH_ICON = "/Resources/Icons/search.png";
        private const string TABLE_OF_CONTENT_ICON = "/Resources/Icons/toc.png";

        private readonly IBookRepository _bookRepository = IoC.Get<IBookRepository>();
        private readonly IBookmarkRepository _bookmarkRepository = IoC.Get<IBookmarkRepository>();
        private ReadController _readController;
        private bool _isLoaded;
        private readonly SemaphoreSlim _event = new SemaphoreSlim(1,1);
        private ManipulationListener _pageManipulationListener;
        private ManipulationListener _textManipulationListener;
        private readonly ManipulationListener _dummyManipulationListenner = new ManipulationListener();
        private bool _textManipulationStarted;
        private TextRenderData _firstWord;
        private TextRenderData _lastWord;
        private double _offsetX;
        private double _offsetY;
        private LinkRenderData _link;
        private BookData _data;
        private TranslateController _translateController = new TranslateController();

        public ReadPageViewModel ViewModel
        {
            get
            {
                return (ReadPageViewModel) DataContext;
            }
        }

        public ReadPage()
        {
            InitializeComponent();

            Unloaded += delegate
                            {
                                _isLoaded = false;
                            };
            Loaded += delegate
                          {
                              NavigationContext.QueryString.Clear();
                              _isLoaded = true;
                              Redraw();
                              SubscribeOnViewModelPropertyChanged();
                          };

            AppBar.Opened += AppBarOpened;
            AppBar.Closed += AppBarClosed;

            OrientationChanged += ReadPage_OrientationChanged;

            SupportedOrientations = AppSettings.Default.Orientation;

            ColorSelector.ColorSelected += ColorSelectorOnColorSelected;
            ColorSelector.Closed += ColorSelectorOnClosed;

            
        }

        private void SubscribeOnViewModelPropertyChanged()
        {
            ViewModel.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName.Equals("IsFavouriteBook"))
                    {
                        _book.IsFavourite = ViewModel.IsFavouriteBook;
                        UpdateIsFavouriteAppBarButton(ViewModel.IsFavouriteBook);
                    }
                };
        }

        private void AppBarClosed()
        {
            UpdateTrayVisibility();

            if (PageCanvas.Manipulator != null)
                PageCanvas.Manipulator.Resume();            
        }

        private void AppBarOpened()
        {
            UpdateTrayVisibility();

            if (PageCanvas.Manipulator != null)
                PageCanvas.Manipulator.Pause();
        }

        private void UpdateTrayVisibility()
        {
            if (Orientation == PageOrientation.PortraitUp && AppBar.IsOpen)
                SystemTray.IsVisible = true;
            else
                SystemTray.IsVisible = false;
        }

        private async void ReadPage_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            PageCanvas.SetSize(ActualWidth, ActualHeight, ActualWidth, ActualHeight - AppBar.HeightInMinimizedState);
            PageCanvas.Clear();

            UpdateTrayVisibility();

            Redraw();
        }

        private ManipulationListener InitializePageManipulationListener()
        {
            var ml = new ManipulationListener();
            ml.Hold += PageOnHold;
            ml.Started += ml_Started;
            ml.Tap += PageOnTap;
            ml.Delta += MlOnDelta;
            ml.Completed += MlOnCompleted;
            return ml;
        }

        private void MlOnDelta(object sender, ManipulationDeltaEventArgs manipulationDeltaEventArgs)
        {
            _link = null;
            PageCanvas.Manipulator.Resume();
        }

        
        private void MlOnCompleted(object sender, ManipulationCompletedEventArgs manipulationCompletedEventArgs)
        {
            if (_link != null)
            {
                PageCanvas.Manipulator.Resume();
                _link = null;
            }
        }

        private void PageOnTap(object sender, PointManipulationEventArgs e)
        {
            if (_link != null)
            {
                GotoLink(_link.LinkID);
                PageCanvas.Manipulator.Resume();
                _link = null;
            }
        }

        private void GotoLink(string linkID)
        {
            int anchorsTokenID = _data.GetAnchorsTokenId(linkID);
            if (anchorsTokenID >= 0)
            {
                _oldOffset = ViewModel.TokenOffset;

                ViewModel.TokenOffset = anchorsTokenID;
                CreateController();
            }            
        }


        private void ml_Started(object sender, ManipulationStartedEventArgs e)
        {

            var clickPoint = e.ManipulationContainer.TransformToVisual(this).Transform(e.ManipulationOrigin);

            var topToBottomInversed = new Point(clickPoint.X, ActualHeight - clickPoint.Y);

            _link = PageCanvas.CurrentLinks.FirstOrDefault(l => l.Rect.Contains(clickPoint));
            if (_link != null)
            {
                PageCanvas.Manipulator.Pause();
                return;
            }

            bool isAppBarTriangleAreTapped = MathHelper.IsPointInTriangle(
                new Point(0, AppBar.HeightInMinimizedState),
                new Point(ActualWidth/2, 120 + AppBar.HeightInMinimizedState),
                new Point(ActualWidth, AppBar.HeightInMinimizedState), topToBottomInversed);
            if (isAppBarTriangleAreTapped)
            {
                e.Handled = true;
                e.Complete();

                PageCanvas.Manipulator.CancelNextEvent();
                AppBar.Open();
            }
            else
            {
                if (AppBar.IsOpen)
                {
                    e.Handled = true;
                    e.Complete();

                    PageCanvas.Manipulator.CancelNextEvent();
                    AppBar.Close();
                }
            }

        }

        private ManipulationListener InitializeTextManipulationListener()
        {
            var manipulationListener = new ManipulationListener();
            manipulationListener.Tap += OnTextTap;
            manipulationListener.Hold += PageOnHold;
            manipulationListener.Started += OnTextManipulatonStated;
            manipulationListener.Delta += OnTextManipulatonDelta;
            manipulationListener.Completed += OnTextManipulatonCompeted;
            return manipulationListener;
        }

        private void UpdateIsFavouriteAppBarButton(bool isFavourite)
        {
            if (_book.Trial)
            {
                return;
            }
            
            AppBar.IconButtons[2].Icon = isFavourite
                                             ? new BitmapImage(new Uri(IS_FAVOURITE_ICON, UriKind.RelativeOrAbsolute))
                                             : new BitmapImage(new Uri(IS_NOT_FAVOURITE_ICON, UriKind.RelativeOrAbsolute));
            AppBar.IconButtons[2].Text = isFavourite
                                             ? UIStrings.ReadPage_AppBar_FromFavorites
                                             : UIStrings.ReadPage_AppBar_ToFavorites;
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            if (AppBar.IsOpen)  
            { 
                AppBar.Close();
                e.Cancel = true;
            }

            if (_textManipulationListener != null && _textManipulationListener.IsAttached)
            {
                ClearTextSelection();
                e.Cancel = true;
            }

            if (TranslationControl.IsOpen)
            {
                TranslationControl.Hide();
                e.Cancel = true;
            }

            if (_oldOffset != null)
            {
                ViewModel.TokenOffset = _oldOffset.Value;
                _oldOffset = null;
                CreateController();
                e.Cancel = true;
            }
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (_book != null)
            {
                try
                {
                    //_book.CurrentTokenID = _readController.Offset;
                    var book = _bookRepository.Get(ViewModel.BookId);
                    book.CurrentTokenID = _readController.Offset;
                    _bookRepository.Save(book);
                }
                catch (NullReferenceException)
                {
                    //_readController.Offset throw null ref exception if book still not prepared 
                }
            }
        }

        public async void Redraw()
        {
            if (!_isLoaded)
                return;

            using (await BusyOverlay.Create())
            {
                await _event.WaitAsync();

                _pageManipulationListener = InitializePageManipulationListener();
                _textManipulationListener = InitializeTextManipulationListener();
                ManipulationService.SetManipulationListener(PageCanvas, _pageManipulationListener);

                SystemTray.IsVisible = true;
                SystemTray.SetForegroundColor(this, AppSettings.Default.ColorScheme.SystemTrayForegroundColor);

                Background = AppSettings.Default.ColorScheme.BackgroundBrush;

                SupportedOrientations = AppSettings.Default.Orientation;

                PageCanvas.Clear();
                PageCanvas.SetSize(ActualWidth, ActualHeight, ActualWidth, ActualHeight - AppBar.HeightInMinimizedState);

                PageCanvas.AddBookmark -= AddBookmark;
                PageCanvas.AddBookmark += AddBookmark;

                PageCanvas.Translate -= Translate;
                PageCanvas.Translate += Translate;

                PageCanvas.Copy -= Copy;
                PageCanvas.Copy += Copy;

                PageCanvas.Share -= ShareText;
                PageCanvas.Share += ShareText;

                PageCanvas.Manipulator = new ManipulatorFactory(PageCanvas).CreateManipulator(AppSettings.Default.FlippingStyle, AppSettings.Default.FlippingMode);

                AppBar.Visibility = Visibility.Visible;
                AppBar.ApplicationBarMinimizedStateView = AppSettings.Default.HideMenu
                    ? ApplicationBarMinimizedState.Hidden
                    : ApplicationBarMinimizedState.Minimized;
                AppBar.Background = AppSettings.Default.ColorScheme.ApplicationBarBackgroundBrush;
                AppBar.SliderForegroundBrush = AppSettings.Default.ColorScheme.ProgressBarBrush;
                
                AppBar.PageSelected -= AppBar_PageSelected;
                AppBar.PageSelected += AppBar_PageSelected;
                AppBar.PageSelectionCancelled -= AppBarOnPageSelectionCancelled;
                AppBar.PageSelectionCancelled += AppBarOnPageSelectionCancelled;
                AppBar.PageSelectionApplied -= AppBarOnPageSelectionApplied;
                AppBar.PageSelectionApplied += AppBarOnPageSelectionApplied;

                UpdateFontMenuItemAvailability();

                _offsetX = AppSettings.Default.Margin.Left;
                _offsetY = AppSettings.Default.Margin.Top;

             
                PageCanvas.Manipulator.TurnForward += () => TurnPage(true);
                PageCanvas.Manipulator.TurnBackward += () => TurnPage(false);

                if (!string.IsNullOrEmpty(ViewModel.BookId))
                {
                    _data = new BookData(ViewModel.BookId);
                    PageCanvas.Bookmarks = _bookmarkRepository.GetBookmarks(ViewModel.BookId);
                }
                
                await CreateController();

                AppBar.IconButtons[1].IsEnabled = _data.HasTableOfContents;

                UpdateTrayVisibility();

                _event.Release();
            }
        }

        private void ShareText(string selectedText)
        {
            ViewModel.ShareText(selectedText);
            ClearTextSelection();
        }

        private async void Translate(string text)
        {
            
            try
            {
                using (await BusyOverlay.Create())
                {
                    var translatedText = await _translateController.Translate(text, AppSettings.Default.CurrentTranslateLanguage.TwoLetterISOLanguageName);
                    ClearTextSelection();
                    
                    TranslationControl.Show(translatedText);
                }
            }
            catch (TranslationException)
            {
                MessageBox.Show(UIStrings.TranslateError_Body, UIStrings.TranslateError_Title, MessageBoxButton.OK);
            }
        }

        private void AppBarOnPageSelectionApplied()
        {
            _preSelectionOffset = null;
        }

        private void AppBarOnPageSelectionCancelled()
        {
            ViewModel.TokenOffset = _preSelectionOffset.Value;
            CreateController();
        }

        private void AppBar_PageSelected(int page)
        {
            if (_preSelectionOffset == null)
                _preSelectionOffset = ViewModel.TokenOffset;

            int tokenOffset = (page - 1)*Constants.WORDS_PER_PAGE;
            ViewModel.TokenOffset = tokenOffset;

            CreateController();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);            
            
            Redraw();
        }

        private async Task CreateController()
        {
            using (await BusyOverlay.Create())
            {
                _book = _bookRepository.Get(ViewModel.BookId);
                if (_book == null)
                {
                    return;
                }

                _readController = new ReadController(PageCanvas, ViewModel.BookId, _bookRepository, ViewModel.TokenOffset);

                await _readController.ShowNextPage();
                PageCanvas.Manipulator.UpdatePanelsVisibility();
                PageCanvas.Manipulator.IsFirstPage = _readController.IsFirst;
                PageCanvas.Manipulator.IsLastPage = _readController.IsLast;

                AppBar.TotalPages = _readController.TotalPages;
                AppBar.PageNumber = _readController.CurrentPage;

                AppBar.MenuItems[2].IsEnabled = ViewModel.CanPinToDesktop;

                if (_book.Trial)
                {
                    AppBar.IconButtons = CreateAppBarIconsForTrial();
                }
                else
                {
                    AppBar.IconButtons[2].IsEnabled = true;
                    UpdateIsFavouriteAppBarButton(_book.IsFavourite);
                }
            }
        }

        private FBReaderApplicationBarItemCollection<FBReaderApplicationBarIconButton> CreateAppBarIconsForTrial()
        {
            var appBarIcons = new FBReaderApplicationBarItemCollection<FBReaderApplicationBarIconButton>(AppBar)
                {
                    new FBReaderApplicationBarIconButton
                        {
                            AppBar = AppBar,
                            Text = UIStrings.ReadPage_AppBar_Search,
                            Icon = new BitmapImage(new Uri(SEARCH_ICON, UriKind.Relative)),
                            Message = "GoToSearch"
                        },
                    new FBReaderApplicationBarIconButton
                        {
                            AppBar = AppBar,
                            Text = UIStrings.ReadPage_AppBar_TableOfContent,
                            Icon = new BitmapImage(new Uri(TABLE_OF_CONTENT_ICON, UriKind.Relative)),
                            Message = "GoToTableOfContents"
                        }
                };

            return appBarIcons;
        }

        private Point GetTapPoint(Point origin)
        {
            return new Point(origin.X - _offsetX, origin.Y - _offsetY);
        }

        #region Page manipulation event handlers

  
        private void PageOnHold(object sender, PointManipulationEventArgs e)
        {
            var point = e.ManipulationOrigin;


            var word = PageCanvas.CurrentTexts.FirstOrDefault(l => l.Rect.Contains(point));
            
            if (word == null)
                return;

            var words = PageCanvas.CurrentTexts.Where(w => w.TokenID == word.TokenID).ToList();

            word = words.FirstOrDefault();
            TextRenderData wordContinuation = words.LastOrDefault() ?? word;
            
            //TextRenderData word = PageCanvas.CurrentTexts.FirstOrDefault(l => l.Rect.Contains(point));
            
            InitTextSelection(word, wordContinuation);
        }

        #endregion

        #region Text manipulation event handlers

        private void OnTextTap(object sender, PointManipulationEventArgs e)
        {
            if (PageCanvas.CheckSelection(GetTapPoint(e.ManipulationOrigin)))
                return;

            ClearTextSelection();
        }

        private void OnTextManipulatonStated(object sender, ManipulationStartedEventArgs e)
        {
            Point manipulationPoint = GetManipulationPoint(e.ManipulationOrigin, e.ManipulationContainer);
            _firstWord = PageCanvas.FirstSelectionText;
            _lastWord = PageCanvas.LastSelectionText;
            double num1 = _firstWord.Rect.DistanceTo(manipulationPoint);
            double num2 = _lastWord.Rect.DistanceTo(manipulationPoint);
            if (num1 > num2)
            {
                TextRenderData textContext = _firstWord;
                _firstWord = _lastWord;
                _lastWord = textContext;
                num1 = num2;
            }
            _textManipulationStarted = num1 < 30.0;
        }

        private void OnTextManipulatonDelta(object sender, ManipulationDeltaEventArgs e)
        {
            Vector2 vector2 = new Vector2();
            while (TouchPanel.IsGestureAvailable)
                vector2 = TouchPanel.ReadGesture().Position;
            if (Orientation == PageOrientation.LandscapeLeft)
                vector2 = new Vector2(vector2.Y, 480f - vector2.X);
            else if (Orientation == PageOrientation.LandscapeRight)
                vector2 = new Vector2((float)ActualWidth- vector2.Y, vector2.X);
            if (!this._textManipulationStarted)
                return;
            var point = new Point(vector2.X - _offsetX, vector2.Y - _offsetY);
            TextRenderData a = PageCanvas.CurrentTexts.FirstOrDefault(l => l.Rect.Contains(point));
            if (a == null)
                return;

            if(a.TokenID < _lastWord.TokenID)
            {
                a = PageCanvas.CurrentTexts.FirstOrDefault(l => l.TokenID == a.TokenID);    
            }
            else
            {
                a = PageCanvas.CurrentTexts.LastOrDefault(l => l.TokenID == a.TokenID);
            }

            
            PageCanvas.SetSelection(a, _lastWord);

            Debug.WriteLine(_lastWord.Text);
        }

        private void OnTextManipulatonCompeted(object sender, ManipulationCompletedEventArgs e)
        {
            _textManipulationStarted = false;
            PageCanvas.ShowActionButtons();
        }

        #endregion

        private Point GetManipulationPoint(Point origin, UIElement container)
        {
            UIElement visual = PageCanvas;
            Point point = container == visual ? origin : container.TransformToVisual(visual).Transform(origin);
            return new Point(point.X - _offsetX, point.Y - _offsetY);
        }

        private void InitTextSelection(TextRenderData word, TextRenderData wordContinuation)
        {
            AppBar.Close();
            PageCanvas.Manipulator.Pause();
            PageCanvas.SetSelection(word, wordContinuation);
            ManipulationService.SetManipulationListener(PageCanvas, _textManipulationListener);
            TouchPanel.EnabledGestures = GestureType.FreeDrag;
            AppBar.Visibility = Visibility.Collapsed;
            PageCanvas.ShowActionButtons();
        }

        private void ClearTextSelection()
        {
            TouchPanel.EnabledGestures = GestureType.None;
            PageCanvas.ClearSelection();
            ManipulationService.SetManipulationListener(PageCanvas, _pageManipulationListener);
            PageCanvas.Manipulator.Resume();
            
            AppBar.Visibility = Visibility.Visible;
        }

        public void UpdateFontMenuItemAvailability()
        {
            bool isLastFontSize = AppSettings.Default.FontSettings.Sizes.IndexOf(AppSettings.Default.FontSettings.FontSize) == AppSettings.Default.FontSettings.Sizes.Count - 1;
            bool isFirstFontSize = AppSettings.Default.FontSettings.Sizes.IndexOf(AppSettings.Default.FontSettings.FontSize) == 0;
            
            AppBar.MenuItems[6].IsEnabled = !isLastFontSize;
            AppBar.MenuItems[7].IsEnabled = !isFirstFontSize;            
        }

        private async Task TurnPage(bool isRight)
        {
            _oldOffset = null;

            await _event.WaitAsync();

            AppBar.CancelPageSelectionMode();

            if (isRight)
                await _readController.ShowNextPage();
            else
                await _readController.ShowPrevPage();

            ViewModel.TokenOffset = _readController.Offset;

            AppBar.PageNumber = _readController.CurrentPage;
            PageCanvas.Manipulator.IsFirstPage = _readController.IsFirst;
            PageCanvas.Manipulator.IsLastPage = _readController.IsLast;
            PageCanvas.Manipulator.UpdatePanelsVisibility();
            
            _event.Release();
        }

        private void Copy(string selectedText)
        {
            try
            {
                Clipboard.SetText(selectedText);
                ClearTextSelection();
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        #region Bookmark handlers

        private string _bookmarkText;
        private TextRenderData _bookmarkFirstTextContext;
        private TextRenderData _bookmarkLastTextContext;
        private int? _preSelectionOffset;
        private BookModel _book;
        private int? _oldOffset;

        private void AddBookmark(string text, TextRenderData first, TextRenderData last)
        {
            ColorSelector.Show();
            
            ClearTextSelection();
            ManipulationService.SetManipulationListener(PageCanvas, _dummyManipulationListenner);

            _bookmarkText = text;
            _bookmarkFirstTextContext = first;
            _bookmarkLastTextContext = last;
        }

        private void ColorSelectorOnColorSelected(Color color)
        {
            ColorSelector.Hide();
            ManipulationService.SetManipulationListener(PageCanvas, _pageManipulationListener);
            AddBookmark(_bookmarkText, _bookmarkFirstTextContext, _bookmarkLastTextContext, color);
        }

        private void ColorSelectorOnClosed()
        {
            ManipulationService.SetManipulationListener(PageCanvas, _pageManipulationListener);
        }

        private void AddBookmark(string text, TextRenderData first, TextRenderData last, Color color)
        {
            BookmarkModel bookmark = _bookmarkRepository.AddBookmark(
                ViewModel.BookId,
                PageCanvas.Bookmarks,
                text,
                ColorHelper.ToInt32(color),
                first.TokenID,
                last.TokenID);

            var previousPageWord = PageCanvas.PreviousTexts.LastOrDefault() ?? new TextRenderData();
            if(first.TokenID == previousPageWord.TokenID)
            {
                PageCanvas.Highlight(PageCanvas.GetPrevPagePanel(), previousPageWord.Rect, previousPageWord.Rect, bookmark, color);
            }

            var nextPageWord = PageCanvas.NextTexts.FirstOrDefault() ?? new TextRenderData();
            if(last.TokenID == nextPageWord.TokenID)
            {
                PageCanvas.Highlight(PageCanvas.GetNextPagePanel(), nextPageWord.Rect, nextPageWord.Rect, bookmark, color);
            }

            PageCanvas.Highlight(PageCanvas.GetCurrentPagePanel(), first.Rect, last.Rect, bookmark, color);
        }

        #endregion
    }
}