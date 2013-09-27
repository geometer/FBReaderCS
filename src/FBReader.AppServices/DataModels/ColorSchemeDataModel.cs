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

using System.Windows.Media;
using Caliburn.Micro;
using FBReader.AppServices.Controller;
using FBReader.Settings;

namespace FBReader.AppServices.DataModels
{
    public class ColorSchemeDataModel : PropertyChangedBase
    {
        private readonly SettingsController _settingsController;
        private readonly Scheme _scheme;

        public ColorSchemeDataModel(Scheme scheme, SettingsController settingsController)
        {
            _scheme = scheme;
            _settingsController = settingsController;
        }

        public ColorSchemes ColorScheme
        {
            get
            {
                return _scheme.ColorScheme;
            }
        }

        public string SchemeName
        {
            get
            {
                return _settingsController.GetColorSchemeName(_scheme.ColorScheme);
            }
        }

        public Brush TitleForegroundBrush
        {
            get
            {
                return _scheme.TitleForegroundBrush;
            }
        }

        public Brush TextForegroundBrush
        {
            get
            {
                return _scheme.TextForegroundBrush;
            }
        }

        public Brush BackgroundBrush
        {
            get
            {
                return _scheme.BackgroundBrush;
            }
        }

        public void Update()
        {
            NotifyOfPropertyChange("SchemeName");
        }
    }
}
