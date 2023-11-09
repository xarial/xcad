using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Sketch;
using Xarial.XCad.SolidWorks.Features;

namespace SolidWorks.Tests.Integration
{
    public class FeaturesTests : IntegrationTests
    {
        //NOTE: SW 2024, 'Lights, Cameras and Themes' feature is renamed to 'Lights and Cameras' (index 11)
        [Test]
        public void IterateFeaturesTest()
        {
            var featNames = new List<string>();

            using (var doc = OpenDataDocument("Features1.SLDPRT"))
            {
                foreach (var feat in m_App.Documents.Active.Features)
                {
                    featNames.Add(feat.Name);
                }
            }

            var expected = new string[] { "Comments", "Favorites", "History", "Selection Sets", "Sensors", "Design Binder", "Annotations", "Notes",
                "Notes1___EndTag___", "Surface Bodies", "Solid Bodies", "Lights, Cameras and Scene", "Ambient", "Directional1", "Directional2", "Directional3",
                "Markups", "Equations", "Material <not specified>", "Front Plane", "Top Plane", "Right Plane", "Origin", "Sketch1", "Boss-Extrude1", "Boss-Extrude2",
                "Sketch1<2>" };

            CollectionAssert.AreEqual(expected, featNames);
        }

        [Test]
        public void IsUserFeatureTest()
        {
            var userFeatsNames = new List<string>();

            using (var doc = OpenDataDocument("Cylinder2.SLDPRT"))
            {
                foreach (var feat in m_App.Documents.Active.Features)
                {
                    if (feat.IsUserFeature)
                    {
                        userFeatsNames.Add(feat.Name);
                    }
                }
            }

            CollectionAssert.AreEqual(new string[] { "Plane2", "Sketch1", "Boss-Extrude1", "Plane1" }, userFeatsNames);
        }

        [Test]
        public void GetFeatureByNameTest()
        {
            IXFeature feat1;
            IXFeature feat2;
            IXFeature feat3;
            bool r1;
            bool r2;
            Exception e1 = null;

            using (var doc = OpenDataDocument("Features1.SLDPRT"))
            {
                feat1 = m_App.Documents.Active.Features["Sketch1"];
                r1 = m_App.Documents.Active.Features.TryGet("Sketch1", out feat2);
                r2 = m_App.Documents.Active.Features.TryGet("Sketch2", out feat3);
                
                try
                {
                    var feat4 = m_App.Documents.Active.Features["Sketch2"];
                }
                catch (Exception ex)
                {
                    e1 = ex;
                }
            }

            Assert.IsNotNull(feat1);
            Assert.IsNotNull(feat2);
            Assert.IsNull(feat3);
            Assert.IsTrue(r1);
            Assert.IsFalse(r2);
            Assert.IsNotNull(e1);
        }

        [Test]
        public void SpecificFeatureEqualsTest() 
        {
            var e1 = false;

            using (var doc = OpenDataDocument("Sketch1.SLDPRT"))
            {
                var sketch1 = m_App.Documents.Active.Features["Sketch1"];
                var sketch2 = ((ISwSketchBase)sketch1).Entities.OfType<IXSketchSegment>().First().OwnerSketch;
                e1 = sketch1.Equals(sketch2);
            }

            Assert.IsTrue(e1);
        }

        [Test]
        public void FilterFeaturesTest()
        {
            string[] feats1;
            string[] feats2;

            using (var doc = OpenDataDocument("PartFeatures1.SLDPRT"))
            {
                var part = m_App.Documents.Active;

                feats1 = part.Features.Filter<IXSketch2D>().Select(f => f.Name).ToArray();
                feats2 = part.Features.Filter<IXSketch2D>(true).Select(f => f.Name).ToArray();
            }

            CollectionAssert.AreEqual(new string[] { "Sketch1", "Sketch2", "Sketch3", "Bend-Lines2", "Bounding-Box2" }, feats1);
            CollectionAssert.AreEqual(new string[] { "Bounding-Box2", "Bend-Lines2", "Sketch3", "Sketch2", "Sketch1" }, feats2);
        }

