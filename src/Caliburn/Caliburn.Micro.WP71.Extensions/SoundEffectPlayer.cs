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
    using System.Windows;
    using System.Windows.Threading;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Audio;

    /// <summary>
    ///   Service allowing to play a .wav sound effect
    /// </summary>
    public interface ISoundEffectPlayer {
        /// <summary>
        ///   Plays a sound effect
        /// </summary>
        /// <param name="wavResource"> The uri of the resource containing the .wav file </param>
        void Play(Uri wavResource);
    }

    /// <summary>
    ///   Default <see cref="ISoundEffectPlayer" /> implementation, using Xna Framework. The sound effect is played without interrupting the music playback on the phone (which is required for the app certification in the WP7 Marketplace. Also note that using the Xna Framework in a WP7 Silverlight app is explicitly allowed for this very purpose.
    /// </summary>
    public class XnaSoundEffectPlayer : ISoundEffectPlayer {
        static XNAFrameworkDispatcherUpdater updater = new XNAFrameworkDispatcherUpdater();

        /// <summary>
        ///   Plays a sound effect
        /// </summary>
        /// <param name="wavResource"> The uri of the resource containing the .wav file </param>
        public void Play(Uri wavResource) {
            var res = Application.GetResourceStream(wavResource);
            using(var stream = res.Stream) {
                var effect = SoundEffect.FromStream(stream);
                effect.Play();
            }
        }

        class XNAFrameworkDispatcherUpdater {
            readonly DispatcherTimer timer;

            public XNAFrameworkDispatcherUpdater() {
                timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
                timer.Tick += OnTick;
                FrameworkDispatcher.Update();
            }

            void OnTick(object sender, EventArgs e) {
                FrameworkDispatcher.Update();
            }
        }
    }
}