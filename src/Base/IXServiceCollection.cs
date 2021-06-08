//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
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
        /// Registered services
        /// </summary>
        IReadOnlyDictionary<Type, Func<object>> Services { get; }

        /// <summary>
        /// Adds new service or replaces existing one
        /// </summary>
        void AddOrReplace(Type svcType, Func<object> svcFactory);

        /// <summary>
        /// Creates service provider from this service
        /// </summary>
        /// <returns></returns>
        IServiceProvider CreateProvider();
    }

    /// <summary>
    /// Extension methods of <see cref="IXServiceCollection"/>
    /// </summary>
    public static class IXServiceCollectionExtension 
    {
        /// <inheritdoc cref="IXServiceCollection.AddOrReplace(Type, Func{object})"/>
        /// <param name="coll">Services collection</param>
        /// <typeparam name="TService">Service interface</typeparam>
        /// <typeparam name="TImplementation">Service implementation</typeparam>
        public static void AddOrReplace<TService, TImplementation>(this IXServiceCollection coll)
            where TImplementation : TService => AddOrReplace(coll, typeof(TService), typeof(TImplementation));

        /// <inheritdoc cref="AddOrReplace{TService, TImplementation}(IXServiceCollection)"/>
        /// <param name="factory">Service creation factory</param>
        public static void AddOrReplace<TService>(this IXServiceCollection coll, Func<TService> factory)
            => AddOrReplace(coll, typeof(TService), new Func<object>(() => factory.Invoke()));

        /// <inheritdoc cref="IXServiceCollection.AddOrReplace(Type, Func{object})"/>
        /// <param name="svcType">Type of service</param>
        /// <param name="impType">Service implementation</param>
        public static void AddOrReplace(this IXServiceCollection coll, Type svcType, Type impType)
            => AddOrReplace(coll, svcType, () => Activator.CreateInstance(impType));

        /// <inheritdoc cref="AddOrReplace(IXServiceCollection, Type, Type)"/>
        /// <param name="factory">Service creation factory</param>
        public static void AddOrReplace(this IXServiceCollection coll, Type svcType, Func<object> factory)
        {
            coll.AddOrReplace(svcType, factory);
        }

        /// <summary>
        /// Merges two service collections
        /// </summary>
        /// <param name="svc">Target service</param>
        /// <param name="other">Service to merge with (will override services if exist)</param>
        public static void Merge(this IXServiceCollection svc, IXServiceCollection other) 
        {
            foreach (var dep in other.Services) 
            {
                svc.AddOrReplace(dep.Key, dep.Value);
            }
        }
    }
}
