//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using Xarial.XCad.Documents;
using Xarial.XCad.SolidWorks.Utils;

namespace Xarial.XCad.SolidWorks.Documents
{
    public interface ISwDocumentOptions : ISwOptions, IXDocumentOptions
    {
    }

    public interface ISwViewEntityKindVisibilityOptions : IXViewEntityKindVisibilityOptions 
    {
    }

    internal class SwViewEntityKindVisibilityOptions : ISwViewEntityKindVisibilityOptions 
    {
        private readonly SwDocument m_Doc;

        internal SwViewEntityKindVisibilityOptions(SwDocument doc) 
        {
            m_Doc = doc;
        }

        public bool Axes
        {
            get => m_Doc.GetUserPreferenceToggle(swUserPreferenceToggle_e.swDisplayAxes);
            set => m_Doc.SetUserPreferenceToggle(swUserPreferenceToggle_e.swDisplayAxes, value);
        }

        public bool TemporaryAxes
        {
            get => m_Doc.GetUserPreferenceToggle(swUserPreferenceToggle_e.swDisplayTemporaryAxes);
            set => m_Doc.SetUserPreferenceToggle(swUserPreferenceToggle_e.swDisplayTemporaryAxes, value);
        }

        public bool CoordinateSystems
        {
            get => m_Doc.GetUserPreferenceToggle(swUserPreferenceToggle_e.swDisplayCoordSystems);
            set => m_Doc.SetUserPreferenceToggle(swUserPreferenceToggle_e.swDisplayCoordSystems, value);
        }

        public bool Curves
        {
            get => m_Doc.GetUserPreferenceToggle(swUserPreferenceToggle_e.swDisplayCurves);
            set => m_Doc.SetUserPreferenceToggle(swUserPreferenceToggle_e.swDisplayCurves, value);
        }

        public bool DimensionNames
        {
            get => m_Doc.GetUserPreferenceToggle(swUserPreferenceToggle_e.swShowDimensionNames);
            set => m_Doc.SetUserPreferenceToggle(swUserPreferenceToggle_e.swShowDimensionNames, value);
        }

        public bool Origins
        {
            get => m_Doc.GetUserPreferenceToggle(swUserPreferenceToggle_e.swDisplayOrigins);
            set => m_Doc.SetUserPreferenceToggle(swUserPreferenceToggle_e.swDisplayOrigins, value);
        }

        public bool Planes
        {
            get => m_Doc.GetUserPreferenceToggle(swUserPreferenceToggle_e.swDisplayPlanes);
            set => m_Doc.SetUserPreferenceToggle(swUserPreferenceToggle_e.swDisplayPlanes, value);
        }

        public bool Points
        {
            get => m_Doc.GetUserPreferenceToggle(swUserPreferenceToggle_e.swDisplayReferencePoints2);
            set => m_Doc.SetUserPreferenceToggle(swUserPreferenceToggle_e.swDisplayReferencePoints2, value);
        }

        public bool Sketches
        {
            get => m_Doc.GetUserPreferenceToggle(swUserPreferenceToggle_e.swDisplaySketches);
            set => m_Doc.SetUserPreferenceToggle(swUserPreferenceToggle_e.swDisplaySketches, value);
        }

