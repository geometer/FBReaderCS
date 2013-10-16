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
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using Caliburn.Micro;
using FBReader.App.Controls;
using FBReader.App.Resources.TelerikLocalization;
using FBReader.App.Views.Pages;
using FBReader.App.Views.Pages.Bookmarks;
using FBReader.App.Views.Pages.Catalogs;
using FBReader.AppServices;
using FBReader.AppServices.CatalogReaders.AcquisitionServices;
using FBReader.AppServices.CatalogReaders.Authorization;
using FBReader.AppServices.CatalogReaders.Readers;
using FBReader.AppServices.Controller;
using FBReader.AppServices.Controller.Translation;
using FBReader.AppServices.DataModels;
using FBReader.AppServices.Services;
using FBReader.AppServices.Tombstone;
using FBReader.AppServices.Tombstone.StateSaving;
using FBReader.AppServices.ViewModels.Pages;
using FBReader.AppServices.ViewModels.Pages.Bookmarks;
using FBReader.AppServices.ViewModels.Pages.Catalogs;
using FBReader.AppServices.ViewModels.Pages.MainHub;
using FBReader.AppServices.ViewModels.Pages.Settings;
using FBReader.Common;
using FBReader.Common.Analytics;
using FBReader.Common.ExtensionMethods;
using FBReader.DataModel.Repositories;
using FBReader.Localization;
using FBReader.PhoneServices;
using FBReader.Render.Downloading;
using FBReader.Render.Downloading.Model;
using FBReader.Render.Tools;
using FBReader.Settings;
using FBReader.WebClient;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using NavigationCoercion;
using Telerik.Windows.Controls;
using Execute = Caliburn.Micro.Execute;
using Screen = FBReader.Common.Screen;

namespace FBReader.App
{
    public class AppBootstrapper : PhoneBootstrapper
    {
        private PhoneContainer _phoneContainer;
        private IAnalytics _analytics;

        protected override void Configure()
        {
            _phoneContainer = new PhoneContainer();
            _analytics = new FlurryAnalytics();

            RegisterConventions();
            
            _phoneContainer.RegisterPhoneServices(RootFrame);

            RegisterServices();
            RegisterViews();
            
            RegisterStorageHandlers();
            RegisterNavigationCoercion();

            Execute.InitializeWithDispatcher();

            LoadLanguagesAsync();

            Telerik.Windows.Controls.LocalizationManager.GlobalStringLoader = new ResourceLoader();
            Log.Init(new DebugLogger());
            InitErrorHandler();

            RootFrame.Navigating += RootFrameNavigating;
            RootFrame.Navigated += RootFrameNavigated;
        }

        private bool _reset;
        void RootFrameNavigating(object sender, NavigatingCancelEventArgs e)
        {
            if (_reset && e.IsCancelable && e.Uri.OriginalString.EndsWith("MainPage.xaml"))
            {
                e.Cancel = true;
                _reset = false;
            }
        }

        void RootFrameNavigated(object sender, NavigationEventArgs e)
        {
            _reset = e.NavigationMode == NavigationMode.Reset;
        }

        private async void LoadLanguagesAsync()
        {
            if (!AppSettings.Default.AreTranslateLanguagesSet)
            {
                try
                {
                    var translateController = new TranslateController();
                    var languages = await translateController.GetLanguages() ?? new List<string>();
                    var cultureList = languages.Select(l =>
                                                       {
                                                           try
                                                           {
                                                               return new CultureInfo(l);
                                                           }
                                                           catch (CultureNotFoundException)
                                                           {
                                                               return null;
                                                           }
                                                       })
                                               .Where(x => x != null)
                                               .ToList();

                    AppSettings.Default.TranslateLanguages = cultureList;
                }
                catch (TranslationException)
                {
                    // Oopsy -> Do nothing, user will see default languages.
                }
            }
        }

