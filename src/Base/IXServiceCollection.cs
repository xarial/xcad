//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;

namespace Xarial.XCad
{
    /// <summary>
    /// Collection of services
    /// </summary>
    public interface IXServiceCollection 
    {
        /// <summary>
        /// Adds new service or replaces existing one
        /// </summary>
        /// <param name="svcType">Service type</param>
        /// <param name="svcFactory">Service factory</param>
        /// <param name="replace">True to replace if service is registered, False if not</param>
        void Add(Type svcType, Func<object> svcFactory, bool replace = true);

        /// <summary>
        /// Creates service provider from this service
        /// </summary>
        /// <returns></returns>
        IServiceProvider CreateProvider();

        /// <summary>
        /// Creates a clone of these services
        /// </summary>
        /// <returns></returns>
        IXServiceCollection Clone();
    }

    /// <summary>
    /// Extension methods of <see cref="IXServiceCollection"/>
    /// </summary>
    public static class XServiceCollectionExtension 
    {
        /// <inheritdoc cref="IXServiceCollection.Add(Type, Func{object}, bool)"/>
        /// <param name="svcColl">Services collection</param>
        /// <typeparam name="TService">Service interface</typeparam>
        /// <typeparam name="TImplementation">Service implementation</typeparam>
        public static void Add<TService, TImplementation>(this IXServiceCollection svcColl, bool replace = true)
            where TImplementation : TService => Add(svcColl, typeof(TService), typeof(TImplementation), replace);

        /// <inheritdoc cref="Add{TService, TImplementation}(IXServiceCollection, bool)"/>
        /// <param name="factory">Service creation factory</param>
        public static void Add<TService>(this IXServiceCollection svcColl, Func<TService> factory, bool replace = true)
            => svcColl.Add(typeof(TService), new Func<object>(() => factory.Invoke()), replace);

        /// <inheritdoc cref="IXServiceCollection.Add(Type, Func{object}, bool)"/>
        /// <param name="svcType">Type of service</param>
        /// <param name="impType">Service implementation</param>
        public static void Add(this IXServiceCollection svcColl, Type svcType, Type impType, bool replace = true)
            => svcColl.Add(svcType, () => Activator.CreateInstance(impType), replace);
    }
}