        [Test]
        public void StructuralMemberProfilePlaneTest() 
        {
            double[] f1_g1_p1;
            double[] f1_g1_p2;

            double[] f2_g1_p1;
            double[] f2_g1_p2;

            double[] f4_g1_p1;
            double[] f4_g1_p2;
            double[] f4_g1_p3;

            double[] f5_g1_p1;
            double[] f5_g1_p2;
            double[] f5_g2_p1;
            double[] f5_g2_p2;

            double[] f6_g1_p1;

            double[] f9_g1_p1;
            double[] f9_g1_p2;

            double[] f10_g1_p1;
            double[] f10_g1_p2;
            double[] f10_g1_p3;

            double[] f11_g1_p1;
            double[] f12_g1_p1;

            using (var doc = OpenDataDocument("StructuralMemberOrientation.SLDPRT"))
            {
                var part = m_App.Documents.Active;

                var f1 = (ISwStructuralMember)part.Features["2D-Simple"];
                var f1_g1 = f1.Groups["Group1"];
                f1_g1_p1 = f1_g1.Pieces["2D-Simple[1]"].ProfilePlane.GetTransformation().ToArray();
                f1_g1_p2 = f1_g1.Pieces["2D-Simple[2]"].ProfilePlane.GetTransformation().ToArray();

                var f2 = (ISwStructuralMember)part.Features["2D-Angle"];
                var f2_g1 = f2.Groups["Group1"];
                f2_g1_p1 = f2_g1.Pieces["2D-Angle[1]"].ProfilePlane.GetTransformation().ToArray();
                f2_g1_p2 = f2_g1.Pieces["2D-Angle[2]"].ProfilePlane.GetTransformation().ToArray();

                var f3 = (ISwStructuralMember)part.Features["3D-Simple-ShiftedProfile"];
                var f3_g1 = f3.Groups["Group1"];
                Assert.Throws<NotSupportedException>(() => f3_g1.Pieces["3D-Simple-ShiftedProfile[1]"].ProfilePlane.GetTransformation().ToArray());
                Assert.Throws<NotSupportedException>(() => f3_g1.Pieces["3D-Simple-ShiftedProfile[2]"].ProfilePlane.GetTransformation().ToArray());
                Assert.Throws<NotSupportedException>(() => f3_g1.Pieces["3D-Simple-ShiftedProfile[3]"].ProfilePlane.GetTransformation().ToArray());

                var f4 = (ISwStructuralMember)part.Features["3D-Angle-Mirror-Locate-Align"];
                var f4_g1 = f4.Groups["Group1"];
                f4_g1_p1 = f4_g1.Pieces["3D-Angle-Mirror-Locate-Align[1]"].ProfilePlane.GetTransformation().ToArray();
                f4_g1_p2 = f4_g1.Pieces["3D-Angle-Mirror-Locate-Align[2]"].ProfilePlane.GetTransformation().ToArray();
                f4_g1_p3 = f4_g1.Pieces["3D-Angle-Mirror-Locate-Align[3]"].ProfilePlane.GetTransformation().ToArray();

                var f5 = (ISwStructuralMember)part.Features["2D-2Groups"];
                var f5_g1 = f5.Groups["Group1"];
                var f5_g2 = f5.Groups["Group2"];
                f5_g1_p1 = f5_g1.Pieces["2D-2Groups[1]"].ProfilePlane.GetTransformation().ToArray();
                f5_g1_p2 = f5_g1.Pieces["2D-2Groups[2]"].ProfilePlane.GetTransformation().ToArray();
                f5_g2_p1 = f5_g2.Pieces["2D-2Groups[3]"].ProfilePlane.GetTransformation().ToArray();
                f5_g2_p2 = f5_g2.Pieces["2D-2Groups[4]"].ProfilePlane.GetTransformation().ToArray();

                var f6 = (ISwStructuralMember)part.Features["2D-Arc"];
                var f6_g1 = f6.Groups["Group1"];
                f6_g1_p1 = f6_g1.Pieces["2D-Arc[1]"].ProfilePlane.GetTransformation().ToArray();
                Assert.Throws<NotSupportedException>(() => f6_g1.Pieces["2D-Arc[2]"].ProfilePlane.GetTransformation().ToArray());
                Assert.Throws<NotSupportedException>(() => f6_g1.Pieces["2D-Arc[3]"].ProfilePlane.GetTransformation().ToArray());

                var f7 = (ISwStructuralMember)part.Features["2D-Arc-MergeArcs"];
                var f7_g1 = f7.Groups["Group1"];
                Assert.Throws<NotSupportedException>(() => f7_g1.Pieces["2D-Arc-MergeArcs"].ProfilePlane.GetTransformation().ToArray());

                var f8 = (ISwStructuralMember)part.Features["2D-Locate-MergeBodies"];
                var f8_g1 = f8.Groups["Group1"];
                Assert.Throws<NotSupportedException>(() => f8_g1.Pieces["2D-Locate-MergeBodies"].ProfilePlane.GetTransformation().ToArray());

                var f9 = (ISwStructuralMember)part.Features["2D-Angle-2Lines-1Group"];
                var f9_g1 = f9.Groups["Group1"];
                f9_g1_p1 = f9_g1.Pieces["2D-Angle-2Lines-1Group[1]"].ProfilePlane.GetTransformation().ToArray();
                f9_g1_p2 = f9_g1.Pieces["2D-Angle-2Lines-1Group[2]"].ProfilePlane.GetTransformation().ToArray();

                var f10 = (ISwStructuralMember)part.Features["3D-Simple"];
                var f10_g1 = f10.Groups["Group1"];
                f10_g1_p1 = f10_g1.Pieces["3D-Simple[1]"].ProfilePlane.GetTransformation().ToArray();
                f10_g1_p2 = f10_g1.Pieces["3D-Simple[2]"].ProfilePlane.GetTransformation().ToArray();
                f10_g1_p3 = f10_g1.Pieces["3D-Simple[3]"].ProfilePlane.GetTransformation().ToArray();

                var f11 = (ISwStructuralMember)part.Features["Front-Single1"];
                var f11_g1 = f11.Groups["Group1"];
                f11_g1_p1 = f11_g1.Pieces["Front-Single1"].ProfilePlane.GetTransformation().ToArray();

                var f12 = (ISwStructuralMember)part.Features["Front-Single2"];
                var f12_g1 = f12.Groups["Group1"];
                f12_g1_p1 = f12_g1.Pieces["Front-Single2"].ProfilePlane.GetTransformation().ToArray();
            }

            AssertCompareDoubleArray(f1_g1_p1, new double[] { -0.696982685679162, 0.717087955458368, 0, 0, 3.06027835287119E-17, 2.97447615606172E-17, 1, 0, 0.717087955458368, 0.696982685679162, -4.26764712693443E-17, 0, -0.0988237874768574, 0.00497627851723362, 0, 1 });
            AssertCompareDoubleArray(f1_g1_p2, new double[] { 0.487942865115074, 0.872875569816966, 0, 0, 3.72512491770063E-17, -2.0823679664165E-17, 1, 0, 0.872875569816966, -0.487942865115074, -4.26764712693443E-17, 0, -0.0305594539712167, 0.0713266587470152, 0, 1 });

            AssertCompareDoubleArray(f2_g1_p1, new double[] { 0.707106781186549, -0.384871887652505, 0.593189371191693, 0, 0.707106781186546, 0.384871887652506, -0.593189371191695, 0, 0, 0.838896453794862, 0.544291043294307, 0, 0, 0.0910599595548547, -0.00914093971061902, 1 });
            AssertCompareDoubleArray(f2_g1_p2, new double[] { 0.707106781186549, 0.474500705567612, 0.524260508159671, 0, 0.707106781186546, -0.474500705567614, -0.524260508159673, 0, 0, 0.741416320856019, -0.671045333169321, 0, 0, 0.147023452122312, 0.0271691765357218, 1 });
                                             
            AssertCompareDoubleArray(f4_g1_p1, new double[] { -0.0489657103604467, -0.748314846174654, -0.661534012884826, 0, -0.937783491705367, -0.193504629924099, 0.2883020653461, 0, -0.343750610026785, 0.634492591913051, -0.692282217678363, 0, -0.29627702850647, 1.26295004578906, -0.730588461212103, 1 });
            AssertCompareDoubleArray(f4_g1_p2, new double[] { -0.837269255153958, 0.546723929652597, 0.00855214120312214, 0, -0.546524671794846, -0.836271425170795, -0.0442818988113923, 0, -0.0170580624184463, -0.0417498285987968, 0.998982469475065, 0, -0.013005862293265, 0.740090009407317, -0.160106299347332, 1 });
            AssertCompareDoubleArray(f4_g1_p3, new double[] { -0.687413944196536, 0.00159848699661821, -0.726264080182604, 0, -0.688264825488334, -0.320666315611199, 0.650741610800978, 0, -0.231848224749888, 0.947190877738833, 0.221530679159039, 0, -2.22044604925031E-16, 0.771922025740906, -0.921777121780735, 1 });
                                             
            AssertCompareDoubleArray(f5_g1_p1, new double[] { -0.996973823605753, -0.0777379897149711, -1.52655665885959E-16, 0, -1.56939220357313E-16, 4.89931463540739E-17, 1, 0, -0.0777379897149711, 0.996973823605753, -6.10450239491097E-17, 0, -0.401004075164735, 1.41221295142481, 0, 1 });
            AssertCompareDoubleArray(f5_g1_p2, new double[] { -0.798363289809178, -0.602176101721968, -1.52655665885959E-16, 0, -1.58634534175924E-16, -4.31894876424868E-17, 1, 0, -0.602176101721968, 0.798363289809178, -6.10450239491097E-17, 0, -0.436788080715985, 1.87113550613924, 0, 1 });
            //AssertCompareDoubleArray(f5_g2_p3, new double[] { }); 180 deg incorrect orientation
            //AssertCompareDoubleArray(f5_g2_p4, new double[] { }); 180 deg incorrect orientation

            AssertCompareDoubleArray(f6_g1_p1, new double[] { 6.12303176911189E-17, -0.456281749976792, 0.889835358163585, 0, 1, -2.14575086302006E-17, -7.98136269001716E-17, 0, 5.55111512312578E-17, 0.889835358163585, 0.456281749976792, 0, 0, 1.46818069700487, -0.511193995107391, 1 });

            AssertCompareDoubleArray(f9_g1_p1, new double[] { -0.634332586758439, 0.518297444561693, 0.573576436351046, 0, 0.444164458980324, -0.362915777716927, 0.819152044288992, 0, 0.632724349740915, 0.774377102738024, -3.88403858090484E-17, 0, -2.47902620556386, -2.01813508691671, 0, 1 });
            AssertCompareDoubleArray(f9_g1_p2, new double[] { -0.634332586758439, 0.518297444561693, 0.573576436351046, 0, 0.444164458980324, -0.362915777716927, 0.819152044288992, 0, 0.632724349740915, 0.774377102738024, -3.88403858090484E-17, 0, -1.91340250696993, 0.209722774339184, 0, 1 });

            AssertCompareDoubleArray(f10_g1_p1, new double[] { 0.466511385820849, 0.881233737101931, 0.0761198232583102, 0, 0.881233737101931, -0.455650371473257, -0.125737184514557, 0, -0.0761198232583102, 0.125737184514557, -0.989138985652409, 0, -0.250029999402821, -0.17469092074905, -0.0706925576774027, 1 });
            AssertCompareDoubleArray(f10_g1_p2, new double[] { 0.233827232692418, -0.27300219461104, 0.933163772865707, 0, 0.927995235532777, -0.223688714506309, -0.297973491826004, 0, 0.290085621980602, 0.935645852200538, 0.201040222791503, 0, -0.271928439961422, -0.138518368800866, -0.3552518269232, 1 });
            AssertCompareDoubleArray(f10_g1_p3, new double[] { 0.502278494441106, -0.0808346916728366, 0.860919314828128, 0, -0.142215261223007, -0.989785612096397, -0.00996300969748107, 0, 0.852930907809799, -0.117431659739262, -0.50864395385538, 0, -0.145539136577952, 0.269139296773361, -0.267659296355191, 1 });

            //AssertCompareDoubleArray(f11_g1_p1, new double[] { }); 180 deg incorrect orientation
            AssertCompareDoubleArray(f12_g1_p1, new double[] { -0.857785215958651, -0.514008291064231, 1.66533453693773E-16, 0, 1.30119549963726E-16, 1.06844244309736E-16, 1, 0, -0.514008291064231, 0.857785215958651, -2.47668856682643E-17, 0, -0.232127246833692, -1.89095761296899, 0, 1 });
        }
    }
}
