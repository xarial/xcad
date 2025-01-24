//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Xarial.XCad.Documents;
using Xarial.XCad.Exceptions;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Sketch;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.UI;

namespace Xarial.XCad.SolidWorks.Sketch
{
    public interface ISwSketchPicture : IXSketchPicture, ISwSketchEntity, ISwFeature
    {
        /// <summary>
        /// Pointer to the sketch picture
        /// </summary>
        ISketchPicture SketchPicture { get; }
    }

    internal class SwSketchPicture : SwFeature, ISwSketchPicture
    {
        private class PictureTransparency
        {
            internal int Style { get; }
            internal double Transparency { get; }
            internal int MatchingColor { get; }
            internal double MatchingTolerance { get; }

            internal PictureTransparency(int style, double transparency, int matchingColor, double matchingTolerance)
            {
                Style = style;
                Transparency = transparency;
                MatchingColor = matchingColor;
                MatchingTolerance = matchingTolerance;
            }
        }

        private const int TRANSPARENT = 1;

        public ISketchPicture SketchPicture { get; private set; }
                
        private SwSketchBase m_OwnerSketch;

        private PictureTransparency m_PictTrans;

        internal SwSketchPicture(IFeature feat, SwDocument doc, SwApplication app, bool created) : base(feat, doc, app, created)
        {
            if (feat != null)
            {
                SketchPicture = feat.GetSpecificFeature2() as ISketchPicture;
            }
        }

        internal SwSketchPicture(ISketchPicture skPict, SwDocument doc, SwApplication app, bool created) : base(skPict.GetFeature(), doc, app, created)
        {
            SketchPicture = skPict;
        }

        internal SwSketchPicture(SwSketchBase ownerSketch, SwDocument doc, SwApplication app) : base(null, doc, app, false)
        {
            m_OwnerSketch = ownerSketch;
        }

        public override object Dispatch => SketchPicture;

        public IXLayer Layer
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public IXImage Image 
        {
            get 
            {
                if (IsCommitted)
                {
                    throw new NotSupportedException();
                }
                else 
                {
                    return m_Creator.CachedProperties.Get<IXImage>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    throw new CommitedElementReadOnlyParameterException();
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public Rect2D Boundary
        {
            get
            {
                if (IsCommitted)
                {
                    double width = -1;
                    double height = -1;
                    double x = -1;
                    double y = -1;

                    SketchPicture.GetSize(ref width, ref height);
                    SketchPicture.GetOrigin(ref x, ref y);
                    var angle = SketchPicture.Angle;

                    var transform = TransformMatrix.CreateFromRotationAroundAxis(new Vector(0, 0, 1), angle, new Point(0, 0, 0));

                    var dirX = new Vector(1, 0, 0).Transform(transform);
                    var dirY = new Vector(0, 1, 0).Transform(transform);

                    return new Rect2D(width, height, new Point(x + width / 2, y + height / 2, 0), dirX, dirY);
                }
                else
                {
                    return m_Creator.CachedProperties.Get<Rect2D>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    throw new CommitedElementReadOnlyParameterException();
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public IXSketchBase OwnerSketch
        {
            get
            {
                var ownerFeat = Feature.GetOwnerFeature();

                if (OwnerDocument is SwDrawing)
                {
                    var draw = (SwDrawing)OwnerDocument;

                    var ownerSketch = (ISketch)ownerFeat.GetSpecificFeature2();

                    foreach (object[] sheet in draw.Drawing.GetViews() as object[])
                    {
                        var sheetView = (IView)sheet.First();

                        var sheetSketch = sheetView.IGetSketch();

                        if (OwnerApplication.Sw.IsSame(sheetSketch, ownerSketch) == (int)swObjectEquality.swObjectSame) 
                        {
                            return new SwSheetSketch((SwSheet)draw.Sheets[sheetView.Name], ownerSketch, draw, OwnerApplication, false);
                        }
                    }

                    throw new Exception("Faild to find the owner sketch of this sketch picture");
                }
                else
                {
                    return OwnerDocument.CreateObjectFromDispatch<ISwSketchBase>(ownerFeat);
                }
            }
        }

        /// <remarks>
        /// Sketch picture in SOLIDWORKS cannot be added into the block
        /// </remarks>
        public IXSketchBlockInstance OwnerBlock => null;

        public override FeatureState_e State 
        {
            get
            {
                var trans = GetTransparency();

                if (trans.Transparency == TRANSPARENT)
                {
                    return FeatureState_e.Suppressed;
                }
                else 
                {
                    return FeatureState_e.Default;
                }
            }
            set 
            {
                const int OPAQUE = 0;
                const int COLOR_IGNORE = 0;
                const int COLOR_EXACT_MATCH = 0;

                if (value.HasFlag(FeatureState_e.Suppressed))
                {
                    m_PictTrans = GetTransparency();
                    SketchPicture.SetTransparency((int)swSketchPictureTransparencyStyle_e.swSketchPictureTransparencyFullImage,
                        TRANSPARENT, COLOR_IGNORE, COLOR_EXACT_MATCH);
                }
                else
                {
                    var style = m_PictTrans?.Style ?? (int)swSketchPictureTransparencyStyle_e.swSketchPictureTransparencyFullImage;
                    var trans = m_PictTrans?.Transparency ?? OPAQUE;
                    int matchColor = m_PictTrans?.MatchingColor ?? COLOR_IGNORE;
                    double matchTol = m_PictTrans?.MatchingTolerance ?? COLOR_EXACT_MATCH;

                    SketchPicture.SetTransparency(style, trans, matchColor, matchTol);
                }
            }
        }

        private PictureTransparency GetTransparency() 
        {
            int style = -1;
            double trans = -1;
            int matchColor = -1;
            double matchTol = -1;

            SketchPicture.GetTransparency(ref style, ref trans, ref matchColor, ref matchTol);

            return new PictureTransparency(style, trans, matchColor, matchTol);
        }

        protected override IFeature InsertFeature(CancellationToken cancellationToken)
        {
            if (Image == null) 
            {
                throw new Exception("Image is not specified");
            }

            if (Boundary == null) 
            {
                throw new Exception("Boundary of the image is not specified");
            }

            using (var editor = m_OwnerSketch?.Edit())
            {
                var tempFileName = "";

                try
                {
                    tempFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".png");

                    File.WriteAllBytes(tempFileName, Image.Buffer);

                    var pict = OwnerDocument.Model.SketchManager.InsertSketchPicture(tempFileName);

                    if (pict != null)
                    {
                        var orig = new Point(Boundary.CenterPoint.X - Boundary.Width / 2, Boundary.CenterPoint.Y - Boundary.Height / 2, 0);

                        pict.SetOrigin(orig.X, orig.Y);
                        pict.SetSize(Boundary.Width, Boundary.Height, false);

                        var angle = Boundary.AxisX.GetAngle(new Vector(1, 0, 0));

                        //picture PMPage stays open after inserting the picture
                        const int swCommands_PmOK = -2;
                        OwnerApplication.Sw.RunCommand(swCommands_PmOK, "");

                        SketchPicture = pict;

                        return pict.GetFeature();
                    }
                    else
                    {
                        throw new Exception("Failed to insert picture");
                    }
                }
                finally
                {
                    if (File.Exists(tempFileName))
                    {
                        try
                        {
                            File.Delete(tempFileName);
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }
    }
}
