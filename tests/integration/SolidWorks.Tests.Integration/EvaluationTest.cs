using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.SolidWorks.Geometry.Exceptions;
using Xarial.XCad.Geometry.Exceptions;

namespace SolidWorks.Tests.Integration
{
    public class EvaluationTest : IntegrationTests
    {
        [Test]
        public void BodyVolumeTest()
        {
            double v1;

            using (var doc = OpenDataDocument("Features1.SLDPRT"))
            {
                var part = (IXPart)m_App.Documents.Active;

                v1 = ((IXSolidBody)part.Bodies["Boss-Extrude2"]).Volume;
            }

            Assert.That(2.3851693679806192E-05, Is.EqualTo(v1).Within(0.001).Percent);
        }

        [Test]
        public void BoundingBoxUserUnitTest()
        {
            Box3D b1;
            Box3D b2;

            using (var doc = OpenDataDocument("BBox2.SLDPRT"))
            {
                var part = (IXPart)m_App.Documents.Active;

                var bbox = part.PreCreateBoundingBox();
                bbox.Precise = true;
                bbox.UserUnits = true;
                bbox.Commit();
                b1 = bbox.Box;

                bbox = part.PreCreateBoundingBox();
                bbox.Precise = false;
                bbox.UserUnits = true;
                bbox.Commit();
                b2 = bbox.Box;
            }

            Assert.That(b1.Width, Is.EqualTo(3.0).Within(0.00000000001).Percent);
            Assert.That(b1.Height, Is.EqualTo(1.5).Within(0.00000000001).Percent);
            Assert.That(b1.Length, Is.EqualTo(2.0).Within(0.00000000001).Percent);
            Assert.That(b1.AxisX.X, Is.EqualTo(1).Within(0.00000000001).Percent);
            Assert.That(b1.AxisX.Y, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b1.AxisX.Z, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b1.AxisY.X, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b1.AxisY.Y, Is.EqualTo(1).Within(0.00000000001).Percent);
            Assert.That(b1.AxisY.Z, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b1.AxisZ.X, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b1.AxisZ.Y, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b1.AxisZ.Z, Is.EqualTo(1).Within(0.00000000001).Percent);
            Assert.That(b1.CenterPoint.X, Is.EqualTo(2.5).Within(0.00000000001).Percent);
            Assert.That(b1.CenterPoint.Y, Is.EqualTo(2.75).Within(0.00000000001).Percent);
            Assert.That(b1.CenterPoint.Z, Is.EqualTo(1.0).Within(0.00000000001).Percent);

            Assert.That(b2.Width, Is.EqualTo(3.0).Within(10).Percent);
            Assert.That(b2.Height, Is.EqualTo(1.5).Within(30).Percent);
            Assert.That(b2.Length, Is.EqualTo(2.0).Within(10).Percent);
            Assert.That(b2.AxisX.X, Is.EqualTo(1).Within(0.00000000001).Percent);
            Assert.That(b2.AxisX.Y, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b2.AxisX.Z, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b2.AxisY.X, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b2.AxisY.Y, Is.EqualTo(1).Within(0.00000000001).Percent);
            Assert.That(b2.AxisY.Z, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b2.AxisZ.X, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b2.AxisZ.Y, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b2.AxisZ.Z, Is.EqualTo(1).Within(0.00000000001).Percent);
            Assert.That(b2.CenterPoint.X, Is.EqualTo(2.5).Within(10).Percent);
            Assert.That(b2.CenterPoint.Y, Is.EqualTo(2.75).Within(30).Percent);
            Assert.That(b2.CenterPoint.Z, Is.EqualTo(1.0).Within(10).Percent);
        }

        [Test]
        public void BoundingBoxPartScopedPreceiseAndApproximateTest()
        {
            Box3D b1;
            Box3D b2;

            using (var doc = OpenDataDocument("BBox1.SLDPRT"))
            {
                var part = (IXPart)m_App.Documents.Active;

                var body = (IXSolidBody)part.Bodies["Boss-Extrude1"];

                var bbox = part.PreCreateBoundingBox();
                bbox.Precise = true;
                bbox.Scope = new IXBody[] { body };
                bbox.Commit();
                b1 = bbox.Box;

                bbox = part.PreCreateBoundingBox();
                bbox.Precise = false;
                bbox.Scope = new IXBody[] { body };
                bbox.Commit();
                b2 = bbox.Box;
            }

            Assert.That(b1.Width, Is.EqualTo(0.1).Within(0.00000000001).Percent);
            Assert.That(b1.Height, Is.EqualTo(0.05).Within(0.00000000001).Percent);
            Assert.That(b1.Length, Is.EqualTo(0.15).Within(0.00000000001).Percent);
            Assert.That(b1.AxisX.X, Is.EqualTo(1).Within(0.00000000001).Percent);
            Assert.That(b1.AxisX.Y, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b1.AxisX.Z, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b1.AxisY.X, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b1.AxisY.Y, Is.EqualTo(1).Within(0.00000000001).Percent);
            Assert.That(b1.AxisY.Z, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b1.AxisZ.X, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b1.AxisZ.Y, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b1.AxisZ.Z, Is.EqualTo(1).Within(0.00000000001).Percent);
            Assert.That(b1.CenterPoint.X, Is.EqualTo(0.05).Within(0.00000000001).Percent);
            Assert.That(b1.CenterPoint.Y, Is.EqualTo(0.025).Within(0.00000000001).Percent);
            Assert.That(b1.CenterPoint.Z, Is.EqualTo(-0.075).Within(0.00000000001).Percent);

            Assert.That(b2.Width, Is.EqualTo(0.1).Within(0.00000000001).Percent);
            Assert.That(b2.Height, Is.EqualTo(0.05).Within(0.00000000001).Percent);
            Assert.That(b2.Length, Is.EqualTo(0.15).Within(0.00000000001).Percent);
            Assert.That(b2.AxisX.X, Is.EqualTo(1).Within(0.00000000001).Percent);
            Assert.That(b2.AxisX.Y, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b2.AxisX.Z, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b2.AxisY.X, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b2.AxisY.Y, Is.EqualTo(1).Within(0.00000000001).Percent);
            Assert.That(b2.AxisY.Z, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b2.AxisZ.X, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b2.AxisZ.Y, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b2.AxisZ.Z, Is.EqualTo(1).Within(0.00000000001).Percent);
            Assert.That(b2.CenterPoint.X, Is.EqualTo(0.05).Within(0.00000000001).Percent);
            Assert.That(b2.CenterPoint.Y, Is.EqualTo(0.025).Within(0.00000000001).Percent);
            Assert.That(b2.CenterPoint.Z, Is.EqualTo(-0.075).Within(0.00000000001).Percent);
        }

