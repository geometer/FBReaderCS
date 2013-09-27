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

namespace Caliburn.Micro {
    using System.Windows.Navigation;
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Shell;

    /// <summary>
    /// A custom bootstrapper designed to setup phone applications.
    /// </summary>
    public abstract class PhoneBootstrapperBase : BootstrapperBase {
        bool phoneApplicationInitialized;

        /// <summary>
        /// The phone application service.
        /// </summary>
        protected PhoneApplicationService PhoneService { get; private set; }

        /// <summary>
        /// The root frame used for navigation.
        /// </summary>
        protected PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneBootstrapperBase"/> class.
        /// </summary>
        protected PhoneBootstrapperBase() : base(true) { }

        /// <summary>
        /// Provides an opportunity to hook into the application object.
        /// </summary>
        protected override void PrepareApplication() {
            base.PrepareApplication();

            PhoneService = new PhoneApplicationService();
            PhoneService.Activated += OnActivate;
            PhoneService.Deactivated += OnDeactivate;
            PhoneService.Launching += OnLaunch;
            PhoneService.Closing += OnClose;

            Application.ApplicationLifetimeObjects.Add(PhoneService);

            if (phoneApplicationInitialized) {
                return;
            }

            RootFrame = CreatePhoneApplicationFrame();
            RootFrame.Navigated += OnNavigated;

            phoneApplicationInitialized = true;
        }

        void OnNavigated(object sender, NavigationEventArgs e) {
            if (Application.RootVisual != RootFrame) {
                Application.RootVisual = RootFrame;
            }
        }

        /// <summary>
        /// Creates the root frame used by the application.
        /// </summary>
        /// <returns>The frame.</returns>
        protected virtual PhoneApplicationFrame CreatePhoneApplicationFrame() {
            return new PhoneApplicationFrame();
        }

        /// <summary>
        /// Occurs when a fresh instance of the application is launching.
        /// </summary>
        protected virtual void OnLaunch(object sender, LaunchingEventArgs e) { }

        /// <summary>
        /// Occurs when a previously tombstoned or paused application is resurrected/resumed.
        /// </summary>
        protected virtual void OnActivate(object sender, ActivatedEventArgs e) { }

        /// <summary>
        /// Occurs when the application is being tombstoned or paused.
        /// </summary>
        protected virtual void OnDeactivate(object sender, DeactivatedEventArgs e) { }

        /// <summary>
        /// Occurs when the application is closing.
        /// </summary>
        protected virtual void OnClose(object sender, ClosingEventArgs e) { }
    }

    /// <summary>
    /// A custom bootstrapper designed to setup phone applications.
    /// </summary>
    public class PhoneBootstrapper : PhoneBootstrapperBase {
        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneBootstrapper"/> class.
        /// </summary>
        public PhoneBootstrapper() {
            Start();
        }
    }
}
