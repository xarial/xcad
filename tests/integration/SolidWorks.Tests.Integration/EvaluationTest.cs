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

                var bbox = part.Evaluation.PreCreateBoundingBox();
                bbox.Precise = true;
                bbox.UserUnits = true;
                bbox.Commit();
                b1 = bbox.Box;

                bbox = part.Evaluation.PreCreateBoundingBox();
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

                var bbox = part.Evaluation.PreCreateBoundingBox();
                bbox.Precise = true;
                bbox.Scope = new IXBody[] { body };
                bbox.Commit();
                b1 = bbox.Box;

                bbox = part.Evaluation.PreCreateBoundingBox();
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

                var bbox = part.Evaluation.PreCreateBoundingBox();
                bbox.Precise = true;
                bbox.RelativeTo = matrix;
                bbox.Scope = new IXBody[] { body };
                bbox.Commit();
                b1 = bbox.Box;

                bbox = part.Evaluation.PreCreateBoundingBox();
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

                var bbox = part.Evaluation.PreCreateBoundingBox();
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

                var bbox = assm.Evaluation.PreCreateBoundingBox();
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

                var bbox = assm.Evaluation.PreCreateBoundingBox();
                bbox.Precise = true;
                bbox.RelativeTo = matrix;
                bbox.Scope = comps;
                bbox.Commit();
                b1 = bbox.Box;

                bbox = assm.Evaluation.PreCreateBoundingBox();
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

                var bbox = assm.Evaluation.PreCreateBoundingBox();
                bbox.Precise = false;
                bbox.Commit();
                b1 = bbox.Box;

                try
                {
                    bbox = assm.Evaluation.PreCreateBoundingBox();
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

                var bbox = assm.Evaluation.PreCreateBoundingBox();
                bbox.Scope = comps;
                bbox.Precise = false;
                bbox.Commit();
                b1 = bbox.Box;

                try
                {
                    bbox = assm.Evaluation.PreCreateBoundingBox();
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

                var bbox = assm.Evaluation.PreCreateBoundingBox();
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

                using (var bbox = assm.Evaluation.PreCreateBoundingBox())
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

                using (var bbox = assm.Evaluation.PreCreateBoundingBox())
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

                using (var bbox = part.Evaluation.PreCreateBoundingBox())
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
                
                using (var massPrps = part.Evaluation.PreCreateMassProperty())
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
                
                using (var massPrps = part.Evaluation.PreCreateMassProperty())
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

                using (var massPrps = part.Evaluation.PreCreateMassProperty())
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

                using (var massPrps = part.Evaluation.PreCreateMassProperty())
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

                using (var massPrps = assm.Evaluation.PreCreateMassProperty())
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

                using (var massPrps = assm.Evaluation.PreCreateMassProperty())
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

                using (var massPrps = assm.Evaluation.PreCreateMassProperty())
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

                using (var massPrps = assm.Evaluation.PreCreateMassProperty())
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

                using (var massPrps = assm.Evaluation.PreCreateMassProperty())
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

                using (var massPrps = assm.Evaluation.PreCreateMassProperty())
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
            MomentOfInertia moi1 = null;

            using (var doc = OpenDataDocument(@"MassPrpsAssembly2\Assem1.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                using (var massPrps = assm.Evaluation.PreCreateMassProperty())
                {
                    massPrps.UserUnits = false;
                    massPrps.VisibleOnly = false;
                    massPrps.Scope = new IXComponent[] { assm.Configurations.Active.Components["Overriden-1"] };

                    massPrps.Commit();

                    mass1 = massPrps.Mass;
                    cog1 = massPrps.CenterOfGravity;
                    pmoi1 = massPrps.PrincipalMomentOfInertia;
                    pai1 = massPrps.PrincipalAxesOfInertia;
                    if (m_App.IsVersionNewerOrEqual(Xarial.XCad.SolidWorks.Enums.SwVersion_e.Sw2020))
                    {
                        moi1 = massPrps.MomentOfInertia;
                    }
                    else
                    {
                        Assert.Throws<MomentOfInertiaOverridenException>(() => 
                        {
                            var x = massPrps.MomentOfInertia;
                        });
                    }
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
            if (m_App.IsVersionNewerOrEqual(Xarial.XCad.SolidWorks.Enums.SwVersion_e.Sw2020))
            {
                AssertCompareDoubles(moi1.Lx.X, 0.00000689);
                AssertCompareDoubles(moi1.Lx.Y, -0.00000012);
                AssertCompareDoubles(moi1.Lx.Z, 0.00000041);
                AssertCompareDoubles(moi1.Ly.X, -0.00000012);
                AssertCompareDoubles(moi1.Ly.Y, 0.00000602);
                AssertCompareDoubles(moi1.Ly.Z, 0.00000008);
                AssertCompareDoubles(moi1.Lz.X, 0.00000041);
                AssertCompareDoubles(moi1.Lz.Y, 0.00000008);
                AssertCompareDoubles(moi1.Lz.Z, 0.00000509);
            }
        }

        [Test]
        public void MassPropertyAssignedPropsSubComponentTest()
        {
            double mass1;
            double density1;
            double volume1;
            double area1;
            Point cog1;
            PrincipalAxesOfInertia pai1;
            PrincipalMomentOfInertia pmoi1;
            MomentOfInertia moi1;

            double mass2;
            double density2;
            double volume2;
            double area2;
            Point cog2;
            PrincipalAxesOfInertia pai2;
            PrincipalMomentOfInertia pmoi2;
            MomentOfInertia moi2;

            using (var doc = OpenDataDocument(@"MassPrpsAssembly3\Assem1.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                using (var massPrps = assm.Evaluation.PreCreateMassProperty())
                {
                    massPrps.UserUnits = true;
                    massPrps.VisibleOnly = false;
                    massPrps.Scope = new IXComponent[] { assm.Configurations.Active.Components["Assem2-1"] };

                    massPrps.Commit();

                    area1 = massPrps.SurfaceArea;
                    density1 = massPrps.Density;
                    volume1 = massPrps.Volume;
                    mass1 = massPrps.Mass;
                    moi1 = massPrps.MomentOfInertia;
                    cog1 = massPrps.CenterOfGravity;
                    pmoi1 = massPrps.PrincipalMomentOfInertia;
                    pai1 = massPrps.PrincipalAxesOfInertia;
                }

                using (var massPrps = assm.Evaluation.PreCreateMassProperty())
                {
                    massPrps.UserUnits = false;
                    massPrps.VisibleOnly = false;
                    massPrps.Scope = new IXComponent[] { assm.Configurations.Active.Components["Assem2-1"] };
                    massPrps.RelativeTo = TransformConverter.ToTransformMatrix(
                        assm.Model.Extension.GetCoordinateSystemTransformByName("Coordinate System1"));

                    massPrps.Commit();

                    area2 = massPrps.SurfaceArea;
                    density2 = massPrps.Density;
                    volume2 = massPrps.Volume;
                    mass2 = massPrps.Mass;
                    moi2 = massPrps.MomentOfInertia;
                    cog2 = massPrps.CenterOfGravity;
                    pmoi2 = massPrps.PrincipalMomentOfInertia;
                    pai2 = massPrps.PrincipalAxesOfInertia;
                }
            }

            AssertCompareDoubles(area1, 60465.27053010);
            AssertCompareDoubles(density1, 0.0016646521587058);
            AssertCompareDoubles(volume1, 247196.25491005);
            AssertCompareDoubles(mass1, 411.49577936);
            AssertCompareDoubles(cog1.X, 182.14962799);
            AssertCompareDoubles(cog1.Y, -102.41561654);
            AssertCompareDoubles(cog1.Z, -55.10900463);
            AssertCompareDoubles(pmoi1.Px, 149212.95863893);
            AssertCompareDoubles(pmoi1.Py, 6557483.61812571);
            AssertCompareDoubles(pmoi1.Pz, 6699522.03125179);
            AssertCompareDoubles(pai1.Ix.X, -0.05974859);
            AssertCompareDoubles(pai1.Ix.Y, 0.30564005);
            AssertCompareDoubles(pai1.Ix.Z, 0.95027063);
            AssertCompareDoubles(pai1.Iy.X, 0.78418176);
            AssertCompareDoubles(pai1.Iy.Y, 0.60340725);
            AssertCompareDoubles(pai1.Iy.Z, -0.14477104);
            AssertCompareDoubles(pai1.Iz.X, -0.61764802);
            AssertCompareDoubles(pai1.Iz.Y, 0.73653502);
            AssertCompareDoubles(pai1.Iz.Z, -0.27573009);
            AssertCompareDoubles(moi1.Lx.X, 6588792.87251996);
            AssertCompareDoubles(moi1.Lx.Y, -52409.00818634);
            AssertCompareDoubles(moi1.Lx.Z, -388034.23992010);
            AssertCompareDoubles(moi1.Ly.X, -52409.00818634);
            AssertCompareDoubles(moi1.Ly.Y, 6035903.15614703);
            AssertCompareDoubles(moi1.Ly.Z, 1890068.89237849);
            AssertCompareDoubles(moi1.Lz.X, -388034.23992010);
            AssertCompareDoubles(moi1.Lz.Y, 1890068.89237849);
            AssertCompareDoubles(moi1.Lz.Z, 781522.57934945);

            AssertCompareDoubles(area2, 0.06046527);
            AssertCompareDoubles(density2, 1664.6521587058);
            AssertCompareDoubles(volume2, 0.00024720);
            AssertCompareDoubles(mass2, 0.41149578);
            AssertCompareDoubles(cog2.X, 0.10276163);
            AssertCompareDoubles(cog2.Y, -0.16882696);
            AssertCompareDoubles(cog2.Z, 0.14185856);
            AssertCompareDoubles(pmoi2.Px, 0.00014921);
            AssertCompareDoubles(pmoi2.Py, 0.00655748);
            AssertCompareDoubles(pmoi2.Pz, 0.00669952);
            AssertCompareDoubles(pai2.Ix.X, -0.99711183);
            AssertCompareDoubles(pai2.Ix.Y, 0.04688399);
            AssertCompareDoubles(pai2.Ix.Z, -0.05974859);
            AssertCompareDoubles(pai2.Iy.X, -0.01782426);
            AssertCompareDoubles(pai2.Iy.Y, 0.62027515);
            AssertCompareDoubles(pai2.Iy.Z, 0.78418176);
            AssertCompareDoubles(pai2.Iz.X, 0.07382614);
            AssertCompareDoubles(pai2.Iz.Y, 0.78298188);
            AssertCompareDoubles(pai2.Iz.Z, -0.61764802);
            AssertCompareDoubles(moi2.Lx.X, 0.02019650);
            AssertCompareDoubles(moi2.Lx.Y, -0.00744680);
            AssertCompareDoubles(moi2.Lx.Z, 0.00638688);
            AssertCompareDoubles(moi2.Ly.X, -0.00744680);
            AssertCompareDoubles(moi2.Ly.Y, 0.01925673);
            AssertCompareDoubles(moi2.Ly.Z, -0.00980440);
            AssertCompareDoubles(moi2.Lz.X, 0.00638688);
            AssertCompareDoubles(moi2.Lz.Y, -0.00980440);
            AssertCompareDoubles(moi2.Lz.Z, 0.02266284);
        }

        [Test]
        public void MassPropertyAssignedPropsComponentRefCoordTest()
        {
            double mass1;
            Point cog1;
            PrincipalAxesOfInertia pai1;
            PrincipalMomentOfInertia pmoi1;
            MomentOfInertia moi1 = null;

            using (var doc = OpenDataDocument(@"MassPrpsAssembly2\Assem1.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                using (var massPrps = assm.Evaluation.PreCreateMassProperty())
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
                    if (m_App.IsVersionNewerOrEqual(Xarial.XCad.SolidWorks.Enums.SwVersion_e.Sw2020))
                    {
                        moi1 = massPrps.MomentOfInertia;
                    }
                    else 
                    {
                        Assert.Throws<MomentOfInertiaOverridenException>(() =>
                        {
                            var x = massPrps.MomentOfInertia;
                        });
                    }
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
            if (m_App.IsVersionNewerOrEqual(Xarial.XCad.SolidWorks.Enums.SwVersion_e.Sw2020))
            {
                AssertCompareDoubles(moi1.Lx.X, 2628846.22823299);
                AssertCompareDoubles(moi1.Lx.Y, -933979.23905004);
                AssertCompareDoubles(moi1.Lx.Z, -161624.97699916);
                AssertCompareDoubles(moi1.Ly.X, -933979.23905004);
                AssertCompareDoubles(moi1.Ly.Y, 425733.37087830);
                AssertCompareDoubles(moi1.Ly.Z, 443270.58593256);
                AssertCompareDoubles(moi1.Lz.X, -161624.97699916);
                AssertCompareDoubles(moi1.Lz.Y, 443270.58593256);
                AssertCompareDoubles(moi1.Lz.Z, 2894937.46974962);
            }
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

                using (var massPrps = part.Evaluation.PreCreateMassProperty())
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
        public void MassPropertyAssignedPropsUserUnitPartTest()
        {
            double mass1;
            Point cog1;
            PrincipalAxesOfInertia pai1;
            PrincipalMomentOfInertia pmoi1;

            using (var doc = OpenDataDocument(@"MassPrpsAssembly2\Overriden.SLDPRT"))
            {
                var part = (ISwPart)m_App.Documents.Active;

                using (var massPrps = part.Evaluation.PreCreateMassProperty())
                {
                    massPrps.UserUnits = true;
                    massPrps.VisibleOnly = true;

                    massPrps.Commit();

                    var vol = massPrps.Volume;
                    var moi = massPrps.MomentOfInertia;
                    var area = massPrps.SurfaceArea;
                    var dens = massPrps.Density;

                    mass1 = massPrps.Mass;
                    cog1 = massPrps.CenterOfGravity;
                    pmoi1 = massPrps.PrincipalMomentOfInertia;
                    pai1 = massPrps.PrincipalAxesOfInertia;
                }
            }

            AssertCompareDoubles(mass1, 25);
            AssertCompareDoubles(cog1.X, -10);
            AssertCompareDoubles(cog1.Y, -20);
            AssertCompareDoubles(cog1.Z, -30);
            AssertCompareDoubles(pmoi1.Px, 5000);
            AssertCompareDoubles(pmoi1.Py, 6000);
            AssertCompareDoubles(pmoi1.Pz, 7000);
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
        public void PrincipalAxesOfInertiaSubAssemblyOverriddenTest()
        {
            PrincipalAxesOfInertia pai1;
            PrincipalAxesOfInertia pai2;
            PrincipalAxesOfInertia pai3;

            using (var doc = OpenDataDocument(@"MassPrpsAssembly7\Assem1.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                using (var massPrps = assm.Evaluation.PreCreateMassProperty())
                {
                    massPrps.UserUnits = true;
                    massPrps.Scope = new IXComponent[] { assm.Configurations.Active.Components["SubSubAssem1-1"] };
                    massPrps.Commit();

                    pai1 = massPrps.PrincipalAxesOfInertia;

                    massPrps.Scope = new IXComponent[] { assm.Configurations.Active.Components["SubAssem2-1"] };
                    pai2 = massPrps.PrincipalAxesOfInertia;

                    massPrps.Scope = new IXComponent[] { assm.Configurations.Active.Components["SubAssem1-1"].Children["SubSubAssem1-1"] };
                    pai3 = massPrps.PrincipalAxesOfInertia;
                }
            }
            
            AssertCompareDoubles(pai1.Ix.X, -0.90443126);
            AssertCompareDoubles(pai1.Ix.Y, 0.22610782);
            AssertCompareDoubles(pai1.Ix.Z, 0.36177251);
            AssertCompareDoubles(pai1.Iy.X, 0.08716355);
            AssertCompareDoubles(pai1.Iy.Y, -0.73217386);
            AssertCompareDoubles(pai1.Iy.Z, 0.67551755);
            AssertCompareDoubles(pai1.Iz.X, 0.41762017);
            AssertCompareDoubles(pai1.Iz.Y, 0.64249257);
            AssertCompareDoubles(pai1.Iz.Z, 0.64249257);
            
            AssertCompareDoubles(pai2.Ix.X, -0.89442719);
            AssertCompareDoubles(pai2.Ix.Y, -0.44721360);
            AssertCompareDoubles(pai2.Ix.Z, 0.00000000);
            AssertCompareDoubles(pai2.Iy.X, 0.44721360);
            AssertCompareDoubles(pai2.Iy.Y, -0.89442719);
            AssertCompareDoubles(pai2.Iy.Z, 0.00000000);
            AssertCompareDoubles(pai2.Iz.X, 0.00000000);
            AssertCompareDoubles(pai2.Iz.Y, 0.00000000);
            AssertCompareDoubles(pai2.Iz.Z, 1.00000000);

            AssertCompareDoubles(pai3.Ix.X, -0.90443126);
            AssertCompareDoubles(pai3.Ix.Y, 0.22610782);
            AssertCompareDoubles(pai3.Ix.Z, 0.36177251);
            AssertCompareDoubles(pai3.Iy.X, 0.08716355);
            AssertCompareDoubles(pai3.Iy.Y, -0.73217386);
            AssertCompareDoubles(pai3.Iy.Z, 0.67551755);
            AssertCompareDoubles(pai3.Iz.X, 0.41762017);
            AssertCompareDoubles(pai3.Iz.Y, 0.64249257);
            AssertCompareDoubles(pai3.Iz.Z, 0.64249257);
        }

        [Test]
        public void MassPropertyEmptyComponentsTest()
        {
            using (var doc = OpenDataDocument(@"MassPrpsAssembly2\Assem1.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                using (var massPrps = assm.Evaluation.PreCreateMassProperty())
                {
                    massPrps.UserUnits = false;
                    massPrps.VisibleOnly = false;

                    massPrps.Scope = new IXComponent[] { assm.Configurations.Active.Components["Empty-1"] };
                    Assert.That(() => massPrps.Commit(), Throws.InstanceOf<EvaluationFailedException>());
                    Assert.DoesNotThrow(() =>
                    {
                        massPrps.Scope = new IXComponent[] { assm.Configurations.Active.Components["Part1-1"] };
                        massPrps.Commit();
                    });
                    Assert.That(() => massPrps.Scope = new IXComponent[] { assm.Configurations.Active.Components["Sketch-1"] }, Throws.InstanceOf<EvaluationFailedException>());
                    Assert.DoesNotThrow(() => massPrps.Scope = new IXComponent[] { assm.Configurations.Active.Components["Part1-1"] });
                    Assert.That(() => massPrps.Scope = new IXComponent[] { assm.Configurations.Active.Components["Surface-1"] }, Throws.InstanceOf<EvaluationFailedException>());
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

                using (var massPrps = assm.Evaluation.PreCreateMassProperty())
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

                using (var massPrps = part.Evaluation.PreCreateMassProperty())
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

                using (var massPrps = assm.Evaluation.PreCreateMassProperty())
                {
                    massPrps.VisibleOnly = true;

                    Assert.That(() =>
                    {
                        massPrps.Scope = new IXComponent[] { assm.Configurations.Active.Components["SubAssem1-1"] };
                        massPrps.Commit();
                    }, Throws.InstanceOf<EvaluationFailedException>());

                    Assert.That(() =>
                    {
                        massPrps.Scope = new IXComponent[] { assm.Configurations.Active.Components["Part2-1"] };
                        massPrps.Commit();
                    }, Throws.InstanceOf<EvaluationFailedException>());
                }
            }
        }

        [Test]
        public void MassPropertyPartOverrideTest()
        {
            object moi1;
            object moi2;
            object moi3;
            object moi4;
            object moi5;
            object moi6;
            object moi7;
            object moi8;
            object moi9;
            object moi10;
            object moi11;
            object moi12;
            object moi13;
            object moi14;
            object moi15;
            object moi16;

            object mass1;
            object mass2;
            object mass3;
            object mass4;
            object mass5;
            object mass6;
            object mass7;
            object mass8;
            object mass9;
            object mass10;
            object mass11;
            object mass12;
            object mass13;
            object mass14;
            object mass15;
            object mass16;

            object cog1;
            object cog2;
            object cog3;
            object cog4;
            object cog5;
            object cog6;
            object cog7;
            object cog8;
            object cog9;
            object cog10;
            object cog11;
            object cog12;
            object cog13;
            object cog14;
            object cog15;
            object cog16;

            object pmoi1;
            object pmoi2;
            object pmoi3;
            object pmoi4;
            object pmoi5;
            object pmoi6;
            object pmoi7;
            object pmoi8;
            object pmoi9;
            object pmoi10;
            object pmoi11;
            object pmoi12;
            object pmoi13;
            object pmoi14;
            object pmoi15;
            object pmoi16;

            object paoi1;
            object paoi2;
            object paoi3;
            object paoi4;
            object paoi5;
            object paoi6;
            object paoi7;
            object paoi8;
            object paoi9;
            object paoi10;
            object paoi11;
            object paoi12;
            object paoi13;
            object paoi14;
            object paoi15;
            object paoi16;

            using (var doc = OpenDataDocument(@"MassPrpsAssembly4\Assem1.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                GetMassPropertyArrayData(assm, "COG_Overridden-1", true, false, true, out moi1, out mass1, out cog1, out pmoi1, out paoi1, out _, out _, out _);
                GetMassPropertyArrayData(assm, "Mass_Overridden-2", true, false, true, out moi2, out mass2, out cog2, out pmoi2, out paoi2, out _, out _, out _);
                GetMassPropertyArrayData(assm, "PMOI_Overridden-1", true, false, true, out moi3, out mass3, out cog3, out pmoi3, out paoi3, out _, out _, out _);
                GetMassPropertyArrayData(assm, "None_Overridden-1", true, false, true, out moi4, out mass4, out cog4, out pmoi4, out paoi4, out _, out _, out _);

                GetMassPropertyArrayData(assm, "COG_Overridden-1", false, false, true, out moi5, out mass5, out cog5, out pmoi5, out paoi5, out _, out _, out _);
                GetMassPropertyArrayData(assm, "Mass_Overridden-2", false, false, true, out moi6, out mass6, out cog6, out pmoi6, out paoi6, out _, out _, out _);
                GetMassPropertyArrayData(assm, "PMOI_Overridden-1", false, false, true, out moi7, out mass7, out cog7, out pmoi7, out paoi7, out _, out _, out _);
                GetMassPropertyArrayData(assm, "None_Overridden-1", false, false, true, out moi8, out mass8, out cog8, out pmoi8, out paoi8, out _, out _, out _);

                GetMassPropertyArrayData(assm, "COG_Overridden-1", false, true, true, out moi9, out mass9, out cog9, out pmoi9, out paoi9, out _, out _, out _);
                GetMassPropertyArrayData(assm, "Mass_Overridden-2", false, true, true, out moi10, out mass10, out cog10, out pmoi10, out paoi10, out _, out _, out _);
                GetMassPropertyArrayData(assm, "PMOI_Overridden-1", false, true, true, out moi11, out mass11, out cog11, out pmoi11, out paoi11, out _, out _, out _);
                GetMassPropertyArrayData(assm, "None_Overridden-1", false, true, true, out moi12, out mass12, out cog12, out pmoi12, out paoi12, out _, out _, out _);

                GetMassPropertyArrayData(assm, "COG_Overridden-1", true, true, true, out moi13, out mass13, out cog13, out pmoi13, out paoi13, out _, out _, out _);
                GetMassPropertyArrayData(assm, "Mass_Overridden-2", true, true, true, out moi14, out mass14, out cog14, out pmoi14, out paoi14, out _, out _, out _);
                GetMassPropertyArrayData(assm, "PMOI_Overridden-1", true, true, true, out moi15, out mass15, out cog15, out pmoi15, out paoi15, out _, out _, out _);
                GetMassPropertyArrayData(assm, "None_Overridden-1", true, true, true, out moi16, out mass16, out cog16, out pmoi16, out paoi16, out _, out _, out _);
            }

            AssertCompareDoubleArray((double[])moi1, new double[] { 732771.57070537, 207033.34471190, 284753.25397601, 207033.34471190, 1016744.10068294, 283072.44302186, 284753.25397601, 283072.44302186, 1170200.96290798 });
            if (m_App.IsVersionNewerOrEqual(Xarial.XCad.SolidWorks.Enums.SwVersion_e.Sw2020))
            {
                AssertCompareDoubleArray((double[])moi2, new double[] { 37977.36013703, -9549.44652330, -4965.19161883, -9549.44652330, 17440.37185897, 1988.87607517, -4965.19161883, 1988.87607517, 35024.94118039 });
                AssertCompareDoubleArray((double[])moi3, new double[] { 1054.58171345, -84.46972878, -210.87298292, -84.46972878, 1130.72391154, -326.34343170, -210.87298292, -326.34343170, 1814.69437501 });
            }
            else
            {
                Assert.IsInstanceOf<MomentOfInertiaOverridenException>(moi2);
                Assert.IsInstanceOf<MomentOfInertiaOverridenException>(moi3);
            }
            AssertCompareDoubleArray((double[])moi4, new double[] { 953380.76757384, 221739.33836451, 355898.35534423, 221739.33836451, 1257823.94327966, 42062.10100616, 355898.35534423, 42062.10100616, 708511.92344279 });

            AssertCompareDoubleArray((double[])moi5, new double[] { 12582.55584223, 250.22092804, 148.85064148, 250.22092804, 21375.67770367, -5235.05710985, 148.85064148, -5235.05710985, 15689.65556923 });
            if (m_App.IsVersionNewerOrEqual(Xarial.XCad.SolidWorks.Enums.SwVersion_e.Sw2020))
            {
                AssertCompareDoubleArray((double[])moi6, new double[] { 3207.83128782, -70.58035199, 50.22342169, -70.58035199, 5220.98393895, 1434.27367730, 50.22342169, 1434.27367730, 4225.95731532 });
                AssertCompareDoubleArray((double[])moi7, new double[] { 1054.58171345, -84.46972878, -210.87298292, -84.46972878, 1130.72391154, -326.34343170, -210.87298292, -326.34343170, 1814.69437501 });
            }
            else
            {
                Assert.IsInstanceOf<MomentOfInertiaOverridenException>(moi6);
                Assert.IsInstanceOf<MomentOfInertiaOverridenException>(moi7);
            }
            AssertCompareDoubleArray((double[])moi8, new double[] { 20444.14131853, -5636.36285550, -349.56564420, -5636.36285550, 16612.77720078, -250.39444679, -349.56564420, -250.39444679, 12590.97059582 });

            if (m_App.IsVersionNewerOrEqual(Xarial.XCad.SolidWorks.Enums.SwVersion_e.Sw2020))
            {
                AssertCompareDoubleArray((double[])moi9, new double[] { 2055493.59519249, 1473733.62357906, 209311.83240675, 1473733.62357906, 1144281.88776368, 284502.22693767, 209311.83240675, 284502.22693767, 3094248.38970080 });
                AssertCompareDoubleArray((double[])moi10, new double[] { 768793.19616025, 104754.95861837, 314872.85703145, 104754.95861837, 834722.70323284, 227762.09556753, 314872.85703145, 227762.09556753, 223073.39439345 });
                AssertCompareDoubleArray((double[])moi11, new double[] { 3378287.78275181, 10966.98973875, 9093.51593862, 10966.98973875, 1318697.38761996, 1647362.86006517, 9093.51593862, 1647362.86006517, 2061484.22500662 });
            }
            else
            {
                Assert.IsInstanceOf<MomentOfInertiaOverridenException>(moi9);
                Assert.IsInstanceOf<MomentOfInertiaOverridenException>(moi10);
                Assert.IsInstanceOf<MomentOfInertiaOverridenException>(moi11);
            }
            AssertCompareDoubleArray((double[])moi12, new double[] { 14712236.09617482, 4987667.80184248, 3855648.81964258, 4987667.80184248, 8226946.56763011, 7118485.74938377, 3855648.81964258, 7118485.74938377, 11915270.93498221 });

            if (m_App.IsVersionNewerOrEqual(Xarial.XCad.SolidWorks.Enums.SwVersion_e.Sw2020))
            {
                AssertCompareDoubleArray((double[])moi13, new double[] { 18169538.72272953, 11995399.31901705, 1683580.55358953, 11995399.31901705, 10041778.53972001, 1993318.14300060, 1683580.55358953, 1993318.14300060, 26090270.31616359 });
                AssertCompareDoubleArray((double[])moi14, new double[] { 429678.07221692, -53952.39135083, 328772.98457456, -53952.39135083, 700029.78915353, -43384.18746203, 328772.98457456, -43384.18746203, 325063.83301430 });
                AssertCompareDoubleArray((double[])moi15, new double[] { 18613926.38640416, 5871368.65378197, 6995931.98530180, 5871368.65378197, 15403813.71193740, 9165628.99074609, 6995931.98530180, 9165628.99074609, 12175445.88721939 });
            }
            else
            {
                Assert.IsInstanceOf<MomentOfInertiaOverridenException>(moi13);
                Assert.IsInstanceOf<MomentOfInertiaOverridenException>(moi14);
                Assert.IsInstanceOf<MomentOfInertiaOverridenException>(moi15);
            }
            AssertCompareDoubleArray((double[])moi16, new double[] { 110981291.63683793, 23338999.87521937, 25883931.20133131, 23338999.87521937, 73306473.61240853, 54171849.01522268, 25883931.20133131, 54171849.01522268, 60433968.38786618 });

            AssertCompareDoubles(322.82511471125844, (double)mass1);
            AssertCompareDoubles(10, (double)mass2);
            AssertCompareDoubles(322.82511471125844, (double)mass3);
            AssertCompareDoubles(322.82511471125844, (double)mass4);

            AssertCompareDoubles(39.2325416754816, (double)mass5);
            AssertCompareDoubles(10, (double)mass6);
            AssertCompareDoubles(39.2325416754816, (double)mass7);
            AssertCompareDoubles(39.2325416754816, (double)mass8);
            
            AssertCompareDoubles(39.2325416754816, (double)mass9);
            AssertCompareDoubles(10, (double)mass10);
            AssertCompareDoubles(39.2325416754816, (double)mass11);
            AssertCompareDoubles(39.2325416754816, (double)mass12);
            
            AssertCompareDoubles(322.82511471125844, (double)mass13);
            AssertCompareDoubles(10, (double)mass14);
            AssertCompareDoubles(322.82511471125844, (double)mass15);
            AssertCompareDoubles(322.82511471125844, (double)mass16);

            AssertCompareDoubles(322.82511471125844, (double)mass13);
            AssertCompareDoubles(10, (double)mass14);
            AssertCompareDoubles(322.82511471125844, (double)mass15);
            AssertCompareDoubles(322.82511471125844, (double)mass16);

            AssertCompareDoubleArray((double[])cog1, new double[] { 76.82290986, -0.07532937, -278.77174281 });
            AssertCompareDoubleArray((double[])cog2, new double[] { -154.08110264, -113.14142480, 92.49402618 });
            AssertCompareDoubleArray((double[])cog3, new double[] { -139.23655151, 45.30273096, 193.49523334 });
            AssertCompareDoubleArray((double[])cog4, new double[] { -391.50242124, 152.80074381, 410.94934022 });

            AssertCompareDoubleArray((double[])cog5, new double[] { 76.82290986, -0.07532937, -278.77174281 });
            AssertCompareDoubleArray((double[])cog6, new double[] { -217.44164968, -1.76969527, 145.08869196 });
            AssertCompareDoubleArray((double[])cog7, new double[] { -138.52048542, 181.89475797, 170.54909351 });
            AssertCompareDoubleArray((double[])cog8, new double[] { -329.99239780, 159.62894541, 534.86186021 });

            AssertCompareDoubleArray((double[])cog9, new double[] { -165.93855976, -225.86082420, -32.13831180 });
            AssertCompareDoubleArray((double[])cog10, new double[] { 120.15541293, 86.88632259, 262.12624774 });
            AssertCompareDoubleArray((double[])cog11, new double[] { 1.25415420, 229.16546409, 183.20508347 });
            AssertCompareDoubleArray((double[])cog12, new double[] { 262.56537709, 483.99090078, 374.67699586 });

            AssertCompareDoubleArray((double[])cog13, new double[] { -165.93855976, -225.86082420, -32.13831180 });
            AssertCompareDoubleArray((double[])cog14, new double[] { 167.28644176, -26.90527256, 198.76570069 });
            AssertCompareDoubleArray((double[])cog15, new double[] { 117.82614748, 154.36654994, 183.92114956 });
            AssertCompareDoubleArray((double[])cog16, new double[] { 184.35125704, 387.64004000, 436.18701929 });

            AssertCompareDoubleArray((double[])pmoi1, new double[] { 421570.35749821, 1090708.81606145, 1407437.46073663 });
            AssertCompareDoubleArray((double[])pmoi3, new double[] { 1000.00000000, 1000.00000000, 2000.00000000 });
            AssertCompareDoubleArray((double[])pmoi4, new double[] { 421570.35749821, 1090708.81606145, 1407437.46073663 });

            if (m_App.IsVersionNewerOrEqual(Xarial.XCad.SolidWorks.Enums.SwVersion_e.Sw2020))
            {
                AssertCompareDoubleArray((double[])pmoi2, new double[] { 13058.78440949, 33786.36810946, 43597.52065745 });
                AssertCompareDoubleArray((double[])pmoi6, new double[] { 3205.35980219, 3205.35980219, 6244.05293771 });
                AssertCompareDoubleArray((double[])pmoi10, new double[] { 3205.35980219, 3205.35980219, 6244.05293771 });
                AssertCompareDoubleArray((double[])pmoi14, new double[] { 13058.78440949, 33786.36810946, 43597.52065745 });
            }
            else
            {
                Assert.IsInstanceOf<PrincipalMomentOfInertiaOverridenException>(pmoi2);
                Assert.IsInstanceOf<PrincipalMomentOfInertiaOverridenException>(pmoi6);
                Assert.IsInstanceOf<PrincipalMomentOfInertiaOverridenException>(pmoi10);
                Assert.IsInstanceOf<PrincipalMomentOfInertiaOverridenException>(pmoi14);
            }

            AssertCompareDoubleArray((double[])pmoi5, new double[] { 12575.44120243, 12575.44120243, 24497.00671027 });
            AssertCompareDoubleArray((double[])pmoi7, new double[] { 1000.00000000, 1000.00000000, 2000.00000000 });
            AssertCompareDoubleArray((double[])pmoi8, new double[] { 12575.44120243, 12575.44120243, 24497.00671027 });

            AssertCompareDoubleArray((double[])pmoi9, new double[]  { 12575.44120243, 12575.44120243, 24497.00671027 });
            AssertCompareDoubleArray((double[])pmoi11, new double[] { 1000.00000000, 1000.00000000, 2000.00000000 });
            AssertCompareDoubleArray((double[])pmoi12, new double[] { 12575.44120243, 12575.44120243, 24497.00671027 });

            AssertCompareDoubleArray((double[])pmoi13, new double[] { 421570.35749821, 1090708.81606145, 1407437.46073663 });
            AssertCompareDoubleArray((double[])pmoi15, new double[] { 1000.00000000, 1000.00000000, 2000.00000000 });
            AssertCompareDoubleArray((double[])pmoi16, new double[] { 421570.35749821, 1090708.81606145, 1407437.46073663 });
            
            AssertCompareDoubleArray((double[])paoi1, new double[] { 0.74428291, 0.47984159, 0.46453740, -0.63704755, 0.71891902, 0.27807527, -0.20053269, -0.50289908, 0.84076106 });
            AssertCompareDoubleArray((double[])paoi2, new double[] { -0.38183860, 0.90872286, 0.16858839, -0.34497319, -0.30935881, 0.88616625, 0.85743384, 0.28021401, 0.43161016 });
            AssertCompareDoubleArray((double[])paoi3, new double[] { 0.54453395, 0.46999645, 0.69468418, 0.67935084, -0.73289707, -0.03666497, 0.49189960, 0.49189960, -0.71837982 });
            AssertCompareDoubleArray((double[])paoi4, new double[] { 0.60088528, 0.19827816, 0.77435306, 0.55652280, 0.59160768, -0.58333758, -0.57377631, 0.78146409, 0.24514203 });

            AssertCompareDoubleArray((double[])paoi5, new double[] { 0.65641077, -0.37181957, 0.65641077, 0.75400803, 0.35152849, -0.55488703, -0.02442923, 0.85917282, 0.51110203 }, 7);
            AssertCompareDoubleArray((double[])paoi6, new double[] { 0.63787041, 0.43155844, 0.63787041, 0.76961549, -0.38786325, -0.50720222, 0.02851909, 0.81444423, -0.57954055 });
            AssertCompareDoubleArray((double[])paoi7, new double[] { 0.54453395, 0.46999645, 0.69468418, 0.67935084, -0.73289707, -0.03666497, 0.49189960, 0.49189960, -0.71837982 });
            AssertCompareDoubleArray((double[])paoi8, new double[] { -0.47372654, 0.62272914, 0.62272914, 0.33991764, -0.52302028, 0.78160462, 0.81242794, 0.58194347, 0.03609197 });

            AssertCompareDoubleArray((double[])paoi9, new double[] { 0.71646322, 0.23623155, -0.65641077, -0.63321170, -0.17468496, -0.75400803, -0.29278558, 0.95586600, 0.02442923 });
            AssertCompareDoubleArray((double[])paoi10, new double[] { 0.10906999, 0.76238119, -0.63787041, -0.05361748, -0.63625243, -0.76961549, -0.99258697, 0.11814296, -0.02851909 });
            AssertCompareDoubleArray((double[])paoi11, new double[] { 0.11878463, 0.83028488, -0.54453395, 0.51810061, -0.51966739, -0.67935084, -0.84703127, -0.20142694, -0.49189960 });
            AssertCompareDoubleArray((double[])paoi12, new double[] { -0.04267474, 0.87963744, 0.47372654, 0.91256521, 0.22733400, -0.33991764, -0.40669843, 0.41780046, -0.81242794 });

            AssertCompareDoubleArray((double[])paoi13, new double[] { -0.04316746, 0.66646794, -0.74428291, -0.34551872, 0.68904806, 0.63704755, 0.93741847, 0.28466341, 0.20053269 });
            AssertCompareDoubleArray((double[])paoi14, new double[] { -0.55965261, 0.73551903, 0.38183860, 0.82460690, 0.44834914, 0.34497319, 0.08253734, 0.50793189, -0.85743384 });
            AssertCompareDoubleArray((double[])paoi15, new double[] { 0.11878463, 0.83028488, -0.54453395, 0.51810061, -0.51966739, -0.67935084, -0.84703127, -0.20142694, -0.49189960 });
            AssertCompareDoubleArray((double[])paoi16, new double[] { 0.37354142, 0.70668500, -0.60088528, -0.83011915, -0.03441769, -0.55652280, -0.41396740, 0.70669069, 0.57377631 });
        }
        
        [Test]
        public void MassPropertySubAssemblyPartOverrideTest()
        {
            object moi1;
            object moi2;
            object moi3;
            object moi4;
            object moi5;
            object moi6;
            object moi7;
            object moi8;
            object moi9;
            object moi10;
            object moi11;
            object moi12;
            object moi13;
            object moi14;
            object moi15;
            object moi16;

            object mass1;
            object mass2;
            object mass3;
            object mass4;
            object mass5;
            object mass6;
            object mass7;
            object mass8;
            object mass9;
            object mass10;
            object mass11;
            object mass12;
            object mass13;
            object mass14;
            object mass15;
            object mass16;

            object cog1;
            object cog2;
            object cog3;
            object cog4;
            object cog5;
            object cog6;
            object cog7;
            object cog8;
            object cog9;
            object cog10;
            object cog11;
            object cog12;
            object cog13;
            object cog14;
            object cog15;
            object cog16;

            object pmoi1;
            object pmoi2;
            object pmoi3;
            object pmoi4;
            object pmoi5;
            object pmoi6;
            object pmoi7;
            object pmoi8;
            object pmoi9;
            object pmoi10;
            object pmoi11;
            object pmoi12;
            object pmoi13;
            object pmoi14;
            object pmoi15;
            object pmoi16;

            object paoi1;
            object paoi2;
            object paoi3;
            object paoi4;
            object paoi5;
            object paoi6;
            object paoi7;
            object paoi8;
            object paoi9;
            object paoi10;
            object paoi11;
            object paoi12;
            object paoi13;
            object paoi14;
            object paoi15;
            object paoi16;

            using (var doc = OpenDataDocument(@"MassPrpsAssembly5\Assem1.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                GetMassPropertyArrayData(assm, "COG_Overridden_Assm-1", true, false, true, out moi1, out mass1, out cog1, out pmoi1, out paoi1, out _, out _, out _);
                GetMassPropertyArrayData(assm, "Mass_Overridden_Assm-2", true, false, true, out moi2, out mass2, out cog2, out pmoi2, out paoi2, out _, out _, out _);
                GetMassPropertyArrayData(assm, "PMOI_Overridden_Assm-1", true, false, true, out moi3, out mass3, out cog3, out pmoi3, out paoi3, out _, out _, out _);
                GetMassPropertyArrayData(assm, "None_Overridden_Assm-2", true, false, true, out moi4, out mass4, out cog4, out pmoi4, out paoi4, out _, out _, out _);

                GetMassPropertyArrayData(assm, "COG_Overridden_Assm-1", false, false, true, out moi5, out mass5, out cog5, out pmoi5, out paoi5, out _, out _, out _);
                GetMassPropertyArrayData(assm, "Mass_Overridden_Assm-2", false, false, true, out moi6, out mass6, out cog6, out pmoi6, out paoi6, out _, out _, out _);
                GetMassPropertyArrayData(assm, "PMOI_Overridden_Assm-1", false, false, true, out moi7, out mass7, out cog7, out pmoi7, out paoi7, out _, out _, out _);
                GetMassPropertyArrayData(assm, "None_Overridden_Assm-2", false, false, true, out moi8, out mass8, out cog8, out pmoi8, out paoi8, out _, out _, out _);

                GetMassPropertyArrayData(assm, "COG_Overridden_Assm-1", false, true, true, out moi9, out mass9, out cog9, out pmoi9, out paoi9, out _, out _, out _);
                GetMassPropertyArrayData(assm, "Mass_Overridden_Assm-2", false, true, true, out moi10, out mass10, out cog10, out pmoi10, out paoi10, out _, out _, out _);
                GetMassPropertyArrayData(assm, "PMOI_Overridden_Assm-1", false, true, true, out moi11, out mass11, out cog11, out pmoi11, out paoi11, out _, out _, out _);
                GetMassPropertyArrayData(assm, "None_Overridden_Assm-2", false, true, true, out moi12, out mass12, out cog12, out pmoi12, out paoi12, out _, out _, out _);

                GetMassPropertyArrayData(assm, "COG_Overridden_Assm-1", true, true, true, out moi13, out mass13, out cog13, out pmoi13, out paoi13, out _, out _, out _);
                GetMassPropertyArrayData(assm, "Mass_Overridden_Assm-2", true, true, true, out moi14, out mass14, out cog14, out pmoi14, out paoi14, out _, out _, out _);
                GetMassPropertyArrayData(assm, "PMOI_Overridden_Assm-1", true, true, true, out moi15, out mass15, out cog15, out pmoi15, out paoi15, out _, out _, out _);
                GetMassPropertyArrayData(assm, "None_Overridden_Assm-2", true, true, true, out moi16, out mass16, out cog16, out pmoi16, out paoi16, out _, out _, out _);
            }

            AssertCompareDoubleArray((double[])moi1, new double[] { 732771.57070537, 207033.34471190, 284753.25397601, 207033.34471190, 1016744.10068294, 283072.44302186, 284753.25397601, 283072.44302186, 1170200.96290798 });
            AssertCompareDoubleArray((double[])moi2, new double[] { 37977.36013703, -9549.44652330, -4965.19161883, -9549.44652330, 17440.37185897, 1988.87607517, -4965.19161883, 1988.87607517, 35024.94118039 });
            AssertCompareDoubleArray((double[])moi3, new double[] { 1054.58171345, -84.46972878, -210.87298292, -84.46972878, 1130.72391154, -326.34343170, -210.87298292, -326.34343170, 1814.69437501 });
            AssertCompareDoubleArray((double[])moi4, new double[] { 953380.76757384, 221739.33836451, 355898.35534423, 221739.33836451, 1257823.94327966, 42062.10100616, 355898.35534423, 42062.10100616, 708511.92344279 });

            if (m_App.IsVersionNewerOrEqual(Xarial.XCad.SolidWorks.Enums.SwVersion_e.Sw2020))
            {
                AssertCompareDoubleArray((double[])moi5, new double[] { 12582.55584223, 250.22092804, 148.85064148, 250.22092804, 21375.67770367, -5235.05710985, 148.85064148, -5235.05710985, 15689.65556923 });
                AssertCompareDoubleArray((double[])moi6, new double[] { 3207.83128782, -70.58035199, 50.22342169, -70.58035199, 5220.98393895, 1434.27367730, 50.22342169, 1434.27367730, 4225.95731532 });
                AssertCompareDoubleArray((double[])moi7, new double[] { 1054.58171345, -84.46972878, -210.87298292, -84.46972878, 1130.72391154, -326.34343170, -210.87298292, -326.34343170, 1814.69437501 });
                AssertCompareDoubleArray((double[])moi8, new double[] { 20444.14131853, -5636.36285550, -349.56564420, -5636.36285550, 16612.77720078, -250.39444679, -349.56564420, -250.39444679, 12590.97059582 });

                AssertCompareDoubleArray((double[])moi9, new double[] { 2055493.59519249, 1473733.62357906, 209311.83240675, 1473733.62357906, 1144281.88776368, 284502.22693767, 209311.83240675, 284502.22693767, 3094248.38970080 });
                AssertCompareDoubleArray((double[])moi10, new double[] { 768793.19616025, 104754.95861837, 314872.85703145, 104754.95861837, 834722.70323284, 227762.09556753, 314872.85703145, 227762.09556753, 223073.39439345 });
                AssertCompareDoubleArray((double[])moi11, new double[] { 3378287.78275181, 10966.98973875, 9093.51593862, 10966.98973875, 1318697.38761996, 1647362.86006517, 9093.51593862, 1647362.86006517, 2061484.22500662 });
                AssertCompareDoubleArray((double[])moi12, new double[] { 14712236.09617482, 4987667.80184248, 3855648.81964258, 4987667.80184248, 8226946.56763011, 7118485.74938377, 3855648.81964258, 7118485.74938377, 11915270.93498221 });
            }
            else
            {
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(moi5);
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(moi6);
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(moi7);
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(moi8);

                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(moi9);
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(moi10);
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(moi11);
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(moi12);
            }

            AssertCompareDoubleArray((double[])moi13, new double[] { 18169538.72272953, 11995399.31901705, 1683580.55358953, 11995399.31901705, 10041778.53972001, 1993318.14300060, 1683580.55358953, 1993318.14300060, 26090270.31616359 });
            AssertCompareDoubleArray((double[])moi14, new double[] { 789955.12562743, 95455.10146005, 311223.79181517, 95455.10146005, 856579.14555075, 237846.12328317, 311223.79181517, 237846.12328317, 257842.92324266 });//note this is calculated differently to previous 2 tests
            AssertCompareDoubleArray((double[])moi15, new double[] { 18613926.38640416, 5871368.65378197, 6995931.98530180, 5871368.65378197, 15403813.71193740, 9165628.99074609, 6995931.98530180, 9165628.99074609, 12175445.88721939 });
            AssertCompareDoubleArray((double[])moi16, new double[] { 110981291.63683793, 23338999.87521937, 25883931.20133131, 23338999.87521937, 73306473.61240853, 54171849.01522268, 25883931.20133131, 54171849.01522268, 60433968.38786618 });

            AssertCompareDoubles(322.82511471125844, (double)mass1);
            AssertCompareDoubles(10, (double)mass2);
            AssertCompareDoubles(322.82511471125844, (double)mass3);
            AssertCompareDoubles(322.82511471125844, (double)mass4);

            if (m_App.IsVersionNewerOrEqual(Xarial.XCad.SolidWorks.Enums.SwVersion_e.Sw2020))
            {
                AssertCompareDoubles(39.2325416754816, (double)mass5);
                AssertCompareDoubles(10, (double)mass6);
                AssertCompareDoubles(39.2325416754816, (double)mass7);
                AssertCompareDoubles(39.2325416754816, (double)mass8);

                AssertCompareDoubles(39.2325416754816, (double)mass9);
                AssertCompareDoubles(10, (double)mass10);
                AssertCompareDoubles(39.2325416754816, (double)mass11);
                AssertCompareDoubles(39.2325416754816, (double)mass12);
            }
            else
            {
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(mass5);
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(mass6);
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(mass7);
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(mass8);
                                                                                     
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(mass9);
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(mass10);
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(mass11);
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(mass12);
            }

            AssertCompareDoubles(322.82511471125844, (double)mass13);
            AssertCompareDoubles(10, (double)mass14);
            AssertCompareDoubles(322.82511471125844, (double)mass15);
            AssertCompareDoubles(322.82511471125844, (double)mass16);

            AssertCompareDoubleArray((double[])cog1, new double[] { 76.82290986, -0.07532937, -278.77174281 });
            AssertCompareDoubleArray((double[])cog2, new double[] { -217.44164968, -1.76969527, 145.08869196 });
            AssertCompareDoubleArray((double[])cog3, new double[] { -139.23655151, 45.30273096, 193.49523334 });
            AssertCompareDoubleArray((double[])cog4, new double[] { -391.50242124, 152.80074381, 410.94934022 });
            
            if (m_App.IsVersionNewerOrEqual(Xarial.XCad.SolidWorks.Enums.SwVersion_e.Sw2020))
            {
                AssertCompareDoubleArray((double[])cog5, new double[] { 76.82290986, -0.07532937, -278.77174281 });
                AssertCompareDoubleArray((double[])cog6, new double[] { -217.44164968, -1.76969527, 145.08869196 });
                AssertCompareDoubleArray((double[])cog7, new double[] { -138.52048542, 181.89475797, 170.54909351 });
                AssertCompareDoubleArray((double[])cog8, new double[] { -329.99239780, 159.62894541, 534.86186021 });

                AssertCompareDoubleArray((double[])cog9, new double[] { -165.93855976, -225.86082420, -32.13831180 });
                AssertCompareDoubleArray((double[])cog10, new double[] { 120.15541293, 86.88632259, 262.12624774 });
                AssertCompareDoubleArray((double[])cog11, new double[] { 1.25415420, 229.16546409, 183.20508347 });
                AssertCompareDoubleArray((double[])cog12, new double[] { 262.56537709, 483.99090078, 374.67699586 });
            }
            else
            {
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(cog5);
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(cog6);
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(cog7);
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(cog8);
                                                                                     
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(cog9);
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(cog10);
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(cog11);
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(cog12);
            }

            AssertCompareDoubleArray((double[])cog13, new double[] { -165.93855976, -225.86082420, -32.13831180 });
            AssertCompareDoubleArray((double[])cog14, new double[] { 120.15541293, 86.88632259, 262.12624774 });
            AssertCompareDoubleArray((double[])cog15, new double[] { 117.82614748, 154.36654994, 183.92114956 });
            AssertCompareDoubleArray((double[])cog16, new double[] { 184.35125704, 387.64004000, 436.18701929 });

            AssertCompareDoubleArray((double[])pmoi1, new double[] { 421570.35749821, 1090708.81606145, 1407437.46073663 });
            AssertCompareDoubleArray((double[])pmoi2, new double[] { 13058.78440949, 33786.36810946, 43597.52065745 });
            AssertCompareDoubleArray((double[])pmoi3, new double[] { 1000.00000000, 1000.00000000, 2000.00000000 });
            AssertCompareDoubleArray((double[])pmoi4, new double[] { 421570.35749821, 1090708.81606145, 1407437.46073663 });

            if (m_App.IsVersionNewerOrEqual(Xarial.XCad.SolidWorks.Enums.SwVersion_e.Sw2020))
            {
                AssertCompareDoubleArray((double[])pmoi5, new double[] { 12575.44120243, 12575.44120243, 24497.00671027 });
                AssertCompareDoubleArray((double[])pmoi6, new double[] { 3205.35980219, 3205.35980219, 6244.05293771 });
                AssertCompareDoubleArray((double[])pmoi7, new double[] { 1000.00000000, 1000.00000000, 2000.00000000 });
                AssertCompareDoubleArray((double[])pmoi8, new double[] { 12575.44120243, 12575.44120243, 24497.00671027 });

                AssertCompareDoubleArray((double[])pmoi9, new double[] { 12575.44120243, 12575.44120243, 24497.00671027 });
                AssertCompareDoubleArray((double[])pmoi10, new double[] { 3205.35980219, 3205.35980219, 6244.05293771 });
                AssertCompareDoubleArray((double[])pmoi11, new double[] { 1000.00000000, 1000.00000000, 2000.00000000 });
                AssertCompareDoubleArray((double[])pmoi12, new double[] { 12575.44120243, 12575.44120243, 24497.00671027 });
            }
            else 
            {
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(pmoi5);
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(pmoi6);
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(pmoi7);
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(pmoi8);
                                                                                     
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(pmoi9);
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(pmoi10);
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(pmoi11);
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(pmoi12);
            }

            AssertCompareDoubleArray((double[])pmoi13, new double[] { 421570.35749821, 1090708.81606145, 1407437.46073663 });
            AssertCompareDoubleArray((double[])pmoi14, new double[] { 13058.78440949, 33786.36810946, 43597.52065745 });
            AssertCompareDoubleArray((double[])pmoi15, new double[] { 1000.00000000, 1000.00000000, 2000.00000000 });
            AssertCompareDoubleArray((double[])pmoi16, new double[] { 421570.35749821, 1090708.81606145, 1407437.46073663 });
            
            AssertCompareDoubleArray((double[])paoi1, new double[] { 0.74428291, 0.47984159, 0.46453740, -0.63704755, 0.71891902, 0.27807527, -0.20053269, -0.50289908, 0.84076106 });//-
            AssertCompareDoubleArray((double[])paoi2, new double[] { -0.38183860, 0.90872286, 0.16858839, -0.34497319, -0.30935881, 0.88616625, 0.85743384, 0.28021401, 0.43161016 });//-
            AssertCompareDoubleArray((double[])paoi3, new double[] { 0.64086508, 0.64086508, -0.42259189, -0.73123884, 0.67717652, -0.08198608, 0.23362730, 0.36155762, 0.90260422 });
            AssertCompareDoubleArray((double[])paoi4, new double[] { 0.60088528, 0.19827816, 0.77435306, 0.55652280, 0.59160768, -0.58333758, -0.57377631, 0.78146409, 0.24514203 });

            if (m_App.IsVersionNewerOrEqual(Xarial.XCad.SolidWorks.Enums.SwVersion_e.Sw2020))
            {
                AssertCompareDoubleArray((double[])paoi5, new double[] { 0.65641077, -0.37181957, 0.65641077, 0.75400803, 0.35152849, -0.55488703, -0.02442923, 0.85917282, 0.51110203 }, 7);
                AssertCompareDoubleArray((double[])paoi6, new double[] { 0.63787041, 0.43155844, 0.63787041, 0.76961549, -0.38786325, -0.50720222, 0.02851909, 0.81444423, -0.57954055 });
                AssertCompareDoubleArray((double[])paoi7, new double[] { 0.64086508, 0.64086508, -0.42259189, -0.73123884, 0.67717652, -0.08198608, 0.23362730, 0.36155762, 0.90260422 });
                AssertCompareDoubleArray((double[])paoi8, new double[] { -0.47372654, 0.62272914, 0.62272914, 0.33991764, -0.52302028, 0.78160462, 0.81242794, 0.58194347, 0.03609197 });

                AssertCompareDoubleArray((double[])paoi9, new double[] { 0.71646322, 0.23623155, -0.65641077, -0.63321170, -0.17468496, -0.75400803, -0.29278558, 0.95586600, 0.02442923 });
                AssertCompareDoubleArray((double[])paoi10, new double[] { 0.10906999, 0.76238119, -0.63787041, -0.05361748, -0.63625243, -0.76961549, -0.99258697, 0.11814296, -0.02851909 });
                AssertCompareDoubleArray((double[])paoi11, new double[] { -0.75857324, 0.11772254, -0.64086508, -0.55657219, 0.39435663, 0.73123884, 0.33881268, 0.91138590, -0.23362730 });
                AssertCompareDoubleArray((double[])paoi12, new double[] { -0.04267474, 0.87963744, 0.47372654, 0.91256521, 0.22733400, -0.33991764, -0.40669843, 0.41780046, -0.81242794 });
            }
            else
            {
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(paoi5);
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(paoi6);
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(paoi7);
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(paoi8);
                                                                                     
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(paoi9);
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(paoi10);
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(paoi11);
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(paoi12);
            }

            AssertCompareDoubleArray((double[])paoi13, new double[] { -0.04316746, 0.66646794, -0.74428291, -0.34551872, 0.68904806, 0.63704755, 0.93741847, 0.28466341, 0.20053269 });
            AssertCompareDoubleArray((double[])paoi14, new double[] { -0.55965261, 0.73551903, 0.38183860, 0.82460690, 0.44834914, 0.34497319, 0.08253734, 0.50793189, -0.85743384 });
            AssertCompareDoubleArray((double[])paoi15, new double[] { -0.75857324, 0.11772254, -0.64086508, -0.55657219, 0.39435663, 0.73123884, 0.33881268, 0.91138590, -0.23362730 });
            AssertCompareDoubleArray((double[])paoi16, new double[] { 0.37354142, 0.70668500, -0.60088528, -0.83011915, -0.03441769, -0.55652280, -0.41396740, 0.70669069, 0.57377631 });
        }

        [Test]
        public void MassPropertySubAssemblyOverrideTest()
        {
            object moi1;
            object moi2;
            object moi3;
            object moi4;
            object moi5;
            object moi6;
            object moi7;
            object moi8;
            object moi9;
            object moi10;
            object moi11;
            object moi12;
            object moi13;
            object moi14;
            object moi15;
            object moi16;

            object mass1;
            object mass2;
            object mass3;
            object mass4;
            object mass5;
            object mass6;
            object mass7;
            object mass8;
            object mass9;
            object mass10;
            object mass11;
            object mass12;
            object mass13;
            object mass14;
            object mass15;
            object mass16;

            object cog1;
            object cog2;
            object cog3;
            object cog4;
            object cog5;
            object cog6;
            object cog7;
            object cog8;
            object cog9;
            object cog10;
            object cog11;
            object cog12;
            object cog13;
            object cog14;
            object cog15;
            object cog16;

            object pmoi1;
            object pmoi2;
            object pmoi3;
            object pmoi4;
            object pmoi5;
            object pmoi6;
            object pmoi7;
            object pmoi8;
            object pmoi9;
            object pmoi10;
            object pmoi11;
            object pmoi12;
            object pmoi13;
            object pmoi14;
            object pmoi15;
            object pmoi16;

            object paoi1;
            object paoi2;
            object paoi3;
            object paoi4;
            object paoi5;
            object paoi6;
            object paoi7;
            object paoi8;
            object paoi9;
            object paoi10;
            object paoi11;
            object paoi12;
            object paoi13;
            object paoi14;
            object paoi15;
            object paoi16;

            using (var doc = OpenDataDocument(@"MassPrpsAssembly6\Assem1.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                GetMassPropertyArrayData(assm, "COG_Overridden_Assm-1", true, false, true, out moi1, out mass1, out cog1, out pmoi1, out paoi1, out _, out _, out _);
                GetMassPropertyArrayData(assm, "Mass_Overridden_Assm-2", true, false, true, out moi2, out mass2, out cog2, out pmoi2, out paoi2, out _, out _, out _);
                GetMassPropertyArrayData(assm, "PMOI_Overridden_Assm-1", true, false, true, out moi3, out mass3, out cog3, out pmoi3, out paoi3, out _, out _, out _);
                GetMassPropertyArrayData(assm, "None_Overridden_Assm-2", true, false, true, out moi4, out mass4, out cog4, out pmoi4, out paoi4, out _, out _, out _);

                GetMassPropertyArrayData(assm, "COG_Overridden_Assm-1", false, false, true, out moi5, out mass5, out cog5, out pmoi5, out paoi5, out _, out _, out _);
                GetMassPropertyArrayData(assm, "Mass_Overridden_Assm-2", false, false, true, out moi6, out mass6, out cog6, out pmoi6, out paoi6, out _, out _, out _);
                GetMassPropertyArrayData(assm, "PMOI_Overridden_Assm-1", false, false, true, out moi7, out mass7, out cog7, out pmoi7, out paoi7, out _, out _, out _);
                GetMassPropertyArrayData(assm, "None_Overridden_Assm-2", false, false, true, out moi8, out mass8, out cog8, out pmoi8, out paoi8, out _, out _, out _);

                GetMassPropertyArrayData(assm, "COG_Overridden_Assm-1", false, true, true, out moi9, out mass9, out cog9, out pmoi9, out paoi9, out _, out _, out _);
                GetMassPropertyArrayData(assm, "Mass_Overridden_Assm-2", false, true, true, out moi10, out mass10, out cog10, out pmoi10, out paoi10, out _, out _, out _);
                GetMassPropertyArrayData(assm, "PMOI_Overridden_Assm-1", false, true, true, out moi11, out mass11, out cog11, out pmoi11, out paoi11, out _, out _, out _);
                GetMassPropertyArrayData(assm, "None_Overridden_Assm-2", false, true, true, out moi12, out mass12, out cog12, out pmoi12, out paoi12, out _, out _, out _);

                GetMassPropertyArrayData(assm, "COG_Overridden_Assm-1", true, true, true, out moi13, out mass13, out cog13, out pmoi13, out paoi13, out _, out _, out _);
                GetMassPropertyArrayData(assm, "Mass_Overridden_Assm-2", true, true, true, out moi14, out mass14, out cog14, out pmoi14, out paoi14, out _, out _, out _);
                GetMassPropertyArrayData(assm, "PMOI_Overridden_Assm-1", true, true, true, out moi15, out mass15, out cog15, out pmoi15, out paoi15, out _, out _, out _);
                GetMassPropertyArrayData(assm, "None_Overridden_Assm-2", true, true, true, out moi16, out mass16, out cog16, out pmoi16, out paoi16, out _, out _, out _);
            }

            if (m_App.IsVersionNewerOrEqual(Xarial.XCad.SolidWorks.Enums.SwVersion_e.Sw2020))
            {
                AssertCompareDoubleArray((double[])moi1, new double[] { 732771.57070537, 207033.34471190, 284753.25397601, 207033.34471190, 1016744.10068294, 283072.44302186, 284753.25397601, 283072.44302186, 1170200.96290798 });
                AssertCompareDoubleArray((double[])moi2, new double[] { 37977.36013703, -9549.44652330, -4965.19161883, -9549.44652330, 17440.37185897, 1988.87607517, -4965.19161883, 1988.87607517, 35024.94118039 });
                AssertCompareDoubleArray((double[])moi3, new double[] { 1054.58171345, -84.46972878, -210.87298292, -84.46972878, 1130.72391154, -326.34343170, -210.87298292, -326.34343170, 1814.69437501 });
            }
            else
            {
                Assert.IsInstanceOf<MomentOfInertiaOverridenException>(moi1);
                Assert.IsInstanceOf<MomentOfInertiaOverridenException>(moi2);
                Assert.IsInstanceOf<MomentOfInertiaOverridenException>(moi3);
            }
            AssertCompareDoubleArray((double[])moi4, new double[] { 953380.76757384, 221739.33836451, 355898.35534423, 221739.33836451, 1257823.94327966, 42062.10100616, 355898.35534423, 42062.10100616, 708511.92344279 });

            if (m_App.IsVersionNewerOrEqual(Xarial.XCad.SolidWorks.Enums.SwVersion_e.Sw2020))
            {
                AssertCompareDoubleArray((double[])moi5, new double[] { 12582.55584223, 250.22092804, 148.85064148, 250.22092804, 21375.67770367, -5235.05710985, 148.85064148, -5235.05710985, 15689.65556923 });
                AssertCompareDoubleArray((double[])moi6, new double[] { 3207.83128782, -70.58035199, 50.22342169, -70.58035199, 5220.98393895, 1434.27367730, 50.22342169, 1434.27367730, 4225.95731532 });
                AssertCompareDoubleArray((double[])moi7, new double[] { 1054.58171345, -84.46972878, -210.87298292, -84.46972878, 1130.72391154, -326.34343170, -210.87298292, -326.34343170, 1814.69437501 });
                AssertCompareDoubleArray((double[])moi8, new double[] { 20444.14131853, -5636.36285550, -349.56564420, -5636.36285550, 16612.77720078, -250.39444679, -349.56564420, -250.39444679, 12590.97059582 });
            }
            else
            {
                Assert.IsInstanceOf<MomentOfInertiaOverridenException>(moi5);
                Assert.IsInstanceOf<MomentOfInertiaOverridenException>(moi6);
                Assert.IsInstanceOf<MomentOfInertiaOverridenException>(moi7);
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(moi8);
            }

            if (m_App.IsVersionNewerOrEqual(Xarial.XCad.SolidWorks.Enums.SwVersion_e.Sw2020))
            {
                AssertCompareDoubleArray((double[])moi9, new double[] { 2055493.59519249, 1473733.62357906, 209311.83240675, 1473733.62357906, 1144281.88776368, 284502.22693767, 209311.83240675, 284502.22693767, 3094248.38970080 });
                AssertCompareDoubleArray((double[])moi10, new double[] { 768793.19616025, 104754.95861837, 314872.85703145, 104754.95861837, 834722.70323284, 227762.09556753, 314872.85703145, 227762.09556753, 223073.39439345 });
                AssertCompareDoubleArray((double[])moi11, new double[] { 3378287.78275181, 10966.98973875, 9093.51593862, 10966.98973875, 1318697.38761996, 1647362.86006517, 9093.51593862, 1647362.86006517, 2061484.22500662 });
                AssertCompareDoubleArray((double[])moi12, new double[] { 14712236.09617482, 4987667.80184248, 3855648.81964258, 4987667.80184248, 8226946.56763011, 7118485.74938377, 3855648.81964258, 7118485.74938377, 11915270.93498221 });
            }
            else
            {
                Assert.IsInstanceOf<MomentOfInertiaOverridenException>(moi9);
                Assert.IsInstanceOf<MomentOfInertiaOverridenException>(moi10);
                Assert.IsInstanceOf<MomentOfInertiaOverridenException>(moi11);
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(moi12);
            }

            if (m_App.IsVersionNewerOrEqual(Xarial.XCad.SolidWorks.Enums.SwVersion_e.Sw2020))
            {
                AssertCompareDoubleArray((double[])moi13, new double[] { 18169538.72272953, 11995399.31901705, 1683580.55358953, 11995399.31901705, 10041778.53972001, 1993318.14300060, 1683580.55358953, 1993318.14300060, 26090270.31616359 });
                AssertCompareDoubleArray((double[])moi14, new double[] { 429678.07221692, -53952.39135083, 328772.98457456, -53952.39135083, 700029.78915353, -43384.18746203, 328772.98457456, -43384.18746203, 325063.83301430 });
                AssertCompareDoubleArray((double[])moi15, new double[] { 18613926.38640416, 5871368.65378197, 6995931.98530180, 5871368.65378197, 15403813.71193740, 9165628.99074609, 6995931.98530180, 9165628.99074609, 12175445.88721939 });
            }
            else
            {
                Assert.IsInstanceOf<MomentOfInertiaOverridenException>(moi13);
                Assert.IsInstanceOf<MomentOfInertiaOverridenException>(moi14);
                Assert.IsInstanceOf<MomentOfInertiaOverridenException>(moi15);
            }
            AssertCompareDoubleArray((double[])moi16, new double[] { 110981291.63683793, 23338999.87521937, 25883931.20133131, 23338999.87521937, 73306473.61240853, 54171849.01522268, 25883931.20133131, 54171849.01522268, 60433968.38786618 });

            AssertCompareDoubles(322.82511471125844, (double)mass1);
            AssertCompareDoubles(10, (double)mass2);
            AssertCompareDoubles(322.82511471125844, (double)mass3);
            AssertCompareDoubles(322.82511471125844, (double)mass4);

            AssertCompareDoubles(10, (double)mass6);
            AssertCompareDoubles(10, (double)mass10);

            if (m_App.IsVersionNewerOrEqual(Xarial.XCad.SolidWorks.Enums.SwVersion_e.Sw2020))
            {
                AssertCompareDoubles(39.2325416754816, (double)mass5);
                AssertCompareDoubles(39.2325416754816, (double)mass7);
                AssertCompareDoubles(39.2325416754816, (double)mass8);

                AssertCompareDoubles(39.2325416754816, (double)mass9);
                AssertCompareDoubles(39.2325416754816, (double)mass11);
                AssertCompareDoubles(39.2325416754816, (double)mass12);
            }
            else 
            {
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(mass5);
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(mass7);
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(mass8);

                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(mass9);
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(mass11);
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(mass12);
            }

            AssertCompareDoubles(322.82511471125844, (double)mass13);
            AssertCompareDoubles(10, (double)mass14);
            AssertCompareDoubles(322.82511471125844, (double)mass15);
            AssertCompareDoubles(322.82511471125844, (double)mass16);

            AssertCompareDoubleArray((double[])cog1, new double[] { 76.82290986, -0.07532937, -278.77174281 });
            AssertCompareDoubleArray((double[])cog2, new double[] { -154.08110264, -113.14142480, 92.49402618 });
            AssertCompareDoubleArray((double[])cog3, new double[] { -139.23655151, 45.30273096, 193.49523334 });
            AssertCompareDoubleArray((double[])cog4, new double[] { -391.50242124, 152.80074381, 410.94934022 });

            AssertCompareDoubleArray((double[])cog5, new double[] { 76.82290986, -0.07532937, -278.77174281 });
            AssertCompareDoubleArray((double[])cog9, new double[] { -165.93855976, -225.86082420, -32.13831180 });

            if (m_App.IsVersionNewerOrEqual(Xarial.XCad.SolidWorks.Enums.SwVersion_e.Sw2020))
            {
                AssertCompareDoubleArray((double[])cog6, new double[] { -217.44164968, -1.76969527, 145.08869196 });
                AssertCompareDoubleArray((double[])cog7, new double[] { -138.52048542, 181.89475797, 170.54909351 });
                AssertCompareDoubleArray((double[])cog8, new double[] { -329.99239780, 159.62894541, 534.86186021 });

                AssertCompareDoubleArray((double[])cog10, new double[] { 120.15541293, 86.88632259, 262.12624774 });
                AssertCompareDoubleArray((double[])cog11, new double[] { 1.25415420, 229.16546409, 183.20508347 });
                AssertCompareDoubleArray((double[])cog12, new double[] { 262.56537709, 483.99090078, 374.67699586 });
            }
            else
            {
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(cog6);
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(cog7);
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(cog8);
                                                                                     
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(cog10);
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(cog11);
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(cog12);
            }

            AssertCompareDoubleArray((double[])cog13, new double[] { -165.93855976, -225.86082420, -32.13831180 });
            AssertCompareDoubleArray((double[])cog14, new double[] { 167.28644176, -26.90527256, 198.76570069 });
            AssertCompareDoubleArray((double[])cog15, new double[] { 117.82614748, 154.36654994, 183.92114956 });
            AssertCompareDoubleArray((double[])cog16, new double[] { 184.35125704, 387.64004000, 436.18701929 });

            AssertCompareDoubleArray((double[])pmoi3, new double[] { 1000.00000000, 1000.00000000, 2000.00000000 });
            AssertCompareDoubleArray((double[])pmoi4, new double[] { 421570.35749821, 1090708.81606145, 1407437.46073663 });

            if (m_App.IsVersionNewerOrEqual(Xarial.XCad.SolidWorks.Enums.SwVersion_e.Sw2020))
            {
                AssertCompareDoubleArray((double[])pmoi1, new double[] { 421570.35749821, 1090708.81606145, 1407437.46073663 });
                AssertCompareDoubleArray((double[])pmoi2, new double[] { 13058.78440949, 33786.36810946, 43597.52065745 });

                AssertCompareDoubleArray((double[])pmoi5, new double[] { 12575.44120243, 12575.44120243, 24497.00671027 });
                AssertCompareDoubleArray((double[])pmoi6, new double[] { 3205.35980219, 3205.35980219, 6244.05293771 });         
                AssertCompareDoubleArray((double[])pmoi8, new double[] { 12575.44120243, 12575.44120243, 24497.00671027 });
                
                AssertCompareDoubleArray((double[])pmoi9, new double[] { 12575.44120243, 12575.44120243, 24497.00671027 });
                AssertCompareDoubleArray((double[])pmoi10, new double[] { 3205.35980219, 3205.35980219, 6244.05293771 });
                AssertCompareDoubleArray((double[])pmoi12, new double[] { 12575.44120243, 12575.44120243, 24497.00671027 });

                AssertCompareDoubleArray((double[])pmoi13, new double[] { 421570.35749821, 1090708.81606145, 1407437.46073663 });
                AssertCompareDoubleArray((double[])pmoi14, new double[] { 13058.78440949, 33786.36810946, 43597.52065745 });
            }
            else 
            {
                Assert.IsInstanceOf<PrincipalMomentOfInertiaOverridenException>(pmoi1);
                Assert.IsInstanceOf<PrincipalMomentOfInertiaOverridenException>(pmoi2);

                Assert.IsInstanceOf<PrincipalMomentOfInertiaOverridenException>(pmoi5);
                Assert.IsInstanceOf<PrincipalMomentOfInertiaOverridenException>(pmoi6);
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(pmoi8);

                Assert.IsInstanceOf<PrincipalMomentOfInertiaOverridenException>(pmoi9);
                Assert.IsInstanceOf<PrincipalMomentOfInertiaOverridenException>(pmoi10);
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(pmoi12);

                Assert.IsInstanceOf<PrincipalMomentOfInertiaOverridenException>(pmoi13);
                Assert.IsInstanceOf<PrincipalMomentOfInertiaOverridenException>(pmoi14);
            }

            AssertCompareDoubleArray((double[])pmoi7, new double[] { 1000.00000000, 1000.00000000, 2000.00000000 });

            AssertCompareDoubleArray((double[])pmoi11, new double[] { 1000.00000000, 1000.00000000, 2000.00000000 });

            AssertCompareDoubleArray((double[])pmoi15, new double[] { 1000.00000000, 1000.00000000, 2000.00000000 });
            AssertCompareDoubleArray((double[])pmoi16, new double[] { 421570.35749821, 1090708.81606145, 1407437.46073663 });

            if (m_App.IsVersionNewerOrEqual(Xarial.XCad.SolidWorks.Enums.SwVersion_e.Sw2020))
            {
                AssertCompareDoubleArray((double[])paoi1, new double[] { 0.74428291, 0.47984159, 0.46453740, -0.63704755, 0.71891902, 0.27807527, -0.20053269, -0.50289908, 0.84076106 });
                AssertCompareDoubleArray((double[])paoi2, new double[] { -0.38183860, 0.90872286, 0.16858839, -0.34497319, -0.30935881, 0.88616625, 0.85743384, 0.28021401, 0.43161016 });

                AssertCompareDoubleArray((double[])paoi5, new double[] { 0.65641077, -0.37181957, 0.65641077, 0.75400803, 0.35152849, -0.55488703, -0.02442923, 0.85917282, 0.51110203 }, 7);
                AssertCompareDoubleArray((double[])paoi6, new double[] { 0.63787041, 0.43155844, 0.63787041, 0.76961549, -0.38786325, -0.50720222, 0.02851909, 0.81444423, -0.57954055 });
                AssertCompareDoubleArray((double[])paoi8, new double[] { -0.47372654, 0.62272914, 0.62272914, 0.33991764, -0.52302028, 0.78160462, 0.81242794, 0.58194347, 0.03609197 });

                AssertCompareDoubleArray((double[])paoi9, new double[] { 0.71646322, 0.23623155, -0.65641077, -0.63321170, -0.17468496, -0.75400803, -0.29278558, 0.95586600, 0.02442923 });
                AssertCompareDoubleArray((double[])paoi10, new double[] { 0.10906999, 0.76238119, -0.63787041, -0.05361748, -0.63625243, -0.76961549, -0.99258697, 0.11814296, -0.02851909 });
                AssertCompareDoubleArray((double[])paoi12, new double[] { -0.04267474, 0.87963744, 0.47372654, 0.91256521, 0.22733400, -0.33991764, -0.40669843, 0.41780046, -0.81242794 });

                AssertCompareDoubleArray((double[])paoi13, new double[] { -0.04316746, 0.66646794, -0.74428291, -0.34551872, 0.68904806, 0.63704755, 0.93741847, 0.28466341, 0.20053269 });
                AssertCompareDoubleArray((double[])paoi14, new double[] { -0.55965261, 0.73551903, 0.38183860, 0.82460690, 0.44834914, 0.34497319, 0.08253734, 0.50793189, -0.85743384 });
            }
            else
            {
                Assert.IsInstanceOf<PrincipalAxesOfInertiaOverridenException>(paoi1);
                Assert.IsInstanceOf<PrincipalAxesOfInertiaOverridenException>(paoi2);

                Assert.IsInstanceOf<PrincipalAxesOfInertiaOverridenException>(paoi5);
                Assert.IsInstanceOf<PrincipalAxesOfInertiaOverridenException>(paoi6);
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(pmoi8);

                Assert.IsInstanceOf<PrincipalAxesOfInertiaOverridenException>(paoi9);
                Assert.IsInstanceOf<PrincipalAxesOfInertiaOverridenException>(paoi10);
                Assert.IsInstanceOf<MassPropertiesHiddenComponentBodiesNotSupported>(pmoi12);

                Assert.IsInstanceOf<PrincipalAxesOfInertiaOverridenException>(paoi13);
                Assert.IsInstanceOf<PrincipalAxesOfInertiaOverridenException>(paoi14);
            }

            AssertCompareDoubleArray((double[])paoi3, new double[] { 0.54453395, 0.46999645, 0.69468418, 0.67935084, -0.73289707, -0.03666497, 0.49189960, 0.49189960, -0.71837982 });
            AssertCompareDoubleArray((double[])paoi4, new double[] { 0.60088528, 0.19827816, 0.77435306, 0.55652280, 0.59160768, -0.58333758, -0.57377631, 0.78146409, 0.24514203 });
            
            AssertCompareDoubleArray((double[])paoi7, new double[] { 0.54453395, 0.46999645, 0.69468418, 0.67935084, -0.73289707, -0.03666497, 0.49189960, 0.49189960, -0.71837982 });
            
            AssertCompareDoubleArray((double[])paoi11, new double[] { 0.11878463, 0.83028488, -0.54453395, 0.51810061, -0.51966739, -0.67935084, -0.84703127, -0.20142694, -0.49189960 }, 7);
            
            AssertCompareDoubleArray((double[])paoi15, new double[] { 0.11878463, 0.83028488, -0.54453395, 0.51810061, -0.51966739, -0.67935084, -0.84703127, -0.20142694, -0.49189960 }, 7);
            AssertCompareDoubleArray((double[])paoi16, new double[] { 0.37354142, 0.70668500, -0.60088528, -0.83011915, -0.03441769, -0.55652280, -0.41396740, 0.70669069, 0.57377631 });
        }

        [Test]
        public void MassPropertyAssemblyLightweightTest()
        {
            object cog1;
            object mass1;
            object moi1;
            object pai1;
            object pmoi1;
            object density1;
            object area1;
            object volume1;

            object cog2;
            object mass2;
            object moi2;
            object pai2;
            object pmoi2;
            object density2;
            object area2;
            object volume2;

            using (var doc = OpenDataDocument(@"MassPrpsAssembly1\Assem1.SLDASM", true, s =>
            {
                s.LightWeight = true;
                s.UseLightWeightDefault = false;
            }))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                GetMassPropertyArrayData(assm, "Part1-1", false, false, false, out moi1, out mass1, out cog1, out pmoi1, out pai1, out density1, out area1, out volume1);
                GetMassPropertyArrayData(assm, "SubAssem1-1", false, false, false, out moi2, out mass2, out cog2, out pmoi2, out pai2, out density2, out area2, out volume2);
            }

            if (m_App.IsVersionNewerOrEqual(Xarial.XCad.SolidWorks.Enums.SwVersion_e.Sw2020))
            {
                AssertCompareDoubles((double)density1, 7300.00000000);
                AssertCompareDoubleArray((double[])cog1, new double[] { 0.03260240, 0.06212415, 0.00000000 });
                AssertCompareDoubleArray((double[])moi1, new double[] { 0.00328186, 0.00358474, 0.00000000, 0.00358474, 0.00613822, 0.00000000, 0.00000000, 0.00000000, 0.00793613 });
                //AssertCompareDoubleArray((double[])pai1, new double[] { 0.82768156, 0.56119804, 0.00000000, 0.00000000, 0.00000000, -1.00000000, -0.56119804, 0.82768156, 0.00000000 });
                Assert.IsAssignableFrom<PrincipalAxesOfInertiaOverridenLightweightComponentException>(pai1);
                //AssertCompareDoubleArray((double[])pmoi1, new double[] { 0.00085128, 0.00793613, 0.00856881 });
                Assert.IsAssignableFrom<PrincipalMomentsOfInertiaOverridenLightweightComponentException>(pmoi1);
                AssertCompareDoubles((double)mass1, 2.25609306);
            }
            else
            {
                Assert.IsAssignableFrom<NotLoadedMassPropertyComponentException>(density1);
                Assert.IsAssignableFrom<NotLoadedMassPropertyComponentException>(cog1);
                Assert.IsAssignableFrom<NotLoadedMassPropertyComponentException>(moi1);
                Assert.IsAssignableFrom<NotLoadedMassPropertyComponentException>(pai1);
                Assert.IsAssignableFrom<NotLoadedMassPropertyComponentException>(pmoi1);
                Assert.IsAssignableFrom<NotLoadedMassPropertyComponentException>(mass1);
            }

            AssertCompareDoubles((double)area1, 0.03850408);
            AssertCompareDoubles((double)volume1, 0.00030905);

            if (m_App.IsVersionNewerOrEqual(Xarial.XCad.SolidWorks.Enums.SwVersion_e.Sw2020))
            {
                AssertCompareDoubles((double)density2, 4553.1112368995864);
                AssertCompareDoubleArray((double[])cog2, new double[] { -0.00177216, 0.01832142, 0.04575921 });
                AssertCompareDoubleArray((double[])moi2, new double[] { 0.00476278, 0.00099353, -0.00189405, 0.00099353, 0.00647799, -0.00072354, -0.00189405, -0.00072354, 0.00361526 }, 6);
                //AssertCompareDoubleArray((double[])pai2, new double[] { -0.59405952, -0.25013380, 0.76454324, 0.65591383, 0.39959869, 0.64038889, -0.46569339, 0.88190360, -0.07331919 });
                Assert.IsAssignableFrom<PrincipalAxesOfInertiaOverridenLightweightComponentException>(pai2);
                //AssertCompareDoubleArray((double[])pmoi2, new double[] { 0.00190684, 0.00600671, 0.00694247 });
                Assert.IsAssignableFrom<PrincipalMomentsOfInertiaOverridenLightweightComponentException>(pmoi2);
                AssertCompareDoubles((double)mass2, 2.27447091);
            }
            else
            {
                Assert.IsAssignableFrom<MassPropertiesHiddenComponentBodiesNotSupported>(density2);
                Assert.IsAssignableFrom<MassPropertiesHiddenComponentBodiesNotSupported>(cog2);
                Assert.IsAssignableFrom<MassPropertiesHiddenComponentBodiesNotSupported>(moi2);
                Assert.IsAssignableFrom<MassPropertiesHiddenComponentBodiesNotSupported>(pai2);
                Assert.IsAssignableFrom<MassPropertiesHiddenComponentBodiesNotSupported>(pmoi2);
                Assert.IsAssignableFrom<MassPropertiesHiddenComponentBodiesNotSupported>(mass2);
            }

            AssertCompareDoubles((double)area2, 0.04697909);
            AssertCompareDoubles((double)volume2, 0.00049954);
        }

        [Test]
        public void RayIntersectionAssemblyTest()
        {
            IXRay[] rays;

            using (var doc = OpenDataDocument("RayIntersectionAssem1.SLDASM")) 
            {
                var assm = (IXAssembly)m_App.Documents.Active;

                var rayInters = assm.Evaluation.PreCreateRayIntersection();
                
                rayInters.AddRay(new Axis(new Point(0.03857029, 0.00533597, 0.02540296), new Vector(0, 0, -1)));
                rayInters.AddRay(new Axis(new Point(-0.12615451, -0.02638223, 0), new Vector(0, 0, -1)));
                rayInters.AddRay(new Axis(new Point(-0.06410509, 0.05318389, 0.00603193), new Vector(0, -1, 0)));

                rayInters.Commit();

                rays = rayInters.Rays;
            }

            var h0_1 = rays[0].Hits.FirstOrDefault(h => h.Point.IsSame(new Point(0.03857029, 0.00533597, 0.01)));
            var h0_2 = rays[0].Hits.FirstOrDefault(h => h.Point.IsSame(new Point(0.03857029, 0.00533597, 0)));
            var h0_3 = rays[0].Hits.FirstOrDefault(h => h.Point.IsSame(new Point(0.03857029, 0.00533597, -0.0385135112493299)));
            var h0_4 = rays[0].Hits.FirstOrDefault(h => h.Point.IsSame(new Point(0.03857029, 0.00533597, -0.0485135112493299)));

            var h2_1 = rays[2].Hits.FirstOrDefault(h => h.Point.IsSame(new Point(-0.06410509, 0.0344190476955077, 0.00603193)));
            var h2_2 = rays[2].Hits.FirstOrDefault(h => h.Point.IsSame(new Point(-0.06410509, -0.0858369122642557, 0.00603193)));

            Assert.AreEqual(3, rays.Length);

            Assert.AreEqual(4, rays[0].Hits.Length);
            Assert.IsNotNull(h0_1);
            Assert.IsNotNull(h0_2);
            Assert.IsNotNull(h0_3);
            Assert.IsNotNull(h0_4);
            Assert.AreEqual(RayIntersectionType_e.Enter, h0_1.Type);
            Assert.AreEqual(RayIntersectionType_e.Exit, h0_2.Type);
            Assert.AreEqual(RayIntersectionType_e.Enter, h0_3.Type);
            Assert.AreEqual(RayIntersectionType_e.Exit, h0_4.Type);

            Assert.AreEqual(0, rays[1].Hits.Length);

            Assert.AreEqual(2, rays[2].Hits.Length);
            Assert.IsNotNull(h2_1);
            Assert.IsNotNull(h2_2);
            Assert.AreEqual(RayIntersectionType_e.Enter, h2_1.Type);
            Assert.AreEqual(RayIntersectionType_e.Exit, h2_2.Type);
        }

        private void GetMassPropertyArrayData(ISwAssembly assm, string compName, bool includeHidden,
            bool relToCoord, bool userUnits, out object moi, out object mass, out object cog, out object pmoi, out object paoi,
            out object density, out object area, out object volume)
        {
            using (var massPrps = assm.Evaluation.PreCreateMassProperty())
            {
                massPrps.Scope = new IXComponent[] { assm.Configurations.Active.Components[compName] };
                massPrps.UserUnits = userUnits;
                massPrps.VisibleOnly = !includeHidden;

                if (relToCoord)
                {
                    massPrps.RelativeTo = TransformConverter.ToTransformMatrix(
                        assm.Model.Extension.GetCoordinateSystemTransformByName("Coordinate System1"));
                }

                massPrps.Commit();

                try
                {
                    var resPmoi = massPrps.PrincipalMomentOfInertia;
                    pmoi = new double[] { resPmoi.Px, resPmoi.Py, resPmoi.Pz };
                }
                catch (Exception ex)
                {
                    pmoi = ex;
                }

                try
                {
                    var resPaoi = massPrps.PrincipalAxesOfInertia;

                    paoi = new double[]
                    {
                        resPaoi.Ix.X, resPaoi.Ix.Y, resPaoi.Ix.Z,
                        resPaoi.Iy.X, resPaoi.Iy.Y, resPaoi.Iy.Z,
                        resPaoi.Iz.X, resPaoi.Iz.Y, resPaoi.Iz.Z
                    };
                }
                catch (Exception ex)
                {
                    paoi = ex;
                }

                try
                {
                    cog = massPrps.CenterOfGravity.ToArray();
                }
                catch (Exception ex)
                {
                    cog = ex;
                }

                try
                {
                    mass = massPrps.Mass;
                }
                catch (Exception ex)
                {
                    mass = ex;
                }

                try
                {
                    var resMoi = massPrps.MomentOfInertia;

                    moi = new double[]
                    {
                        resMoi.Lx.X, resMoi.Lx.Y, resMoi.Lx.Z,
                        resMoi.Ly.X, resMoi.Ly.Y, resMoi.Ly.Z,
                        resMoi.Lz.X, resMoi.Lz.Y, resMoi.Lz.Z
                    };
                }
                catch (Exception ex)
                {
                    moi = ex;
                }

                try
                {
                    density = massPrps.Density;
                }
                catch (Exception ex)
                {
                    density = ex;
                }

                try
                {
                    area = massPrps.SurfaceArea;
                }
                catch (Exception ex)
                {
                    area = ex;
                }

                try
                {
                    volume = massPrps.Volume;
                }
                catch (Exception ex)
                {
                    volume = ex;
                }
            }
        }
    }
}