        [Test]
        public void BoundingBoxRelativePartScopedTest()
        {
            Box3D b1;
            Exception b2 = null;

            using (var doc = OpenDataDocument("BBox1.SLDPRT"))
            {
                var part = (ISwPart)m_App.Documents.Active;

                var body = (IXSolidBody)part.Bodies["Body-Move/Copy1"];

                var matrix = TransformConverter.ToTransformMatrix(
                    part.Model.Extension.GetCoordinateSystemTransformByName("Coordinate System1"));

                var bbox = part.PreCreateBoundingBox();
                bbox.Precise = true;
                bbox.RelativeTo = matrix;
                bbox.Scope = new IXBody[] { body };
                bbox.Commit();
                b1 = bbox.Box;

                bbox = part.PreCreateBoundingBox();
                bbox.Precise = false;
                bbox.RelativeTo = matrix;
                bbox.Scope = new IXBody[] { body };
                try
                {
                    bbox.Commit();
                }
                catch (Exception ex)
                {
                    b2 = ex;
                }
            }

            Assert.That(b1.Width, Is.EqualTo(0.1).Within(0.00000000001).Percent);
            Assert.That(b1.Height, Is.EqualTo(0.05).Within(0.00000000001).Percent);
            Assert.That(b1.Length, Is.EqualTo(0.15).Within(0.00000000001).Percent);

            var normX = b1.AxisX.Normalize();
            var expNormX = new Vector(-0.04748737 - -0.09748737, -0.02248737 - -0.07248737, 0.0517767 - 0.12248737).Normalize();
            var normY = b1.AxisY.Normalize();
            var expNormY = new Vector(-0.1048097 - -0.09748737, -0.0298097 - -0.07248737, 0.14748737 - 0.12248737).Normalize();
            var normZ = b1.AxisZ.Normalize();
            var expNormZ = new Vector(0.03054564 - -0.09748737, -0.09445436 - -0.07248737, 0.19748737 - 0.12248737).Normalize();

            AssertCompareDoubles(normX.X, expNormX.X, 5);
            AssertCompareDoubles(normX.Y, expNormX.Y, 5);
            AssertCompareDoubles(normX.Z, expNormX.Z, 5);

            AssertCompareDoubles(normY.X, expNormY.X, 5);
            AssertCompareDoubles(normY.Y, expNormY.Y, 5);
            AssertCompareDoubles(normY.Z, expNormY.Z, 5);

            AssertCompareDoubles(normZ.X, expNormZ.X, 5);
            AssertCompareDoubles(normZ.Y, expNormZ.Y, 5);
            AssertCompareDoubles(normZ.Z, expNormZ.Z, 5);

            AssertCompareDoubles(b1.CenterPoint.X, -0.01213203);
            AssertCompareDoubles(b1.CenterPoint.Y, -0.03713203);
            AssertCompareDoubles(b1.CenterPoint.Z, 0.13713203);

            Assert.IsAssignableFrom<NotSupportedException>(b2);
        }

        [Test]
        public void BoundingBoxPartApproximateFullTest()
        {
            Box3D b1;

            using (var doc = OpenDataDocument("BBox1.SLDPRT"))
            {
                var part = (IXPart)m_App.Documents.Active;

                var bbox = part.PreCreateBoundingBox();
                bbox.Precise = false;
                bbox.Commit();
                b1 = bbox.Box;
            }

            AssertCompareDoubles(b1.Width, 0.2048097, 7);
            AssertCompareDoubles(b1.Height, 0.14445436, 7);
            AssertCompareDoubles(b1.Length, 0.37248737, 7);
            Assert.That(b1.AxisX.X, Is.EqualTo(1).Within(0.00000000001).Percent);
            Assert.That(b1.AxisX.Y, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b1.AxisX.Z, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b1.AxisY.X, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b1.AxisY.Y, Is.EqualTo(1).Within(0.00000000001).Percent);
            Assert.That(b1.AxisY.Z, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b1.AxisZ.X, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b1.AxisZ.Y, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b1.AxisZ.Z, Is.EqualTo(1).Within(0.00000000001).Percent);
            AssertCompareDoubles(b1.CenterPoint.X, -0.00240485);
            AssertCompareDoubles(b1.CenterPoint.Y, -0.02222718);
            AssertCompareDoubles(b1.CenterPoint.Z, 0.03624369);
        }

        [Test]
        public void BoundingBoxAssemblyScopedPreciseTest()
        {
            Box3D b1;

            using (var doc = OpenDataDocument(@"BBoxAssembly1\Assem1.SLDASM"))
            {
                var assm = (IXAssembly)m_App.Documents.Active;

                var bbox = assm.PreCreateBoundingBox();
                bbox.Precise = true;
                bbox.Scope = new IXComponent[]
                {
                    assm.Configurations.Active.Components["Part1-1"],
                    assm.Configurations.Active.Components["Part1-2"],
                    assm.Configurations.Active.Components["SubAssem1-2"].Children["SubSubAssem1-1"].Children["Part1-1"]
                };
                bbox.Commit();
                b1 = bbox.Box;
            }

            AssertCompareDoubles(b1.Width, 0.75545085, 5);
            AssertCompareDoubles(b1.Height, 0.17649638, 5);
            AssertCompareDoubles(b1.Length, 0.54968753, 5);
            Assert.That(b1.AxisX.X, Is.EqualTo(1).Within(0.00000000001).Percent);
            Assert.That(b1.AxisX.Y, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b1.AxisX.Z, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b1.AxisY.X, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b1.AxisY.Y, Is.EqualTo(1).Within(0.00000000001).Percent);
            Assert.That(b1.AxisY.Z, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b1.AxisZ.X, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b1.AxisZ.Y, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b1.AxisZ.Z, Is.EqualTo(1).Within(0.00000000001).Percent);
            AssertCompareDoubles(b1.CenterPoint.X, 0.20224695, 5);
            AssertCompareDoubles(b1.CenterPoint.Y, 0.06324819, 5);
            AssertCompareDoubles(b1.CenterPoint.Z, 0.17320367, 5);
        }

