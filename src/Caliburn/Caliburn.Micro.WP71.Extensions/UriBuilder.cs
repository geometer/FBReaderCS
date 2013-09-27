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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// Builds a Uri in a strongly typed fashion, based on a ViewModel.
    /// </summary>
    /// <typeparam name="TViewModel"></typeparam>
    public class UriBuilder<TViewModel> {
        readonly Dictionary<string, string> queryString = new Dictionary<string, string>();
        INavigationService navigationService;

        /// <summary>
        /// Adds a query string parameter to the Uri.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="property">The property.</param>
        /// <param name="value">The property value.</param>
        /// <returns>Itself</returns>
        public UriBuilder<TViewModel> WithParam<TValue>(Expression<Func<TViewModel, TValue>> property, TValue value) {
            if (value is ValueType || !ReferenceEquals(null, value)) {
                queryString[property.GetMemberInfo().Name] = value.ToString();
            }

            return this;
        }

        /// <summary>
        /// Attaches a navigation servies to this builder.
        /// </summary>
        /// <param name="navigationService">The navigation service.</param>
        /// <returns>Itself</returns>
        public UriBuilder<TViewModel> AttachTo(INavigationService navigationService) {
            this.navigationService = navigationService;
            return this;
        }

        /// <summary>
        /// Navigates to the Uri represented by this builder.
        /// </summary>
        public void Navigate() {
            var uri = BuildUri();

            if (navigationService == null) {
                throw new InvalidOperationException("Cannot navigate without attaching an INavigationService. Call AttachTo first.");
            }
#if WinRT
            navigationService.NavigateToViewModel<TViewModel>(uri.AbsoluteUri);
#else
            navigationService.Navigate(uri);
#endif
        }

        /// <summary>
        /// Builds the URI.
        /// </summary>
        /// <returns>A uri constructed with the current configuration information.</returns>
        public Uri BuildUri() {
            var viewType = ViewLocator.LocateTypeForModelType(typeof(TViewModel), null, null);
            if(viewType == null) {
                throw new InvalidOperationException(string.Format("No view was found for {0}. See the log for searched views.", typeof(TViewModel).FullName));
            }

            var packUri = ViewLocator.DeterminePackUriFromType(typeof(TViewModel), viewType);
            var qs = BuildQueryString();
#if WinRT
            // We need a value uri here otherwise there are problems using uri as a parameter
            return new Uri("caliburn://" + packUri + qs, UriKind.Absolute);
#else
            return new Uri(packUri + qs, UriKind.Relative);
#endif
        }

        string BuildQueryString() {
            if (queryString.Count < 1) {
                return string.Empty;
            }

            var result = queryString
                .Aggregate("?", (current, pair) => current + (pair.Key + "=" + Uri.EscapeDataString(pair.Value) + "&"));

            return result.Remove(result.Length - 1);
        }
    }
}
