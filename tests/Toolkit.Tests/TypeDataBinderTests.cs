//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Xarial.XCad.Base;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.Binders;

namespace Toolkit.Tests
{
    public class TypeDataBinderTests
    {
        #region Mocks

        public class DataModelMock1
        {
            public string Field1 { get; set; }
            public int Field2 { get; set; }
            public double Field3 { get; set; }
        }

        public class DataModelMock2
        {
            public string Field1 { get; set; }
            public int Field2 { get; set; }
            public double Field3 { get; set; }

            public DataModelMock1 Group1 { get; set; }
        }

        public class DataModelMock3
        {
            public string Field1 { get; set; }

            public DataModelMock2 Group2 { get; set; }
        }

        #endregion

        [Test]
        public void TestBindSimple()
        {
            var binder = new TypeDataBinder(new Mock<IXLogger>().Object);
            IEnumerable<IBinding> bindings;

            IRawDependencyGroup dependencies;

            binder.Bind<DataModelMock1>(
                a =>
                {
                    return new Mock<IPage>().Object;
                },
                (Type t, IAttributeSet a, IGroup p, IMetadata[] md, out int r) =>
                {
                    r = 1;
                    return new Mock<IControl>().Object;
                }, x => null, out bindings, out dependencies, out _);

            var d1 = (bindings.ElementAt(0) as PropertyInfoBinding<DataModelMock1>).ControlDescriptor;
            var d2 = (bindings.ElementAt(1) as PropertyInfoBinding<DataModelMock1>).ControlDescriptor;
            var d3 = (bindings.ElementAt(2) as PropertyInfoBinding<DataModelMock1>).ControlDescriptor;

            Assert.AreEqual(3, bindings.Count());
            
            Assert.AreEqual("Field1", d1.Name);
            Assert.AreEqual(typeof(string), d1.DataType);

            Assert.AreEqual("Field2", d2.Name);
            Assert.AreEqual(typeof(int), d2.DataType);

            Assert.AreEqual("Field3", d3.Name);
            Assert.AreEqual(typeof(double), d3.DataType);
        }

        [Test]
        public void TestBindGroup()
        {
            var binder = new TypeDataBinder(new Mock<IXLogger>().Object);
            IEnumerable<IBinding> bindings;

            IRawDependencyGroup dependencies;

            binder.Bind<DataModelMock2>(
                a =>
                {
                    return new Mock<IPage>().Object;
                },
                (Type t, IAttributeSet a, IGroup p, IMetadata[] md, out int r) =>
                {
                    r = 1;
                    if (t == typeof(DataModelMock1))
                    {
                        return new Mock<IGroup>().Object;
                    }
                    else
                    {
                        return new Mock<IControl>().Object;
                    }
                }, x => null, out bindings, out dependencies, out _);

            var d1 = (bindings.ElementAt(0) as PropertyInfoBinding<DataModelMock2>).ControlDescriptor;
            var d2 = (bindings.ElementAt(1) as PropertyInfoBinding<DataModelMock2>).ControlDescriptor;
            var d3 = (bindings.ElementAt(2) as PropertyInfoBinding<DataModelMock2>).ControlDescriptor;
            var d4 = (bindings.ElementAt(3) as PropertyInfoBinding<DataModelMock2>).ControlDescriptor;
            var d5 = (bindings.ElementAt(4) as PropertyInfoBinding<DataModelMock2>).ControlDescriptor;
            var d6 = (bindings.ElementAt(5) as PropertyInfoBinding<DataModelMock2>).ControlDescriptor;
            var d7 = (bindings.ElementAt(6) as PropertyInfoBinding<DataModelMock2>).ControlDescriptor;

            Assert.AreEqual(7, bindings.Count());

            Assert.AreEqual("Field1", d1.Name);
            Assert.AreEqual(typeof(string), d1.DataType);

            Assert.AreEqual("Field2", d2.Name);
            Assert.AreEqual(typeof(int), d2.DataType);

            Assert.AreEqual("Field3", d3.Name);
            Assert.AreEqual(typeof(double), d3.DataType);

            Assert.AreEqual("Group1", d4.Name);
            Assert.AreEqual(typeof(DataModelMock1), d4.DataType);

            Assert.AreEqual("Field1", d5.Name);
            Assert.AreEqual(typeof(string), d5.DataType);

            Assert.AreEqual("Field2", d6.Name);
            Assert.AreEqual(typeof(int), d6.DataType);

            Assert.AreEqual("Field3", d7.Name);
            Assert.AreEqual(typeof(double), d7.DataType);
        }