        private void StartBookDownloading()
        {
            var downloadController = (DownloadController) _phoneContainer.GetInstance(typeof (DownloadController), null);
            downloadController.StartDownload();
        }


        private void InitErrorHandler()
        {
            ApplicationUsageHelper.Init(Assembly.GetExecutingAssembly().GetVersion());
            var diagnostics = new RadDiagnostics();
            diagnostics.ExceptionOccurred += OnExceptionOccurred;
            diagnostics.HandleUnhandledException = true;
            diagnostics.Init();
        }

        private void OnExceptionOccurred(object sender, ExceptionOccurredEventArgs e)
        {
            
#if DEBUG
            e.Handled = true;
            e.Cancel = true;

            Deployment.Current.Dispatcher.BeginInvoke(
                () => MessageBox.Show(string.Format("{0}\n\n{1}", e.Exception.Message, e.Exception.StackTrace)));
#else
            var diagnostics = (RadDiagnostics)sender;
            diagnostics.EmailTo = string.Format(UINotifications.DiagnosticsEmail_To_Format, "sergey.nikonenko@cactussoft.biz");
            diagnostics.EmailSubject = UINotifications.DiagnosticsEmail_Subject;
            diagnostics.MessageBoxInfo.Content = UINotifications.DiagnosticsEmail_Message_Content;
            diagnostics.MessageBoxInfo.Title = UINotifications.DiagnosticsEmail_Message_Title;
#endif
        }


        private void ApplySettings()
        {
#if DEBUG
            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
#else
            PhoneApplicationService.Current.UserIdleDetectionMode = AppSettings.Default.LockScreen
                                                                        ? IdleDetectionMode.Enabled
                                                                        : IdleDetectionMode.Disabled;
#endif

            var localizationManager = (ILocalizationManager)_phoneContainer.GetInstance(typeof(ILocalizationManager), null);
            localizationManager.Reset(AppSettings.Default.CurrentUILanguage);
        }


        private void RegisterNavigationCoercion()
        {
            var fluent = new FluentNavigation(RootFrame);

            fluent.WhenNavigatedTo<ReadPage>()
                .ThenTo<BookmarksPivot>()
                .ThenTo<ReadPage>()
                .RemoveEntriesFromBackStack(2);

            fluent.WhenNavigatedTo<ReadPage>()
                .ThenTo<ContentsPage>()
                .ThenTo<ReadPage>()
                .RemoveEntriesFromBackStack(2);

            fluent.WhenNavigatedTo<ReadPage>()
                .ThenTo<SearchInBookPage>()
                .ThenTo<ReadPage>()
                .RemoveEntriesFromBackStack(2);

            fluent.WhenNavigatedTo<ReadPage>()
                .ThenTo<BookmarksPivot>()
                .ThenTo<BookmarkSearchPage>()
                .ThenTo<ReadPage>()
                .RemoveEntriesFromBackStack(3);

            fluent.WhenNavigatedTo<ReadPage>()
                .ThenTo<ReadPage>()
                .RemoveEntriesFromBackStack(1);

            fluent.WhenNavigatedTo<ReadPage>()
                .ThenTo<BookInfoPage>()
                .ThenTo<ReadPage>()
                .RemoveEntriesFromBackStack(2);

            fluent.WhenNavigatedTo<CatalogPage>()
                .ThenTo<AuthorizationPage>()
                .ThenTo<CatalogPage>()
                .RemoveEntriesFromBackStack(2);

            fluent.WhenNavigatedTo<BookInfoPage>()
                .ThenTo<AuthorizationPage>()
                .ThenTo<BookInfoPage>()
                .RemoveEntriesFromBackStack(2);

            fluent.WhenNavigatedTo<BookInfoPage>()
                .ThenTo<WebBrowserPage>()
                .ThenTo<BookInfoPage>()
                .RemoveEntriesFromBackStack(2);
        }

