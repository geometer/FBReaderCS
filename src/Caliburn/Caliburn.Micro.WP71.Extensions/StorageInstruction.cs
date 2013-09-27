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
    /// An instruction for saving/loading data.
    /// </summary>
    /// <typeparam name="T">The model type.</typeparam>
    public class StorageInstruction<T> : PropertyChangedBase {
        IStorageHandler owner;
        IStorageMechanism storageMechanism;
        string key;
        Action<T, Func<string>, StorageMode> save;
        Action<T, Func<string>, StorageMode> restore;

        /// <summary>
        /// Gets or sets the owner.
        /// </summary>
        /// <value>
        /// The owner.
        /// </value>
        public IStorageHandler Owner {
            get { return owner; }
            set {
                owner = value;
                NotifyOfPropertyChange("Owner");
            }
        }

        /// <summary>
        /// Gets or sets the storage mechanism.
        /// </summary>
        /// <value>
        /// The storage mechanism.
        /// </value>
        public IStorageMechanism StorageMechanism {
            get { return storageMechanism; }
            set {
                storageMechanism = value;
                NotifyOfPropertyChange("StorageMechanism");
            }
        }

        /// <summary>
        /// Gets or sets the persistence key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string Key {
            get { return key; }
            set {
                key = value;
                NotifyOfPropertyChange("Key");
            }
        }

        /// <summary>
        /// Gets or sets the save action.
        /// </summary>
        /// <value>
        /// The save action.
        /// </value>
        public Action<T, Func<string>, StorageMode> Save {
            get { return save; }
            set {
                save = value;
                NotifyOfPropertyChange("Save");
            }
        }

        /// <summary>
        /// Gets or sets the restore action.
        /// </summary>
        /// <value>
        /// The restore action.
        /// </value>
        public Action<T, Func<string>, StorageMode> Restore {
            get { return restore; }
            set {
                restore = value;
                NotifyOfPropertyChange("Restore");
            }
        }
    }
}