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

#if NETFX_CORE && !WinRT
#define WinRT
#endif

namespace Caliburn.Micro {
    using System;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Extension methods for the <see cref="SimpleContainer"/>.
    /// </summary>
    public static class ContainerExtensions {
        /// <summary>
        /// Registers a singleton.
        /// </summary>
        /// <typeparam name="TImplementation">The type of the implementation.</typeparam>
        /// <param name="container">The container.</param>
        /// <returns>The container.</returns>
        public static SimpleContainer Singleton<TImplementation>(this SimpleContainer container) {
            container.RegisterSingleton(typeof(TImplementation), null, typeof(TImplementation));
            return container;
        }

        /// <summary>
        /// Registers a singleton.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <typeparam name="TImplementation">The type of the implementation.</typeparam>
        /// <param name="container">The container.</param>
        /// <returns>The container.</returns>
        public static SimpleContainer Singleton<TService, TImplementation>(this SimpleContainer container)
            where TImplementation : TService {
            container.RegisterSingleton(typeof(TService), null, typeof(TImplementation));
            return container;
        }

        /// <summary>
        /// Registers an service to be created on each request.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <typeparam name="TImplementation">The type of the implementation.</typeparam>
        /// <param name="container">The container.</param>
        /// <returns>The container.</returns>
        public static SimpleContainer PerRequest<TService, TImplementation>(this SimpleContainer container)
            where TImplementation : TService {
            container.RegisterPerRequest(typeof(TService), null, typeof(TImplementation));
            return container;
        }

        /// <summary>
        /// Registers an service to be created on each request.
        /// </summary>
        /// <typeparam name="TImplementation">The type of the implementation.</typeparam>
        /// <param name="container">The container.</param>
        /// <returns>The container.</returns>
        public static SimpleContainer PerRequest<TImplementation>(this SimpleContainer container) {
            container.RegisterPerRequest(typeof(TImplementation), null, typeof(TImplementation));
            return container;
        }

        /// <summary>
        /// Registers an instance with the container.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="instance">The instance.</param>
        /// <returns>The container.</returns>
        public static SimpleContainer Instance<TService>(this SimpleContainer container, TService instance) {
            container.RegisterInstance(typeof(TService), null, instance);
            return container;
        }

        /// <summary>
        /// Registers a custom service handler with the container.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="handler">The handler.</param>
        /// <returns>The container.</returns>
        public static SimpleContainer Handler<TService>(this SimpleContainer container, Func<SimpleContainer, object> handler) {
            container.RegisterHandler(typeof(TService), null, handler);
            return container;
        }

        /// <summary>
        /// Registers all specified types in an assembly as singleton in the container.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="assembly">The assembly.</param>
        /// <param name="filter">The type filter.</param>
        /// <returns>The container.</returns>
        public static SimpleContainer AllTypesOf<TService>(this SimpleContainer container, Assembly assembly, Func<Type, bool> filter = null) {
            if(filter == null)
                filter = type => true;

#if WinRT
            var serviceInfo = typeof(TService).GetTypeInfo();
            var types = from info in assembly.DefinedTypes
                        let type = info.AsType()
                        where serviceInfo.IsAssignableFrom(info)
                              && !info.IsAbstract
                              && !info.IsInterface
                              && filter(type)
                        select type;
#else
            var serviceType = typeof(TService);
            var types = from type in assembly.GetTypes()
                        where serviceType.IsAssignableFrom(type)
                              && !type.IsAbstract
                              && !type.IsInterface
                              && filter(type)
                        select type;
#endif

            foreach (var type in types) {
                container.RegisterSingleton(typeof(TService), null, type);
            }

            return container;
        }
    }
}