        private static void RegisterConventions()
        {

            // Add FBReader.AppServices dll to Caliburn assembly list
            AssemblySource.Instance.Add(typeof(MainPageViewModel).Assembly);

            MessageBinder.CustomConverters.Add(
                typeof(int?),
                (val, ctx) =>
                {
                    if (val == null) return null;
                    return Convert.ToInt32(val);
                });

            AddGenericItemsControlConvention<Pivot>(
                () => Pivot.HeaderTemplateProperty,
                () => Pivot.SelectedItemProperty);

            ViewModelLocator.AddNamespaceMapping("FBReader.App.Views.Pages", "FBReader.AppServices.ViewModels.Pages", "Page");
            ViewLocator.AddNamespaceMapping("FBReader.AppServices.ViewModels.Pages", "FBReader.App.Views.Pages", "Page");

            ViewModelLocator.AddNamespaceMapping("FBReader.App.Views.Pages.Settings", "FBReader.AppServices.ViewModels.Pages.Settings", "");
            ViewLocator.AddNamespaceMapping("FBReader.AppServices.ViewModels.Pages.Settings", "FBReader.App.Views.Pages.Settings", "");

            ViewModelLocator.AddNamespaceMapping("FBReader.App.Views.Pages.Settings", "FBReader.AppServices.ViewModels.Pages.Settings", "View");
            ViewLocator.AddNamespaceMapping("FBReader.AppServices.ViewModels.Pages.Settings", "FBReader.App.Views.Pages.Settings", "View");

            ViewModelLocator.AddNamespaceMapping("FBReader.App.Views.Pages.Settings", "FBReader.AppServices.ViewModels.Pages.Settings", "Page");
            ViewLocator.AddNamespaceMapping("FBReader.AppServices.ViewModels.Pages.Settings", "FBReader.App.Views.Pages.Settings", "Page");

            ViewModelLocator.AddNamespaceMapping("FBReader.App.Views.Pages.MainHub", "FBReader.AppServices.ViewModels.Pages.MainHub");
            ViewLocator.AddNamespaceMapping("FBReader.AppServices.ViewModels.Pages.MainHub", "FBReader.App.Views.Pages.MainHub");

            ViewModelLocator.AddNamespaceMapping("FBReader.App.Views.Pages.Bookmarks", "FBReader.AppServices.ViewModels.Pages.Bookmarks", "");
            ViewLocator.AddNamespaceMapping("FBReader.AppServices.ViewModels.Pages.Bookmarks", "FBReader.App.Views.Pages.Bookmarks", "");

            ViewModelLocator.AddNamespaceMapping("FBReader.App.Views.Pages.Bookmarks", "FBReader.AppServices.ViewModels.Pages.Bookmarks", "View");
            ViewLocator.AddNamespaceMapping("FBReader.AppServices.ViewModels.Pages.Bookmarks", "FBReader.App.Views.Pages.Bookmarks", "View");

            ViewModelLocator.AddNamespaceMapping("FBReader.App.Views.Pages.Catalogs", "FBReader.AppServices.ViewModels.Pages.Catalogs", "");
            ViewLocator.AddNamespaceMapping("FBReader.AppServices.ViewModels.Pages.Catalogs", "FBReader.App.Views.Pages.Catalogs", "");
        }

