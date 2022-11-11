//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.swconst;
using System;
using Xarial.XCad.Documents;

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

    internal class SwStepSaveOptions : IXStepSaveOptions
    {
        private readonly SwDocument m_Doc;

        internal SwStepSaveOptions(SwDocument doc)
        {
            m_Doc = doc;
        }

        public StepFormat_e Format
        {
            get 
            {
                switch (m_Doc.OwnerApplication.Sw.GetUserPreferenceIntegerValue((int)swUserPreferenceIntegerValue_e.swStepAP)) 
                {
                    case 203:
                        return StepFormat_e.Ap203;
                    case 214:
                        return StepFormat_e.Ap214;
                    default:
                        throw new NotSupportedException();
                }
            }
            set 
            {
                int format;

                switch (value) 
                {
                    case StepFormat_e.Ap203:
                        format = 203;
                        break;

                    case StepFormat_e.Ap214:
                        format = 214;
                        break;

                    default:
                        throw new NotSupportedException();
                }

                if (!m_Doc.OwnerApplication.Sw.SetUserPreferenceIntegerValue((int)swUserPreferenceIntegerValue_e.swStepAP, format)) 
                {
                    throw new Exception("Failed to set the option");
                }
            }
        }
    }

    internal class SwPdfSaveOptions : IXPdfSaveOptions
    {
        public bool Pdf3D { get; set; }
    }

    internal class SwSaveOptions : IXSaveOptions
    {
        private readonly SwDocument m_Doc;

        internal SwSaveOptions(SwDocument doc)
        {
            m_Doc = doc;
            Step = new SwStepSaveOptions(m_Doc);
            Pdf = new SwPdfSaveOptions();
        }

        public IXStepSaveOptions Step { get; }
        public IXPdfSaveOptions Pdf { get; }
    }

    internal class SwDocumentOptions : SwOptions, ISwDocumentOptions 
    {
        protected readonly SwDocument m_Doc;

        internal SwDocumentOptions(SwDocument doc) 
        {
            m_Doc = doc;
            ViewEntityKindVisibility = new SwViewEntityKindVisibilityOptions(doc);
            SaveOptions = new SwSaveOptions(doc);
        }

        public IXViewEntityKindVisibilityOptions ViewEntityKindVisibility { get; }

        public IXSaveOptions SaveOptions { get; }
    }
}