        public bool BendLines
        {
            get
            {
                if (m_Doc.OwnerApplication.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2022))
                {
                    return m_Doc.GetUserPreferenceToggle(swUserPreferenceToggle_e.swDisplayBendLines);
                }
                else 
                {
                    return Sketches;
                }
            }
            set
            {
                if (m_Doc.OwnerApplication.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2022))
                {
                    m_Doc.SetUserPreferenceToggle(swUserPreferenceToggle_e.swDisplayBendLines, value);
                }
            }
        }

        public bool SketchDimensions
        {
            get => m_Doc.GetUserPreferenceToggle(swUserPreferenceToggle_e.swHideShowSketchDimensions);
            set => m_Doc.SetUserPreferenceToggle(swUserPreferenceToggle_e.swHideShowSketchDimensions, value);
        }

        public bool SketchPlanes
        {
            get => m_Doc.GetUserPreferenceToggle(swUserPreferenceToggle_e.swDisplaySketchPlanes);
            set => m_Doc.SetUserPreferenceToggle(swUserPreferenceToggle_e.swDisplaySketchPlanes, value);
        }

        public bool SketchRelations
        {
            get => m_Doc.GetUserPreferenceToggle(swUserPreferenceToggle_e.swViewSketchRelations);
            set => m_Doc.SetUserPreferenceToggle(swUserPreferenceToggle_e.swViewSketchRelations, value);
        }

        public void HideAll()
            => m_Doc.SetUserPreferenceToggle(swUserPreferenceToggle_e.swDisplayAllAnnotations, false);

        public void ShowAll()
            => m_Doc.SetUserPreferenceToggle(swUserPreferenceToggle_e.swDisplayAllAnnotations, true);
    }

    internal static class TextFormatUserPreferences 
    {
        internal static IFont GetFont(SwDocument doc, swUserPreferenceTextFormat_e pref) 
        {
            var textFormat = doc.Model.Extension.GetUserPreferenceTextFormat(
                    (int)pref,
                    (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified);

            return new SwTextFormat(textFormat);
        }

        internal static void SetFont(SwDocument doc, swUserPreferenceTextFormat_e pref, IFont font) 
        {
            TextFormat textFormat;

            if (font is SwTextFormat)
            {
                textFormat = (TextFormat)((SwTextFormat)font).TextFormat;
            }
            else 
            {
                textFormat = doc.Model.Extension.GetUserPreferenceTextFormat(
                    (int)pref,
                    (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified);

                SwFontHelper.FillTextFormat(font, textFormat);
            }

            if (!doc.Model.Extension.SetUserPreferenceTextFormat(
                (int)pref,
                (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified, textFormat))
            {
                throw new Exception($"Failed to set text format '{pref}'");
            }
        }
    }

    internal class SwAnnotationsDraftingStandardOptions : IXAnnotationsDraftingStandardOptions 
    {
        private readonly SwDocument m_Doc;

        internal SwAnnotationsDraftingStandardOptions(SwDocument doc) 
        {
            m_Doc = doc;
        }

        public IFont TextFont
        {
            get => TextFormatUserPreferences.GetFont(m_Doc, swUserPreferenceTextFormat_e.swDetailingAnnotationTextFormat);
            set => TextFormatUserPreferences.SetFont(m_Doc, swUserPreferenceTextFormat_e.swDetailingAnnotationTextFormat, value);
        }
    }

    internal class SwDimensionsDraftingStandardOptions : IXDimensionsDraftingStandardOptions 
    {
        private readonly SwDocument m_Doc;

        internal SwDimensionsDraftingStandardOptions(SwDocument doc)
        {
            m_Doc = doc;
        }

        public IFont TextFont
        {
            get => TextFormatUserPreferences.GetFont(m_Doc, swUserPreferenceTextFormat_e.swDetailingDimensionTextFormat);
            set => TextFormatUserPreferences.SetFont(m_Doc, swUserPreferenceTextFormat_e.swDetailingDimensionTextFormat, value);
        }
    }

    internal class SwTablesDraftingStandardOptions : IXTablesDraftingStandardOptions 
    {
        private readonly SwDocument m_Doc;

        internal SwTablesDraftingStandardOptions(SwDocument doc)
        {
            m_Doc = doc;
        }

        public IFont TextFont
        {
            get => TextFormatUserPreferences.GetFont(m_Doc, swUserPreferenceTextFormat_e.swDetailingTableTextFormat);
            set => TextFormatUserPreferences.SetFont(m_Doc, swUserPreferenceTextFormat_e.swDetailingTableTextFormat, value);
        }
    }

    internal class SwViewsDraftingStandardOptions : IXViewsDraftingStandardOptions 
    {
        private readonly SwDocument m_Doc;

        internal SwViewsDraftingStandardOptions(SwDocument doc)
        {
            m_Doc = doc;
        }

        public IFont TextFont
        {
            get => TextFormatUserPreferences.GetFont(m_Doc, swUserPreferenceTextFormat_e.swDetailingViewTextFormat);
            set => TextFormatUserPreferences.SetFont(m_Doc, swUserPreferenceTextFormat_e.swDetailingViewTextFormat, value);
        }
    }

    internal class SwSheetMetalDraftingStandardOptions : IXSheetMetalDraftingStandardOptions 
    {
        private readonly SwDocument m_Doc;

        internal SwSheetMetalDraftingStandardOptions(SwDocument doc)
        {
            m_Doc = doc;
        }

        public IFont TextFont
        {
            get => TextFormatUserPreferences.GetFont(m_Doc, swUserPreferenceTextFormat_e.swSheetMetalBendNotesTextFormat);
            set => TextFormatUserPreferences.SetFont(m_Doc, swUserPreferenceTextFormat_e.swSheetMetalBendNotesTextFormat, value);
        }
    }

    internal class SwDocumentOptions : SwOptions, ISwDocumentOptions 
    {
        protected readonly SwDocument m_Doc;

        internal SwDocumentOptions(SwDocument doc)
        {
            m_Doc = doc;
            ViewEntityKindVisibility = new SwViewEntityKindVisibilityOptions(doc);
            AnnotationsDraftingStandard = new SwAnnotationsDraftingStandardOptions(doc);
            DimensionsDraftingStandard = new SwDimensionsDraftingStandardOptions(doc);
            TablesDraftingStandard = new SwTablesDraftingStandardOptions(doc);
            ViewsDraftingStandard = new SwViewsDraftingStandardOptions(doc);
            SheetMetalDraftingStandard = new SwSheetMetalDraftingStandardOptions(doc);
        }

        public IXViewEntityKindVisibilityOptions ViewEntityKindVisibility { get; }
        public IXAnnotationsDraftingStandardOptions AnnotationsDraftingStandard { get; }
        public IXDimensionsDraftingStandardOptions DimensionsDraftingStandard { get; }
        public IXTablesDraftingStandardOptions TablesDraftingStandard { get; }
        public IXViewsDraftingStandardOptions ViewsDraftingStandard { get; }
        public IXSheetMetalDraftingStandardOptions SheetMetalDraftingStandard { get; }
    }
}