        private void RegisterViews()
        {
            _phoneContainer.PerRequest<MainPageViewModel>();
            _phoneContainer.PerRequest<ReadPageViewModel>();
            _phoneContainer.PerRequest<ContentsPageViewModel>();
            _phoneContainer.PerRequest<CatalogsViewModel>();
            _phoneContainer.PerRequest<UserLibraryViewModel>();
            _phoneContainer.PerRequest<UserLibraryPageViewModel>();
            _phoneContainer.PerRequest<AddCatalogPageViewModel>();
            _phoneContainer.PerRequest<CatalogPageViewModel>();
            _phoneContainer.PerRequest<CatalogSearchPageViewModel>();
            _phoneContainer.PerRequest<AllCatalogsSearchPageViewModel>();
            _phoneContainer.PerRequest<SearchPageViewModel>();
            _phoneContainer.PerRequest<WebBrowserPageViewModel>();
            _phoneContainer.PerRequest<AuthorizationPageViewModel>();

            //settings
            _phoneContainer.PerRequest<SettingsPivotViewModel>();
            _phoneContainer.PerRequest<GeneralViewModel>();
            _phoneContainer.PerRequest<FormattingViewModel>();
            _phoneContainer.PerRequest<AboutViewModel>();

            _phoneContainer.PerRequest<LockScreenSettingPageViewModel>();
            _phoneContainer.PerRequest<OrientationSettingPageViewModel>();
            _phoneContainer.PerRequest<FontSettingPageViewModel>();
            _phoneContainer.PerRequest<MenuSettingPageViewModel>();
            _phoneContainer.PerRequest<MarginsSettingPageViewModel>();
            _phoneContainer.PerRequest<ThemeSettingPageViewModel>();
            _phoneContainer.PerRequest<CSSSettingPageViewModel>();
            _phoneContainer.PerRequest<HyphenationSettingPageViewModel>();
            _phoneContainer.PerRequest<FlippingSettingPageViewModel>();
            _phoneContainer.PerRequest<ScreenBrightnessPageViewModel>();

            //bookmarks
            _phoneContainer.PerRequest<BookmarksPivotViewModel>();
            _phoneContainer.PerRequest<ThisBookBookmarksViewModel>();
            _phoneContainer.PerRequest<AllBooksBookmarksViewModel>();
            _phoneContainer.PerRequest<BookmarkSearchPageViewModel>();

            _phoneContainer.PerRequest<SearchInBookPageViewModel>();
            _phoneContainer.PerRequest<BookInfoPageViewModel>();
            _phoneContainer.PerRequest<EditBookPageViewModel>();

            _phoneContainer.PerRequest<DownloadListPageViewModel>();
        }

        private void RegisterServices()
        {
            _phoneContainer.RegisterInstance(typeof(IAnalytics), null, _analytics);

            _phoneContainer.RegisterHandler(typeof(ILocalizationManager), null, c => Application.Current.Resources["Localization"]);
            _phoneContainer.RegisterHandler(typeof(IBusyIndicatorManager), null, c => BusyIndicatorManager.Create((PhoneApplicationPage)RootFrame.Content));
            _phoneContainer.Singleton<SettingsController>();
            _phoneContainer.PerRequest<INotificationsService, NotificationsService>();
            _phoneContainer.PerRequest<IErrorHandler, ErrorHandler>();

            _phoneContainer.PerRequest<ITileManager, TileManager>();
            _phoneContainer.PerRequest<IBookRepository, BookRepository>();
            _phoneContainer.PerRequest<ICatalogRepository, CatalogRepository>();
            _phoneContainer.PerRequest<IWebDataGateway, WebDataGateway>();
            _phoneContainer.PerRequest<IWebClient, WebClient.WebClient>();
            _phoneContainer.PerRequest<ICatalogReaderFactory, CatalogReaderFactory>();
            _phoneContainer.PerRequest<ICatalogAuthorizationFactory, CatalogAuthorizationFactory>();
            _phoneContainer.PerRequest<IAcquisitionServiceFactory, AcquisitionServiceFactory>();

            _phoneContainer.Singleton<BookmarksController>();
            _phoneContainer.PerRequest<IBookmarkRepository, BookmarkRepository>();

            _phoneContainer.Singleton<SearchInBookController>();

            _phoneContainer.PerRequest<BookSearch>();

            _phoneContainer.Singleton<ISdCardStorage, SdCardStorage>();
            _phoneContainer.PerRequest<DataBaseInitializer>();

            _phoneContainer.PerRequest<IStorageStateSaver, StorageStateSaver>();

            _phoneContainer.PerRequest<ILiveLogin, LiveLogin>();
            _phoneContainer.PerRequest<ISkyDriveService, SkyDriveService>();

            _phoneContainer.Handler<AppSettings>(container => AppSettings.Default);

            _phoneContainer.PerRequest<SharingDataModel>();

            _phoneContainer.PerRequest<IBusyOverlayManager, BusyOverlayManager>();

            _phoneContainer.PerRequest<BookTool>();

            _phoneContainer.Singleton<CatalogController>();

            _phoneContainer.PerRequest<IFileLoadingFactory, FileLoadingFactory>();
            _phoneContainer.Singleton<DownloadController>();
            _phoneContainer.Singleton<IBookDownloader, BookDownloader>();
            _phoneContainer.Singleton<IDownloadsContainer, DownloadsContainer>();
            _phoneContainer.PerRequest<IBookDownloadsRepository, BookDownloadsRepository>();


        }

