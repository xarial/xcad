using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad;
using Xarial.XCad.Toolkit;
using Xarial.XCad.Toolkit.Exceptions;

namespace Toolkit.Tests
{
    public class ServiceCollectionTests
    {
        public interface I1 
        {
        }

        public class C1_1 : I1
        {
        }

        public class C1_2 : I1 
        {
        }

        public interface I2
        {
        }

        public class C2_1 : I2
        {
        }

        public class C2_2 : I2
        {
        }

        public interface I3 
        {
        }

        public class C3 : I3, IDisposable
        {
            public bool IsDisposed { get; private set; }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }

        public interface I4
        {
        }

        public class C4 : I4, IDisposable
        {
            public bool IsDisposed { get; private set; }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }

        [Test]
        public void ReplaceTest() 
        {
            var svcColl = new ServiceCollection();

            svcColl.Add<I1, C1_1>(ServiceLifetimeScope_e.Transient, true);
            svcColl.Add<I1, C1_2>(ServiceLifetimeScope_e.Transient, true);
            svcColl.Add<I2, C2_1>(ServiceLifetimeScope_e.Transient, false);
            svcColl.Add<I2, C2_2>(ServiceLifetimeScope_e.Transient, false);

            var svcProv = svcColl.CreateProvider();

            var s1 = svcProv.GetService<I1>();
            var s2 = svcProv.GetService<I2>();

            Assert.IsAssignableFrom<C1_2>(s1);
            Assert.IsAssignableFrom<C2_1>(s2);
        }

        [Test]
        public void LifetimeTest()
        {
            var svcColl = new ServiceCollection();

            svcColl.Add<I1, C1_1>(ServiceLifetimeScope_e.Transient);
            svcColl.Add<I2, C2_1>(ServiceLifetimeScope_e.Singleton);

            var svcProv = svcColl.CreateProvider();

            var s1_1 = svcProv.GetService<I1>();
            var s1_2 = svcProv.GetService<I1>();
            var s2_1 = svcProv.GetService<I2>();
            var s2_2 = svcProv.GetService<I2>();

            Assert.AreNotEqual(s1_1, s1_2);
            Assert.AreEqual(s2_1, s2_2);
        }

        [Test]
        public void DisposeTest()
        {
            var svcColl = new ServiceCollection();

            svcColl.Add<I2, C2_1>(ServiceLifetimeScope_e.Singleton);
            svcColl.Add<I3, C3>(ServiceLifetimeScope_e.Singleton);
            svcColl.Add<I4, C4>(ServiceLifetimeScope_e.Transient);

            var svcProv = svcColl.CreateProvider();

            var s0 = svcProv.GetService<I2>();
            var s1 = (C3)svcProv.GetService<I3>();
            var s2 = (C4)svcProv.GetService<I4>();

            ((IDisposable)svcProv).Dispose();

            Assert.That(s1.IsDisposed == true);
            Assert.That(s2.IsDisposed == false);
        }

        [Test]
        public void NotRegisteredServiceTest() 
        {
            var svcColl = new ServiceCollection();

            svcColl.Add<I3, C3>(ServiceLifetimeScope_e.Singleton);

            var svcProv = svcColl.CreateProvider();

            var s1 = svcProv.GetService<I3>();

            Assert.Throws<ServiceNotRegisteredException>(() => svcProv.GetService<I4>());
        }
    }
}