        [Test]
        public void BoundingBoxRelativeAssemblyScopedTest()
        {
            Box3D b1;
            Exception b2 = null;

            using (var doc = OpenDataDocument(@"BBoxAssembly4\Assem1.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                var comps = assm.Configurations.Active.Components.ToArray();

                var matrix = TransformConverter.ToTransformMatrix(
                    assm.Model.Extension.GetCoordinateSystemTransformByName("Coordinate System1"));

                var bbox = assm.PreCreateBoundingBox();
                bbox.Precise = true;
                bbox.RelativeTo = matrix;
                bbox.Scope = comps;
                bbox.Commit();
                b1 = bbox.Box;

                bbox = assm.PreCreateBoundingBox();
                bbox.Precise = false;
                bbox.RelativeTo = matrix;
                bbox.Scope = comps;
                try
                {
                    bbox.Commit();
                }
                catch (Exception ex)
                {
                    b2 = ex;
                }
            }

            AssertCompareDoubles(b1.Width, 0.06);
            AssertCompareDoubles(b1.Height, 0.05);
            AssertCompareDoubles(b1.Length, 0.075);

            var normX = b1.AxisX.Normalize();
            var expNormX = new Vector(-0.06468049 - -0.01782764, 0.16547951 - 0.13826241, 0.3872723 - 0.36150332).Normalize();
            var normY = b1.AxisY.Normalize();
            var expNormY = new Vector(-0.01717473 - -0.01782764, 0.1732238 - 0.13826241, 0.32576434 - 0.36150332).Normalize();
            var normZ = b1.AxisZ.Normalize();
            var expNormZ = new Vector(-0.06466841 - -0.01782764, 0.0968212 - 0.13826241, 0.32010804 - 0.36150332).Normalize();

            AssertCompareDoubles(normX.X, expNormX.X, 5);
            AssertCompareDoubles(normX.Y, expNormX.Y, 5);
            AssertCompareDoubles(normX.Z, expNormX.Z, 5);

            AssertCompareDoubles(normY.X, expNormY.X, 5);
            AssertCompareDoubles(normY.Y, expNormY.Y, 5);
            AssertCompareDoubles(normY.Z, expNormY.Z, 5);

            AssertCompareDoubles(normZ.X, expNormZ.X, 5);
            AssertCompareDoubles(normZ.Y, expNormZ.Y, 5);
            AssertCompareDoubles(normZ.Z, expNormZ.Z, 5);

            AssertCompareDoubles(b1.CenterPoint.X, -0.06434799);
            AssertCompareDoubles(b1.CenterPoint.Y, 0.14863105);
            AssertCompareDoubles(b1.CenterPoint.Z, 0.33582068);

            Assert.IsAssignableFrom<NotSupportedException>(b2);
        }

        [Test]
        public void BoundingBoxAssemblyFullTest()
        {
            Box3D b1;
            Exception b2 = null;

            using (var doc = OpenDataDocument(@"BBoxAssembly2\Assem1.SLDASM"))
            {
                var assm = (IXAssembly)m_App.Documents.Active;

                var bbox = assm.PreCreateBoundingBox();
                bbox.Precise = false;
                bbox.Commit();
                b1 = bbox.Box;

                try
                {
                    bbox = assm.PreCreateBoundingBox();
                    bbox.VisibleOnly = false;
                    bbox.Commit();
                }
                catch (Exception ex)
                {
                    b2 = ex;
                }
            }

            AssertCompareDoubles(b1.Width, 0.2, 7);
            AssertCompareDoubles(b1.Height, 0.05, 7);
            AssertCompareDoubles(b1.Length, 0.01, 7);
            Assert.That(b1.AxisX.X, Is.EqualTo(1).Within(0.00000000001).Percent);
            Assert.That(b1.AxisX.Y, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b1.AxisX.Z, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b1.AxisY.X, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b1.AxisY.Y, Is.EqualTo(1).Within(0.00000000001).Percent);
            Assert.That(b1.AxisY.Z, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b1.AxisZ.X, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b1.AxisZ.Y, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b1.AxisZ.Z, Is.EqualTo(1).Within(0.00000000001).Percent);
            AssertCompareDoubles(b1.CenterPoint.X, 0);
            AssertCompareDoubles(b1.CenterPoint.Y, 0.025);
            AssertCompareDoubles(b1.CenterPoint.Z, 0.005);

            Assert.IsAssignableFrom<NotSupportedException>(b2);
        }

        [Test]
        public void BoundingBoxAssemblyScopedApproximateFullTest()
        {
            Box3D b1;
            Exception b2 = null;

            using (var doc = OpenDataDocument(@"BBoxAssembly3\Assem1.SLDASM"))
            {
                var assm = (IXAssembly)m_App.Documents.Active;

                var comps = new IXComponent[]
                {
                    assm.Configurations.Active.Components["SubAssem1-1"],
                    assm.Configurations.Active.Components["SubAssem1-2"]
                };

                var bbox = assm.PreCreateBoundingBox();
                bbox.Scope = comps;
                bbox.Precise = false;
                bbox.Commit();
                b1 = bbox.Box;

                try
                {
                    bbox = assm.PreCreateBoundingBox();
                    bbox.VisibleOnly = false;
                    bbox.Scope = comps;
                    bbox.Commit();
                }
                catch (Exception ex)
                {
                    b2 = ex;
                }
            }

            AssertCompareDoubles(b1.Width, 0.2, 7);
            AssertCompareDoubles(b1.Height, 0.05, 7);
            AssertCompareDoubles(b1.Length, 0.07, 7);
            Assert.That(b1.AxisX.X, Is.EqualTo(1).Within(0.00000000001).Percent);
            Assert.That(b1.AxisX.Y, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b1.AxisX.Z, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b1.AxisY.X, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b1.AxisY.Y, Is.EqualTo(1).Within(0.00000000001).Percent);
            Assert.That(b1.AxisY.Z, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b1.AxisZ.X, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b1.AxisZ.Y, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b1.AxisZ.Z, Is.EqualTo(1).Within(0.00000000001).Percent);
            AssertCompareDoubles(b1.CenterPoint.X, 0);
            AssertCompareDoubles(b1.CenterPoint.Y, 0.025);
            AssertCompareDoubles(b1.CenterPoint.Z, 0.035);

            Assert.IsAssignableFrom<NotSupportedException>(b2);
        }

        [Test]
        public void BoundingBoxAssemblyCustomBodyTest()
        {
            Box3D b1;

            using (var doc = OpenDataDocument(@"BBoxAssembly1\Assem1.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                var bbox = assm.PreCreateBoundingBox();
                bbox.Precise = true;
                var swBody = (assm.Configurations.Active.Components["SubAssem1-2"].Children["SubSubAssem1-1"].Children["Part1-1"]
                    .Bodies.First() as ISwBody).Body.ICopy();
                (bbox as IXBoundingBox).Scope = new IXBody[]
                {
                    assm.CreateObjectFromDispatch<ISwBody>(swBody)
                };
                bbox.Commit();
                b1 = bbox.Box;
            }

            AssertCompareDoubles(b1.Width, 0.15);
            AssertCompareDoubles(b1.Height, 0.05);
            AssertCompareDoubles(b1.Length, 0.075);
            Assert.That(b1.AxisX.X, Is.EqualTo(1).Within(0.00000000001).Percent);
            Assert.That(b1.AxisX.Y, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b1.AxisX.Z, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b1.AxisY.X, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b1.AxisY.Y, Is.EqualTo(1).Within(0.00000000001).Percent);
            Assert.That(b1.AxisY.Z, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b1.AxisZ.X, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b1.AxisZ.Y, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(b1.AxisZ.Z, Is.EqualTo(1).Within(0.00000000001).Percent);
            AssertCompareDoubles(b1.CenterPoint.X, 0, 5);
            AssertCompareDoubles(b1.CenterPoint.Y, 0, 5);
            AssertCompareDoubles(b1.CenterPoint.Z, 0.0375, 5);
        }

        [Test]
        public void BoundingBoxEmptyComponentsTest()
        {
            using (var doc = OpenDataDocument(@"MassPrpsAssembly2\Assem1.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                using (var bbox = assm.PreCreateBoundingBox())
                {
                    bbox.UserUnits = false;
                    bbox.VisibleOnly = false;
                    bbox.Precise = true;

                    bbox.Scope = new IXComponent[] { assm.Configurations.Active.Components["Empty-1"] };
                    Assert.Throws<EvaluationFailedException>(() => bbox.Commit());
                    Assert.DoesNotThrow(() =>
                    {
                        bbox.Scope = new IXComponent[] { assm.Configurations.Active.Components["Part1-1"] };
                        bbox.Commit();
                    });
                    Assert.Throws<EvaluationFailedException>(() => bbox.Scope = new IXComponent[] { assm.Configurations.Active.Components["Sketch-1"] });
                    Assert.DoesNotThrow(() => bbox.Scope = new IXComponent[] { assm.Configurations.Active.Components["Part1-1"] });
                    Assert.DoesNotThrow(() => bbox.Scope = new IXComponent[] { assm.Configurations.Active.Components["Surface-1"] });
                    Assert.DoesNotThrow(() => bbox.Scope = new IXComponent[] { assm.Configurations.Active.Components["SubAssem1-1"] });
                    Assert.DoesNotThrow(() => bbox.Scope = new IXComponent[] { assm.Configurations.Active.Components["Part2-1"] });
                }
            }
        }

        [Test]
        public void BoundingBoxEmptyAssemblyTest()
        {
            using (var doc = NewDocument(Interop.swconst.swDocumentTypes_e.swDocASSEMBLY))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                using (var bbox = assm.PreCreateBoundingBox())
                {
                    bbox.UserUnits = false;
                    bbox.VisibleOnly = true;
                    bbox.Precise = false;

                    bbox.Scope = null;
                    Assert.Throws<EvaluationFailedException>(() => bbox.Commit());
                    Assert.Throws<EvaluationFailedException>(() =>
                    {
                        bbox.Scope = new IXComponent[0];
                        bbox.Commit();
                    });
                }
            }
        }

        [Test]
        public void BoundingBoxEmptyPartTest()
        {
            using (var doc = OpenDataDocument(@"MassPrpsAssembly2\Sketch.sldprt"))
            {
                var part = (ISwPart)m_App.Documents.Active;

                using (var bbox = part.PreCreateBoundingBox())
                {
                    bbox.UserUnits = false;
                    bbox.VisibleOnly = false;
                    bbox.Precise = true;

                    bbox.Scope = null;
                    Assert.Throws<EvaluationFailedException>(() => bbox.Commit());
                    Assert.Throws<EvaluationFailedException>(() =>
                    {
                        bbox.Scope = new IXBody[0];
                        bbox.Commit();
                    });
                }
            }
        }

        [Test]
        public void MassPropertyPartScopedBodyTest()
        {
            double density;
            Point cog;
            double mass;
            MomentOfInertia moi;
            PrincipalAxesOfInertia pai;
            PrincipalMomentOfInertia pmoi;
            double area;
            double volume;

            using (var doc = OpenDataDocument(@"MassPrpPart1.SLDPRT"))
            {
                var part = (ISwPart)m_App.Documents.Active;
                
                using (var massPrps = part.PreCreateMassProperty())
                {
                    massPrps.Scope = new IXBody[] { part.Bodies["Sweep1"] };
                    massPrps.UserUnits = false;
                    massPrps.Commit();

                    density = massPrps.Density;
                    cog = massPrps.CenterOfGravity;
                    mass = massPrps.Mass;
                    moi = massPrps.MomentOfInertia;
                    pai = massPrps.PrincipalAxesOfInertia;
                    pmoi = massPrps.PrincipalMomentOfInertia;
                    area = massPrps.SurfaceArea;
                    volume = massPrps.Volume;
                }
            }

            AssertCompareDoubles(density, 1000);
            AssertCompareDoubles(cog.X, -0.19035882);
            AssertCompareDoubles(cog.Y, 0.05413023);
            AssertCompareDoubles(cog.Z, 0.00000000);
            AssertCompareDoubles(mass, 0.50052601);
            AssertCompareDoubles(moi.Lx.X, 0.00147253);
            AssertCompareDoubles(moi.Lx.Y, 0.00131611);
            AssertCompareDoubles(moi.Lx.Z, 0.00000000);
            AssertCompareDoubles(moi.Ly.X, 0.00131611);
            AssertCompareDoubles(moi.Ly.Y, 0.00147253);
            AssertCompareDoubles(moi.Ly.Z, 0.00000000);
            AssertCompareDoubles(moi.Lz.X, 0.00000000);
            AssertCompareDoubles(moi.Lz.Y, 0.00000000);
            AssertCompareDoubles(moi.Lz.Z, 0.00278864);
            AssertCompareDoubles(pai.Ix.X, 0.70710678);
            AssertCompareDoubles(pai.Ix.Y, 0.70710678);
            AssertCompareDoubles(pai.Ix.Z, 0.00000000);
            AssertCompareDoubles(pai.Iy.X, -0.70710678);
            AssertCompareDoubles(pai.Iy.Y, 0.70710678);
            AssertCompareDoubles(pai.Iy.Z, 0.00000000);
            AssertCompareDoubles(pai.Iz.X, 0.00000000);
            AssertCompareDoubles(pai.Iz.Y, 0.00000000);
            AssertCompareDoubles(pai.Iz.Z, 1.00000000);
            AssertCompareDoubles(pmoi.Px, 0.00015641);
            AssertCompareDoubles(pmoi.Py, 0.00278864);
            AssertCompareDoubles(pmoi.Pz, 0.00278864);
            AssertCompareDoubles(area, 0.04396907);
            AssertCompareDoubles(volume, 0.00050053);
        }

        [Test]
        public void MassPropertyPartFullTest()
        {
            double density;
            Point cog;
            double mass;
            MomentOfInertia moi;
            PrincipalAxesOfInertia pai;
            PrincipalMomentOfInertia pmoi;
            double area;
            double volume;

            using (var doc = OpenDataDocument(@"MassPrpPart1.SLDPRT"))
            {
                var part = (ISwPart)m_App.Documents.Active;
                
                using (var massPrps = part.PreCreateMassProperty())
                {
                    massPrps.UserUnits = false;
                    massPrps.VisibleOnly = true;
                    massPrps.Commit();

                    density = massPrps.Density;
                    cog = massPrps.CenterOfGravity;
                    mass = massPrps.Mass;
                    moi = massPrps.MomentOfInertia;
                    pai = massPrps.PrincipalAxesOfInertia;
                    pmoi = massPrps.PrincipalMomentOfInertia;
                    area = massPrps.SurfaceArea;
                    volume = massPrps.Volume;
                }
            }

            AssertCompareDoubles(density, 1000.00000000);
            AssertCompareDoubles(cog.X, -0.15800198);
            AssertCompareDoubles(cog.Y, 0.05435970);
            AssertCompareDoubles(cog.Z, 0.00474664);
            AssertCompareDoubles(mass, 0.60125017);
            AssertCompareDoubles(moi.Lx.X, 0.00164893);
            AssertCompareDoubles(moi.Lx.Y, 0.00133830);
            AssertCompareDoubles(moi.Lx.Z, 0.00045888);
            AssertCompareDoubles(moi.Ly.X, 0.00133830);
            AssertCompareDoubles(moi.Ly.Y, 0.00468398);
            AssertCompareDoubles(moi.Ly.Z, 0.00000325);
            AssertCompareDoubles(moi.Lz.X, 0.00045888);
            AssertCompareDoubles(moi.Lz.Y, 0.00000325);
            AssertCompareDoubles(moi.Lz.Z, 0.00603085);
            AssertCompareDoubles(pai.Ix.X, 0.93305248);
            AssertCompareDoubles(pai.Ix.Y, 0.34902187);
            AssertCompareDoubles(pai.Ix.Z, 0.08715970);
            AssertCompareDoubles(pai.Iy.X, -0.33108391);
            AssertCompareDoubles(pai.Iy.Y, 0.92790273);
            AssertCompareDoubles(pai.Iy.Z, -0.17140585);
            AssertCompareDoubles(pai.Iz.X, -0.14070011);
            AssertCompareDoubles(pai.Iz.Y, 0.13107348);
            AssertCompareDoubles(pai.Iz.Z, 0.98133747);
            AssertCompareDoubles(pmoi.Px, 0.00110545);
            AssertCompareDoubles(pmoi.Py, 0.00516210);
            AssertCompareDoubles(pmoi.Pz, 0.00609621);
            AssertCompareDoubles(area, 0.05933487);
            AssertCompareDoubles(volume, 0.00060125);
        }

        [Test]
        public void MassPropertyPartUserUnitTest()
        {
            double density;
            Point cog;
            double mass;
            MomentOfInertia moi;
            PrincipalAxesOfInertia pai;
            PrincipalMomentOfInertia pmoi;
            double area;
            double volume;

            using (var doc = OpenDataDocument(@"MassPrpPart1.SLDPRT"))
            {
                var part = (ISwPart)m_App.Documents.Active;

                using (var massPrps = part.PreCreateMassProperty())
                {
                    massPrps.UserUnits = true;
                    massPrps.VisibleOnly = true;
                    massPrps.Commit();

                    density = massPrps.Density;
                    cog = massPrps.CenterOfGravity;
                    mass = massPrps.Mass;
                    moi = massPrps.MomentOfInertia;
                    pai = massPrps.PrincipalAxesOfInertia;
                    pmoi = massPrps.PrincipalMomentOfInertia;
                    area = massPrps.SurfaceArea;
                    volume = massPrps.Volume;
                }
            }

            AssertCompareDoubles(density, 0.00100000);
            AssertCompareDoubles(cog.X, -158.00198073);
            AssertCompareDoubles(cog.Y, 54.35970321);
            AssertCompareDoubles(cog.Z, 4.74663690);
            AssertCompareDoubles(mass, 601.25017421);
            AssertCompareDoubles(moi.Lx.X, 1648927.75914956);
            AssertCompareDoubles(moi.Lx.Y, 1338297.62262486);
            AssertCompareDoubles(moi.Lx.Z, 458881.21232608);
            AssertCompareDoubles(moi.Ly.X, 1338297.62262486);
            AssertCompareDoubles(moi.Ly.Y, 4683983.28898563);
            AssertCompareDoubles(moi.Ly.Z, 3254.31155824);
            AssertCompareDoubles(moi.Lz.X, 458881.21232608);
            AssertCompareDoubles(moi.Lz.Y, 3254.31155824);
            AssertCompareDoubles(moi.Lz.Z, 6030848.94700435);
            AssertCompareDoubles(pai.Ix.X, 0.93305248);
            AssertCompareDoubles(pai.Ix.Y, 0.34902187);
            AssertCompareDoubles(pai.Ix.Z, 0.08715970);
            AssertCompareDoubles(pai.Iy.X, -0.33108391);
            AssertCompareDoubles(pai.Iy.Y, 0.92790273);
            AssertCompareDoubles(pai.Iy.Z, -0.17140585);
            AssertCompareDoubles(pai.Iz.X, -0.14070011);
            AssertCompareDoubles(pai.Iz.Y, 0.13107348);
            AssertCompareDoubles(pai.Iz.Z, 0.98133747);
            AssertCompareDoubles(pmoi.Px, 1105452.34207252);
            AssertCompareDoubles(pmoi.Py, 5162100.87963526);
            AssertCompareDoubles(pmoi.Pz, 6096206.77343175);
            AssertCompareDoubles(area, 59334.87176312);
            AssertCompareDoubles(volume, 601250.17421272);
        }

        [Test]
        public void MassPropertyPartRelCoordSysTest()
        {
            double density;
            Point cog;
            double mass;
            MomentOfInertia moi;
            PrincipalAxesOfInertia pai;
            PrincipalMomentOfInertia pmoi;
            double area;
            double volume;

            using (var doc = OpenDataDocument(@"MassPrpPart1.SLDPRT"))
            {
                var part = (ISwPart)m_App.Documents.Active;

                using (var massPrps = part.PreCreateMassProperty())
                {
                    massPrps.Scope = new IXBody[] { part.Bodies["Sweep1"] };
                    massPrps.UserUnits = true;
                    massPrps.RelativeTo = TransformConverter.ToTransformMatrix(
                        part.Model.Extension.GetCoordinateSystemTransformByName("Coordinate System1"));
                    massPrps.Commit();

                    density = massPrps.Density;
                    cog = massPrps.CenterOfGravity;
                    mass = massPrps.Mass;
                    moi = massPrps.MomentOfInertia;
                    pai = massPrps.PrincipalAxesOfInertia;
                    pmoi = massPrps.PrincipalMomentOfInertia;
                    area = massPrps.SurfaceArea;
                    volume = massPrps.Volume;
                }
            }

            AssertCompareDoubles(density, 0.00100000);
            AssertCompareDoubles(cog.X, 127.45790092);
            AssertCompareDoubles(cog.Y, 0.00000000);
            AssertCompareDoubles(cog.Z, 0.00000000);
            AssertCompareDoubles(mass, 500.52600646);
            AssertCompareDoubles(moi.Lx.X, 156414.37701816);
            AssertCompareDoubles(moi.Lx.Y, 0.00000000);
            AssertCompareDoubles(moi.Lx.Z, 0.00000000);
            AssertCompareDoubles(moi.Ly.X, 0.00000000);
            AssertCompareDoubles(moi.Ly.Y, 10919945.18828213);
            AssertCompareDoubles(moi.Ly.Z, 0.00000000);
            AssertCompareDoubles(moi.Lz.X, 0.00000000);
            AssertCompareDoubles(moi.Lz.Y, 0.00000000);
            AssertCompareDoubles(moi.Lz.Z, 10919945.18828213);
            AssertCompareDoubles(pai.Ix.X, 1.00000000);
            AssertCompareDoubles(pai.Ix.Y, 0.00000000);
            AssertCompareDoubles(pai.Ix.Z, 0.00000000);
            AssertCompareDoubles(pai.Iy.X, 0.00000000);
            AssertCompareDoubles(pai.Iy.Y, 0.00000000);
            AssertCompareDoubles(pai.Iy.Z, -1.00000000);
            AssertCompareDoubles(pai.Iz.X, 0.00000000);
            AssertCompareDoubles(pai.Iz.Y, 1.00000000);
            AssertCompareDoubles(pai.Iz.Z, 0.00000000);
            AssertCompareDoubles(pmoi.Px, 156414.37701816);
            AssertCompareDoubles(pmoi.Py, 2788641.68845234);
            AssertCompareDoubles(pmoi.Pz, 2788641.68845234);
            AssertCompareDoubles(area, 43969.07133364);
            AssertCompareDoubles(volume, 500526.00645810);
        }

        [Test]
        public void MassPropertyAssemblyScopedCompTest()
        {
            double density;
            Point cog;
            double mass;
            MomentOfInertia moi;
            PrincipalAxesOfInertia pai;
            PrincipalMomentOfInertia pmoi;
            double area;
            double volume;

            using (var doc = OpenDataDocument(@"MassPrpsAssembly1\Assem1.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                using (var massPrps = assm.PreCreateMassProperty())
                {
                    massPrps.Scope = new IXComponent[] { assm.Configurations.Active.Components["Part1-1"] };
                    massPrps.UserUnits = false;
                    massPrps.Commit();

                    density = massPrps.Density;
                    cog = massPrps.CenterOfGravity;
                    mass = massPrps.Mass;
                    moi = massPrps.MomentOfInertia;
                    pai = massPrps.PrincipalAxesOfInertia;
                    pmoi = massPrps.PrincipalMomentOfInertia;
                    area = massPrps.SurfaceArea;
                    volume = massPrps.Volume;
                }
            }

            AssertCompareDoubles(density, 7300.00000000);
            AssertCompareDoubles(cog.X, 0.03260240);
            AssertCompareDoubles(cog.Y, 0.06212415);
            AssertCompareDoubles(cog.Z, 0.00000000);
            AssertCompareDoubles(mass, 2.25609306);
            AssertCompareDoubles(moi.Lx.X, 0.00328186);
            AssertCompareDoubles(moi.Lx.Y, 0.00358474);
            AssertCompareDoubles(moi.Lx.Z, 0.00000000);
            AssertCompareDoubles(moi.Ly.X, 0.00358474);
            AssertCompareDoubles(moi.Ly.Y, 0.00613822);
            AssertCompareDoubles(moi.Ly.Z, 0.00000000);
            AssertCompareDoubles(moi.Lz.X, 0.00000000);
            AssertCompareDoubles(moi.Lz.Y, 0.00000000);
            AssertCompareDoubles(moi.Lz.Z, 0.00793613);
            AssertCompareDoubles(pai.Ix.X, 0.82768156);
            AssertCompareDoubles(pai.Ix.Y, 0.56119804);
            AssertCompareDoubles(pai.Ix.Z, 0.00000000);
            AssertCompareDoubles(pai.Iy.X, 0.00000000);
            AssertCompareDoubles(pai.Iy.Y, 0.00000000);
            AssertCompareDoubles(pai.Iy.Z, -1.00000000);
            AssertCompareDoubles(pai.Iz.X, -0.56119804);
            AssertCompareDoubles(pai.Iz.Y, 0.82768156);
            AssertCompareDoubles(pai.Iz.Z, 0.00000000);
            AssertCompareDoubles(pmoi.Px, 0.00085128);
            AssertCompareDoubles(pmoi.Py, 0.00793613);
            AssertCompareDoubles(pmoi.Pz, 0.00856881);
            AssertCompareDoubles(area, 0.03850408);
            AssertCompareDoubles(volume, 0.00030905);
        }

        [Test]
        public void MassPropertyAssemblyFullTest()
        {
            double density;
            Point cog;
            double mass;
            MomentOfInertia moi;
            PrincipalAxesOfInertia pai;
            PrincipalMomentOfInertia pmoi;
            double area;
            double volume;

            using (var doc = OpenDataDocument(@"MassPrpsAssembly1\Assem1.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                using (var massPrps = assm.PreCreateMassProperty())
                {
                    massPrps.UserUnits = false;
                    massPrps.VisibleOnly = true;
                    massPrps.Commit();

                    density = massPrps.Density;
                    cog = massPrps.CenterOfGravity;
                    mass = massPrps.Mass;
                    moi = massPrps.MomentOfInertia;
                    pai = massPrps.PrincipalAxesOfInertia;
                    pmoi = massPrps.PrincipalMomentOfInertia;
                    area = massPrps.SurfaceArea;
                    volume = massPrps.Volume;
                }
            }

            AssertCompareDoubles(density, 5603.0008626423742);
            AssertCompareDoubles(cog.X, 0.01534540);
            AssertCompareDoubles(cog.Y, 0.04013394);
            AssertCompareDoubles(cog.Z, 0.02297242);
            AssertCompareDoubles(mass, 4.53056397);
            AssertCompareDoubles(moi.Lx.X, 0.01258938);
            AssertCompareDoubles(moi.Lx.Y, 0.00628366);
            AssertCompareDoubles(moi.Lx.Z, -0.00367561);
            AssertCompareDoubles(moi.Ly.X, 0.00628366);
            AssertCompareDoubles(moi.Ly.Y, 0.01632613);
            AssertCompareDoubles(moi.Ly.Z, -0.00299375);
            AssertCompareDoubles(moi.Lz.X, -0.00367561);
            AssertCompareDoubles(moi.Lz.Y, -0.00299375);
            AssertCompareDoubles(moi.Lz.Z, 0.01506285);
            AssertCompareDoubles(pai.Ix.X, 0.71409552);
            AssertCompareDoubles(pai.Ix.Y, 0.53992467);
            AssertCompareDoubles(pai.Ix.Z, -0.44558830);
            AssertCompareDoubles(pai.Iy.X, -0.38690303);
            AssertCompareDoubles(pai.Iy.Y, -0.22607012);
            AssertCompareDoubles(pai.Iy.Z, -0.89397894);
            AssertCompareDoubles(pai.Iz.X, -0.58341548);
            AssertCompareDoubles(pai.Iz.Y, 0.81078582);
            AssertCompareDoubles(pai.Iz.Z, 0.04746283);
            AssertCompareDoubles(pmoi.Px, 0.00554479);
            AssertCompareDoubles(pmoi.Py, 0.01741067);
            AssertCompareDoubles(pmoi.Pz, 0.02102291);
            AssertCompareDoubles(area, 0.08548317);
            AssertCompareDoubles(volume, 0.00080860);
        }

        [Test]
        public void MassPropertyAssemblyUserUnitTest()
        {
            double density;
            Point cog;
            double mass;
            MomentOfInertia moi;
            PrincipalAxesOfInertia pai;
            PrincipalMomentOfInertia pmoi;
            double area;
            double volume;

            using (var doc = OpenDataDocument(@"MassPrpsAssembly1\Assem1.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                using (var massPrps = assm.PreCreateMassProperty())
                {
                    massPrps.Scope = new IXComponent[] { assm.Configurations.Active.Components["SubAssem1-1"].Children["Part2-1"] };
                    massPrps.UserUnits = true;
                    massPrps.VisibleOnly = true;
                    massPrps.Commit();

                    density = massPrps.Density;
                    cog = massPrps.CenterOfGravity;
                    mass = massPrps.Mass;
                    moi = massPrps.MomentOfInertia;
                    pai = massPrps.PrincipalAxesOfInertia;
                    pmoi = massPrps.PrincipalMomentOfInertia;
                    area = massPrps.SurfaceArea;
                    volume = massPrps.Volume;
                }
            }

            AssertCompareDoubles(density, 0.30708190);
            AssertCompareDoubles(cog.X, -0.51644616);
            AssertCompareDoubles(cog.Y, 0.53002827);
            AssertCompareDoubles(cog.Z, 2.18503937);
            AssertCompareDoubles(mass, 4.43478664);
            AssertCompareDoubles(moi.Lx.X, 8.22406199);
            AssertCompareDoubles(moi.Lx.Y, 0.00000000);
            AssertCompareDoubles(moi.Lx.Z, 0.00000000);
            AssertCompareDoubles(moi.Ly.X, 0.00000000);
            AssertCompareDoubles(moi.Ly.Y, 8.22406199);
            AssertCompareDoubles(moi.Ly.Z, 0.00000000);
            AssertCompareDoubles(moi.Lz.X, 0.00000000);
            AssertCompareDoubles(moi.Lz.Y, 0.00000000);
            AssertCompareDoubles(moi.Lz.Z, 2.33250250);
            AssertCompareDoubles(pai.Ix.X, 0.00000000);
            AssertCompareDoubles(pai.Ix.Y, 0.00000000);
            AssertCompareDoubles(pai.Ix.Z, 1.00000000);
            AssertCompareDoubles(pai.Iy.X, 0.00000000);
            AssertCompareDoubles(pai.Iy.Y, -1.00000000);
            AssertCompareDoubles(pai.Iy.Z, 0.00000000);
            AssertCompareDoubles(pai.Iz.X, 1.00000000);
            AssertCompareDoubles(pai.Iz.Y, 0.00000000);
            AssertCompareDoubles(pai.Iz.Z, 0.00000000);
            AssertCompareDoubles(pmoi.Px, 2.33250250);
            AssertCompareDoubles(pmoi.Py, 8.22406199);
            AssertCompareDoubles(pmoi.Pz, 8.22406199);
            AssertCompareDoubles(area, 34.77105464);
            AssertCompareDoubles(volume, 14.44170660);
        }

        [Test]
        public void MassPropertyAssemblyIncludeHiddenTest()
        {
            double density;
            Point cog;
            double mass;
            MomentOfInertia moi;
            PrincipalAxesOfInertia pai;
            PrincipalMomentOfInertia pmoi;
            double area;
            double volume;

            using (var doc = OpenDataDocument(@"MassPrpsAssembly1\Assem1.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                using (var massPrps = assm.PreCreateMassProperty())
                {
                    massPrps.UserUnits = false;
                    massPrps.VisibleOnly = false;
                    massPrps.Commit();

                    density = massPrps.Density;
                    cog = massPrps.CenterOfGravity;
                    mass = massPrps.Mass;
                    moi = massPrps.MomentOfInertia;
                    pai = massPrps.PrincipalAxesOfInertia;
                    pmoi = massPrps.PrincipalMomentOfInertia;
                    area = massPrps.SurfaceArea;
                    volume = massPrps.Volume;
                }
            }

            AssertCompareDoubles(density, 2709.2811116185171);
            AssertCompareDoubles(cog.X, 0.03657451);
            AssertCompareDoubles(cog.Y, -0.00669951);
            AssertCompareDoubles(cog.Z, 0.24174744);
            AssertCompareDoubles(mass, 8.71280826);
            AssertCompareDoubles(moi.Lx.X, 0.72951493);
            AssertCompareDoubles(moi.Lx.Y, -0.01723309);
            AssertCompareDoubles(moi.Lx.Z, 0.09435880);
            AssertCompareDoubles(moi.Ly.X, -0.01723309);
            AssertCompareDoubles(moi.Ly.Y, 0.73205728);
            AssertCompareDoubles(moi.Ly.Z, -0.14902465);
            AssertCompareDoubles(moi.Lz.X, 0.09435880);
            AssertCompareDoubles(moi.Lz.Y, -0.14902465);
            AssertCompareDoubles(moi.Lz.Z, 0.08274464);
            AssertCompareDoubles(pai.Ix.X, 0.13710383);
            AssertCompareDoubles(pai.Ix.Y, -0.21088390);
            AssertCompareDoubles(pai.Ix.Z, 0.96784840);
            AssertCompareDoubles(pai.Iy.X, 0.98002680);
            AssertCompareDoubles(pai.Iy.Y, 0.17096646);
            AssertCompareDoubles(pai.Iy.Z, -0.10157722);
            AssertCompareDoubles(pai.Iz.X, -0.14404861);
            AssertCompareDoubles(pai.Iz.Y, 0.96244400);
            AssertCompareDoubles(pai.Iz.Z, 0.23011203);
            AssertCompareDoubles(pmoi.Px, 0.03690704);
            AssertCompareDoubles(pmoi.Py, 0.74230130);
            AssertCompareDoubles(pmoi.Pz, 0.76510851);
            AssertCompareDoubles(area, 0.32101114);
            AssertCompareDoubles(volume, 0.00321591);
        }

        [Test]
        public void MassPropertyAssemblyRelCoordSysTest()
        {
            double density;
            Point cog;
            double mass;
            MomentOfInertia moi;
            PrincipalAxesOfInertia pai;
            PrincipalMomentOfInertia pmoi;
            double area;
            double volume;

            using (var doc = OpenDataDocument(@"MassPrpsAssembly1\Assem1.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                using (var massPrps = assm.PreCreateMassProperty())
                {
                    massPrps.Scope = new IXComponent[] { assm.Configurations.Active.Components["Part1-1"] };
                    massPrps.UserUnits = false;
                    massPrps.RelativeTo = TransformConverter.ToTransformMatrix(
                        assm.Model.Extension.GetCoordinateSystemTransformByName("Coordinate System1"));

                    massPrps.Commit();

                    density = massPrps.Density;
                    cog = massPrps.CenterOfGravity;
                    mass = massPrps.Mass;
                    moi = massPrps.MomentOfInertia;
                    pai = massPrps.PrincipalAxesOfInertia;
                    pmoi = massPrps.PrincipalMomentOfInertia;
                    area = massPrps.SurfaceArea;
                    volume = massPrps.Volume;
                }
            }

            AssertCompareDoubles(density, 7300.00000000);
            AssertCompareDoubles(cog.X, 0.10201760);
            AssertCompareDoubles(cog.Y, 0.00000000);
            AssertCompareDoubles(cog.Z, 0.00000000);
            AssertCompareDoubles(mass, 2.25609306);
            AssertCompareDoubles(moi.Lx.X, 0.00085128);
            AssertCompareDoubles(moi.Lx.Y, 0.00000000);
            AssertCompareDoubles(moi.Lx.Z, 0.00000000);
            AssertCompareDoubles(moi.Ly.X, 0.00000000);
            AssertCompareDoubles(moi.Ly.Y, 0.03204930);
            AssertCompareDoubles(moi.Ly.Z, 0.00000000);
            AssertCompareDoubles(moi.Lz.X, 0.00000000);
            AssertCompareDoubles(moi.Lz.Y, 0.00000000);
            AssertCompareDoubles(moi.Lz.Z, 0.03141662);
            AssertCompareDoubles(pai.Ix.X, 1.00000000);
            AssertCompareDoubles(pai.Ix.Y, 0.00000000);
            AssertCompareDoubles(pai.Ix.Z, 0.00000000);
            AssertCompareDoubles(pai.Iy.X, 0.00000000);
            AssertCompareDoubles(pai.Iy.Y, 0.00000000);
            AssertCompareDoubles(pai.Iy.Z, -1.00000000);
            AssertCompareDoubles(pai.Iz.X, 0.00000000);
            AssertCompareDoubles(pai.Iz.Y, 1.00000000);
            AssertCompareDoubles(pai.Iz.Z, 0.00000000);
            AssertCompareDoubles(pmoi.Px, 0.00085128);
            AssertCompareDoubles(pmoi.Py, 0.00793613);
            AssertCompareDoubles(pmoi.Pz, 0.00856881);
            AssertCompareDoubles(area, 0.03850408);
            AssertCompareDoubles(volume, 0.00030905);
        }

        [Test]
        public void MassPropertyAssemblyChangeScopeTest()
        {
            double mass1;
            PrincipalMomentOfInertia pmoi1;

            double mass2;
            PrincipalMomentOfInertia pmoi2;

            double mass3;
            PrincipalMomentOfInertia pmoi3;

            double mass4;
            PrincipalMomentOfInertia pmoi4;

            using (var doc = OpenDataDocument(@"MassPrpsAssembly1\Assem1.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                using (var massPrps = assm.PreCreateMassProperty())
                {
                    massPrps.UserUnits = false;
                    massPrps.VisibleOnly = false;
                    massPrps.Commit();

                    mass1 = massPrps.Mass;
                    pmoi1 = massPrps.PrincipalMomentOfInertia;

                    massPrps.Scope = new IXComponent[] { assm.Configurations.Active.Components["Part1-1"] };

                    mass2 = massPrps.Mass;
                    pmoi2 = massPrps.PrincipalMomentOfInertia;

                    massPrps.Scope = new IXComponent[] { assm.Configurations.Active.Components["SubAssem1-1"] };

                    mass3 = massPrps.Mass;
                    pmoi3 = massPrps.PrincipalMomentOfInertia;

                    massPrps.Scope = null;

                    mass4 = massPrps.Mass;
                    pmoi4 = massPrps.PrincipalMomentOfInertia;
                }
            }

            AssertCompareDoubles(mass1, 8.71280826);
            AssertCompareDoubles(pmoi1.Px, 0.03690704);
            AssertCompareDoubles(pmoi1.Py, 0.74230130);
            AssertCompareDoubles(pmoi1.Pz, 0.76510851);

            AssertCompareDoubles(mass2, 2.25609306);
            AssertCompareDoubles(pmoi2.Px, 0.00085128);
            AssertCompareDoubles(pmoi2.Py, 0.00793613);
            AssertCompareDoubles(pmoi2.Pz, 0.00856881);

            AssertCompareDoubles(mass3, 3.22835760);
            AssertCompareDoubles(pmoi3.Px, 0.01081765);
            AssertCompareDoubles(pmoi3.Py, 0.01357870);
            AssertCompareDoubles(pmoi3.Pz, 0.02074788);

            AssertCompareDoubles(mass4, 8.71280826);
            AssertCompareDoubles(pmoi4.Px, 0.03690704);
            AssertCompareDoubles(pmoi4.Py, 0.74230130);
            AssertCompareDoubles(pmoi4.Pz, 0.76510851);
        }

        [Test]
        public void MassPropertyAssignedPropsComponentTest()
        {
            double mass1;
            Point cog1;
            PrincipalAxesOfInertia pai1;
            PrincipalMomentOfInertia pmoi1;

            using (var doc = OpenDataDocument(@"MassPrpsAssembly2\Assem1.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                using (var massPrps = assm.PreCreateMassProperty())
                {
                    massPrps.UserUnits = false;
                    massPrps.VisibleOnly = false;
                    massPrps.Scope = new IXComponent[] { assm.Configurations.Active.Components["Overriden-1"] };

                    massPrps.Commit();

                    mass1 = massPrps.Mass;
                    cog1 = massPrps.CenterOfGravity;
                    pmoi1 = massPrps.PrincipalMomentOfInertia;
                    pai1 = massPrps.PrincipalAxesOfInertia;
                }
            }

            AssertCompareDoubles(mass1, 0.02500000);
            AssertCompareDoubles(cog1.X, 0.03495069);
            AssertCompareDoubles(cog1.Y, -0.09596771);
            AssertCompareDoubles(cog1.Z, 0.10021386);
            AssertCompareDoubles(pmoi1.Px, 0.00000500);
            AssertCompareDoubles(pmoi1.Py, 0.00000600);
            AssertCompareDoubles(pmoi1.Pz, 0.00000700);
            AssertCompareDoubles(pai1.Ix.X, 0.57735027);
            AssertCompareDoubles(pai1.Ix.Y, 0.57735027);
            AssertCompareDoubles(pai1.Ix.Z, 0.57735027);
            AssertCompareDoubles(pai1.Iy.X, 0.70710678);
            AssertCompareDoubles(pai1.Iy.Y, -0.70710678);
            AssertCompareDoubles(pai1.Iy.Z, 0.00000000);
            AssertCompareDoubles(pai1.Iz.X, 0.40824829);
            AssertCompareDoubles(pai1.Iz.Y, 0.40824829);
            AssertCompareDoubles(pai1.Iz.Z, -0.81649658);
        }

        [Test]
        public void MassPropertyAssignedPropsComponentRefCoordTest()
        {
            double mass1;
            Point cog1;
            PrincipalAxesOfInertia pai1;
            PrincipalMomentOfInertia pmoi1;

            using (var doc = OpenDataDocument(@"MassPrpsAssembly2\Assem1.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                using (var massPrps = assm.PreCreateMassProperty())
                {
                    massPrps.UserUnits = true;
                    massPrps.VisibleOnly = true;
                    massPrps.RelativeTo = TransformConverter.ToTransformMatrix(
                        assm.Model.Extension.GetCoordinateSystemTransformByName("Coordinate System1"));

                    massPrps.Scope = new IXComponent[] { assm.Configurations.Active.Components["Overriden-1"] };

                    massPrps.Commit();

                    mass1 = massPrps.Mass;
                    cog1 = massPrps.CenterOfGravity;
                    pmoi1 = massPrps.PrincipalMomentOfInertia;
                    pai1 = massPrps.PrincipalAxesOfInertia;
                }
            }

            AssertCompareDoubles(mass1, 25.00000000);
            AssertCompareDoubles(cog1.X, 117.06080141);
            AssertCompareDoubles(cog1.Y, -319.12608668);
            AssertCompareDoubles(cog1.Z, -55.54863587);
            AssertCompareDoubles(pmoi1.Px, 5000.00000000);
            AssertCompareDoubles(pmoi1.Py, 6000.00000000);
            AssertCompareDoubles(pmoi1.Pz, 7000.00000000);
            AssertCompareDoubles(pai1.Ix.X, 0.84143725);
            AssertCompareDoubles(pai1.Ix.Y, 0.53912429);
            AssertCompareDoubles(pai1.Ix.Z, 0.03644656);
            AssertCompareDoubles(pai1.Iy.X, 0.48866011);
            AssertCompareDoubles(pai1.Iy.Y, -0.73041626);
            AssertCompareDoubles(pai1.Iy.Z, -0.47718275);
            AssertCompareDoubles(pai1.Iz.X, -0.23063965);
            AssertCompareDoubles(pai1.Iz.Y, 0.41932932);
            AssertCompareDoubles(pai1.Iz.Z, -0.87804799);
        }

        [Test]
        public void MassPropertyAssignedPropsPartTest()
        {
            double mass1;
            Point cog1;
            PrincipalAxesOfInertia pai1;
            PrincipalMomentOfInertia pmoi1;

            using (var doc = OpenDataDocument(@"MassPrpsAssembly2\Overriden.SLDPRT"))
            {
                var part = (ISwPart)m_App.Documents.Active;

                using (var massPrps = part.PreCreateMassProperty())
                {
                    massPrps.UserUnits = false;
                    massPrps.VisibleOnly = false;

                    massPrps.Commit();

                    mass1 = massPrps.Mass;
                    cog1 = massPrps.CenterOfGravity;
                    pmoi1 = massPrps.PrincipalMomentOfInertia;
                    pai1 = massPrps.PrincipalAxesOfInertia;
                }
            }

            AssertCompareDoubles(mass1, 0.02500000);
            AssertCompareDoubles(cog1.X, -0.01);
            AssertCompareDoubles(cog1.Y, -0.02);
            AssertCompareDoubles(cog1.Z, -0.03);
            AssertCompareDoubles(pmoi1.Px, 0.00000500);
            AssertCompareDoubles(pmoi1.Py, 0.00000600);
            AssertCompareDoubles(pmoi1.Pz, 0.00000700);
            AssertCompareDoubles(pai1.Ix.X, 0.57735027);
            AssertCompareDoubles(pai1.Ix.Y, 0.57735027);
            AssertCompareDoubles(pai1.Ix.Z, 0.57735027);
            AssertCompareDoubles(pai1.Iy.X, 0.70710678);
            AssertCompareDoubles(pai1.Iy.Y, -0.70710678);
            AssertCompareDoubles(pai1.Iy.Z, 0.00000000);
            AssertCompareDoubles(pai1.Iz.X, 0.40824829);
            AssertCompareDoubles(pai1.Iz.Y, 0.40824829);
            AssertCompareDoubles(pai1.Iz.Z, -0.81649658);
        }

        [Test]
        public void MassPropertyEmptyComponentsTest()
        {
            using (var doc = OpenDataDocument(@"MassPrpsAssembly2\Assem1.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                using (var massPrps = assm.PreCreateMassProperty())
                {
                    massPrps.UserUnits = false;
                    massPrps.VisibleOnly = false;

                    massPrps.Scope = new IXComponent[] { assm.Configurations.Active.Components["Empty-1"] };
                    Assert.Throws<EvaluationFailedException>(() => massPrps.Commit());
                    Assert.DoesNotThrow(() =>
                    {
                        massPrps.Scope = new IXComponent[] { assm.Configurations.Active.Components["Part1-1"] };
                        massPrps.Commit();
                    });
                    Assert.Throws<EvaluationFailedException>(() => massPrps.Scope = new IXComponent[] { assm.Configurations.Active.Components["Sketch-1"] });
                    Assert.DoesNotThrow(() => massPrps.Scope = new IXComponent[] { assm.Configurations.Active.Components["Part1-1"] });
                    Assert.Throws<EvaluationFailedException>(() => massPrps.Scope = new IXComponent[] { assm.Configurations.Active.Components["Surface-1"] });
                    Assert.DoesNotThrow(() => massPrps.Scope = new IXComponent[] { assm.Configurations.Active.Components["SubAssem1-1"] });
                    Assert.DoesNotThrow(() => massPrps.Scope = new IXComponent[] { assm.Configurations.Active.Components["Part2-1"] });
                }
            }
        }

        [Test]
        public void MassPropertyEmptyAssemblyTest()
        {
            using (var doc = NewDocument(Interop.swconst.swDocumentTypes_e.swDocASSEMBLY))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                using (var massPrps = assm.PreCreateMassProperty())
                {
                    massPrps.UserUnits = false;
                    massPrps.VisibleOnly = false;

                    massPrps.Scope = null;
                    Assert.Throws<EvaluationFailedException>(() => massPrps.Commit());
                    Assert.Throws<EvaluationFailedException>(() =>
                    {
                        massPrps.Scope = new IXComponent[0];
                        massPrps.Commit();
                    });
                }
            }
        }

        [Test]
        public void MassPropertyEmptyPartTest()
        {
            using (var doc = OpenDataDocument(@"MassPrpsAssembly2\Surface.sldprt"))
            {
                var part = (ISwPart)m_App.Documents.Active;

                using (var massPrps = part.PreCreateMassProperty())
                {
                    massPrps.UserUnits = false;
                    massPrps.VisibleOnly = false;

                    massPrps.Scope = null;
                    Assert.Throws<EvaluationFailedException>(() => massPrps.Commit());
                    Assert.Throws<EvaluationFailedException>(() =>
                    {
                        massPrps.Scope = new IXBody[0];
                        massPrps.Commit();
                    });
                }
            }
        }

        [Test]
        public void MassPropertyNoVisibleBodiesTest()
        {
            using (var doc = OpenDataDocument(@"MassPrpsAssembly2\Assem1.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                using (var massPrps = assm.PreCreateMassProperty())
                {
                    massPrps.VisibleOnly = true;

                    Assert.Throws<EvaluationFailedException>(() =>
                    {
                        massPrps.Scope = new IXComponent[] { assm.Configurations.Active.Components["SubAssem1-1"] };
                        massPrps.Commit();
                    });

                    Assert.Throws<EvaluationFailedException>(() =>
                    {
                        massPrps.Scope = new IXComponent[] { assm.Configurations.Active.Components["Part2-1"] };
                        massPrps.Commit();
                    });
                }
            }
        }
    }
}
