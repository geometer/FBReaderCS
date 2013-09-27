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

	/// <summary>
	/// WindowManager extensions
	/// </summary>
	public static class WindowManagerExtensions {
		/// <summary>
		///   Shows a modal dialog for the specified model, using vibrate and audio feedback
		/// </summary>
		/// <param name = "windowManager">The WindowManager instance.</param>
		/// <param name = "rootModel">The root model.</param>
		/// <param name = "context">The context.</param>
		/// <param name="wavOpeningSound">If not null, use the specified .wav as opening sound</param>
		/// <param name="vibrate">If true, use a vibration feedback on dialog opening</param>
		public static void ShowDialogWithFeedback(this IWindowManager windowManager, object rootModel, object context = null, Uri wavOpeningSound= null, bool vibrate = true) {
			if (wavOpeningSound != null) {
				IoC.Get<ISoundEffectPlayer>().Play(wavOpeningSound);
			}

			if (vibrate) {
				IoC.Get<IVibrateController>().Start(TimeSpan.FromMilliseconds(200));
			}

			windowManager.ShowDialog(rootModel, context);
		}
	}
}