        [Test]
        public void TestBindParent()
        {
            var binder = new TypeDataBinder(new Mock<IXLogger>().Object);
            IEnumerable<IBinding> bindings;

            IPage page = null;
            IGroup grp1 = null;
            IGroup grp2 = null;

            var parents = new Dictionary<IControl, IGroup>();

            IRawDependencyGroup dependencies;

            binder.Bind<DataModelMock3>(
                a =>
                {
                    page = new Moq.Mock<IPage>().Object;
                    return page;
                },
                (Type t, IAttributeSet a, IGroup p, IMetadata[] md, out int r) =>
                {
                    r = 1;
                    if (t == typeof(DataModelMock1))
                    {
                        grp1 = new Mock<IGroup>().Object;
                        parents.Add(grp1, p);
                        return grp1;
                    }
                    if (t == typeof(DataModelMock2))
                    {
                        grp2 = new Mock<IGroup>().Object;
                        parents.Add(grp2, p);
                        return grp2;
                    }
                    else
                    {
                        var ctrl = new Mock<IControl>().Object;
                        parents.Add(ctrl, p);
                        return ctrl;
                    }
                }, x => null, out bindings, out dependencies, out _);

            Assert.AreEqual(page,
                parents[(bindings.ElementAt(0) as PropertyInfoBinding<DataModelMock3>).Control]);
            Assert.AreEqual(page,
                parents[(bindings.ElementAt(1) as PropertyInfoBinding<DataModelMock3>).Control]);

            Assert.AreEqual(grp2,
                parents[(bindings.ElementAt(2) as PropertyInfoBinding<DataModelMock3>).Control]);
            Assert.AreEqual(grp2,
                parents[(bindings.ElementAt(3) as PropertyInfoBinding<DataModelMock3>).Control]);
            Assert.AreEqual(grp2,
                parents[(bindings.ElementAt(4) as PropertyInfoBinding<DataModelMock3>).Control]);
            Assert.AreEqual(grp2,
                parents[(bindings.ElementAt(5) as PropertyInfoBinding<DataModelMock3>).Control]);

            Assert.AreEqual(grp1,
                parents[(bindings.ElementAt(6) as PropertyInfoBinding<DataModelMock3>).Control]);
            Assert.AreEqual(grp1,
                parents[(bindings.ElementAt(7) as PropertyInfoBinding<DataModelMock3>).Control]);
            Assert.AreEqual(grp1,
                parents[(bindings.ElementAt(8) as PropertyInfoBinding<DataModelMock3>).Control]);
        }

        [Test]
        public void TestBindIds()
        {
            var binder = new TypeDataBinder(new Mock<IXLogger>().Object);
            IEnumerable<IBinding> bindings;

            IPage page = null;
            
            IRawDependencyGroup dependencies;

            binder.Bind<DataModelMock1>(
                a =>
                {
                    page = new Moq.Mock<IPage>().Object;
                    return page;
                },
                (Type t, IAttributeSet a, IGroup p, IMetadata[] md, out int r) =>
                {
                    r = 1;
                    var ctrlMock = new Mock<IControl>();
                    ctrlMock.SetupGet(c => c.Id).Returns(() => a.Id);
                    return ctrlMock.Object;
                }, x => null, out bindings, out dependencies, out _);

            Assert.AreEqual(0, bindings.ElementAt(0).Control.Id);
            Assert.AreEqual(1, bindings.ElementAt(1).Control.Id);
            Assert.AreEqual(2, bindings.ElementAt(2).Control.Id);
        }

        [Test]
        public void TestBindAttributes()
        {
        }

        [Test]
        public void TestDependencies()
        {
        }
    }
}
