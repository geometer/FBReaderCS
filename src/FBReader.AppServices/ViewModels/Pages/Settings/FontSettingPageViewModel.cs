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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Caliburn.Micro;
using FBReader.Settings;

namespace FBReader.AppServices.ViewModels.Pages.Settings
{
    public class FontSettingPageViewModel : Screen
    {
        public List<FontFamily> Fonts
        {
            get { return AppSettings.Default.FontSettings.Fonts; }
        }

        public FontFamily SelectedFont
        {
            get { return AppSettings.Default.FontSettings.FontFamily; }
            set { AppSettings.Default.FontSettings.FontFamily = value; }
        }

        public List<decimal> Sizes
        {
            get { return AppSettings.Default.FontSettings.Sizes; }
        }

        public decimal SelectedSize
        {
            get { return AppSettings.Default.FontSettings.FontSize; }
            set { AppSettings.Default.FontSettings.FontSize = value; }
        }

        public List<decimal> Intervals
        {
            get { return AppSettings.Default.FontSettings.Intervals; }
        }

        public decimal SelectedInterval
        {
            get { return AppSettings.Default.FontSettings.FontInterval; }
            set { AppSettings.Default.FontSettings.FontInterval = value; }
        }

        public FontSettingPageViewModel()
        {
        }

    }
}