        private void RegisterStorageHandlers()
        {
            _phoneContainer.Singleton<IStorageHandler, AddCatalogPageViewModelStorageHandler>();
            _phoneContainer.Singleton<IStorageHandler, CatalogPageViewModelStorageHandler>();
            _phoneContainer.Singleton<IStorageHandler, ContentPageViewModelStorageHandler>();
            _phoneContainer.Singleton<IStorageHandler, ReadPageViewModelStorageHandler>();
            _phoneContainer.Singleton<IStorageHandler, SearchInBookPageViewModelStorageHandler>();

        }

        protected override object GetInstance(Type serviceType, string key)
        {
            return _phoneContainer.GetInstance(serviceType, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return _phoneContainer.GetAllInstances(serviceType);
        }

        protected override void BuildUp(object instance)
        {
            _phoneContainer.BuildUp(instance);
        }

        protected override void OnUnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            base.OnUnhandledException(sender, e);
            _analytics.LogError(e.ExceptionObject);
        }

        protected override void OnLaunch(object sender, LaunchingEventArgs e)
        {
            base.OnLaunch(sender, e);
            _analytics.StartSession();
            ApplySettings();

            StartBookDownloading();
        }

        protected override void OnActivate(object sender, ActivatedEventArgs e)
        {
            base.OnActivate(sender, e);
            if (e.IsApplicationInstancePreserved)
            {
                _analytics.StartSession();
            }

            ApplySettings();
        }

        protected override void OnDeactivate(object sender, DeactivatedEventArgs e)
        {
            base.OnDeactivate(sender, e);
            _analytics.EndSession();
        }

        protected override PhoneApplicationFrame CreatePhoneApplicationFrame()
        {
            var frame = new RadPhoneApplicationFrame
                {
                            Background = new SolidColorBrush(Colors.Black)
                        };
            frame.UriMapper = new AssociationUriMapper();

            Screen.Init(frame);
//
//            frame.Navigated += (sender, args) =>
//                {
//                    if (args.NavigationMode == NavigationMode.New && args.Content != null)
//                    {
//                        AliveChecker.Monitor(args.Content);
//                    }
//                };

            return frame;
        }

        private static void AddGenericItemsControlConvention<T>(
            Func<DependencyProperty> headerTemplateProperty,
            Func<DependencyProperty> selectedItemProperty
            ) where T : ItemsControl
        {
            ConventionManager.AddElementConvention<T>(
                ItemsControl.ItemsSourceProperty,
                "SelectedItem",
                "SelectionChanged").ApplyBinding =
                (viewModelType, path, property, element, convention) =>
                {
                    if (ConventionManager
                        .GetElementConvention(typeof(ItemsControl))
                        .ApplyBinding(viewModelType, path, property, element, convention))
                    {
                        ConventionManager.ConfigureSelectedItem(
                            element, selectedItemProperty(), viewModelType, path);
                        ConventionManager.ApplyHeaderTemplate(
                            element, headerTemplateProperty(), null, viewModelType);
                        return true;
                    }

                    return false;
                };
        }
    }
}
