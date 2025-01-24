//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolidWorks.Interop.sldworks;

namespace Xarial.XCad.SolidWorks.Annotations
{
    /// <summary>
    /// Represents the placeholder for the <see cref="IAnnotation"/> for the entities which are not native annotations (e.g. break line, detail circle etc.)
    /// </summary>
    internal class NotSupportedAnnotation : IAnnotation
    {
        private readonly object m_SpecAnn;

        internal NotSupportedAnnotation(object specAnn)
        {
            m_SpecAnn = specAnn;
        }

        public object GetSpecificAnnotation() => m_SpecAnn;

        #region Not Supported
        public object GetNext() => throw new NotSupportedException();
        public Annotation IGetNext() => throw new NotSupportedException();
        public object GetDisplayData() => throw new NotSupportedException();
        public DisplayData IGetDisplayData() => throw new NotSupportedException();
        int IAnnotation.GetType() => throw new NotSupportedException();
        public object IGetSpecificAnnotation() => throw new NotSupportedException();
        public object GetPosition() => throw new NotSupportedException();
        public double IGetPosition() => throw new NotSupportedException();
        public bool SetPosition(double X, double Y, double Z) => throw new NotSupportedException();
        public int GetLeaderCount() => throw new NotSupportedException();
        public object GetLeaderPointsAtIndex(int Index) => throw new NotSupportedException();
        public double IGetLeaderPointsAtIndex(int Index, int PointCount) => throw new NotSupportedException();
        public int GetArrowHeadStyleAtIndex(int Index) => throw new NotSupportedException();
        public int SetArrowHeadStyleAtIndex(int Index, int ArrowHeadStyle) => throw new NotSupportedException();
        public bool GetLeader() => throw new NotSupportedException();
        public bool GetBentLeader() => throw new NotSupportedException();
        public int GetLeaderSide() => throw new NotSupportedException();
        public bool GetSmartArrowHeadStyle() => throw new NotSupportedException();
        public int SetLeader(bool Leader, int LeaderSide, bool SmartArrowHeadStyle, bool BentLeader) => throw new NotSupportedException();
        public string GetName() => throw new NotSupportedException();
        public bool SetName(string NameIn) => throw new NotSupportedException();
        public object GetVisualProperties() => throw new NotSupportedException();
        public int IGetVisualProperties() => throw new NotSupportedException();
        public int IGetAttachedEntityCount() => throw new NotSupportedException();
        public object GetAttachedEntities() => throw new NotSupportedException();
        public object IGetAttachedEntities() => throw new NotSupportedException();
        public object GetAttachedEntityTypes() => throw new NotSupportedException();
        public int IGetAttachedEntityTypes() => throw new NotSupportedException();
        public object GetNext2() => throw new NotSupportedException();
        public Annotation IGetNext2() => throw new NotSupportedException();
        public int GetTextFormatCount() => throw new NotSupportedException();
        public bool GetUseDocTextFormat(int Index) => throw new NotSupportedException();
        public object GetTextFormat(int Index) => throw new NotSupportedException();
        public TextFormat IGetTextFormat(int Index) => throw new NotSupportedException();
        public bool SetTextFormat(int Index, bool UseDoc, object TextFormat) => throw new NotSupportedException();
        public bool ISetTextFormat(int Index, bool UseDoc, TextFormat TextFormat) => throw new NotSupportedException();
        public bool GetLeaderPerpendicular() => throw new NotSupportedException();
        public bool GetLeaderAllAround() => throw new NotSupportedException();
        public int SetLeader2(bool Leader, int LeaderSide, bool SmartArrowHeadStyle, bool BentLeader, bool Perpendicular, bool AllAround) => throw new NotSupportedException();
        public bool Select(bool AppendFlag) => throw new NotSupportedException();
        public bool SelectByMark(bool AppendFlag, int Mark) => throw new NotSupportedException();
        public bool DeSelect() => throw new NotSupportedException();
        public bool Select2(bool Append, int Mark) => throw new NotSupportedException();
        public bool Select3(bool Append, SelectData Data) => throw new NotSupportedException();
        public int GetAttachedEntityCount2() => throw new NotSupportedException();
        public object GetAttachedEntities2() => throw new NotSupportedException();
        public Annotation GetNext3() => throw new NotSupportedException();
        public bool GetDashedLeader() => throw new NotSupportedException();
        public int GetLeaderStyle() => throw new NotSupportedException();
        public int SetLeader3(int LeaderStyle, int LeaderSide, bool SmartArrowHeadStyle, bool Perpendicular, bool AllAround, bool Dashed) => throw new NotSupportedException();
        public bool SetAttachedEntities(object AttachedEnts) => throw new NotSupportedException();
        public bool ISetAttachedEntities(int Count, ref object LpArr) => throw new NotSupportedException();
        public object CheckSpelling(int Options, string Dictionary) => throw new NotSupportedException();
        public bool ConvertToMultiJog(int LeaderNumber, int NumberOfPoints, object PointsData) => throw new NotSupportedException();
        public int GetAttachedEntityCount3() => throw new NotSupportedException();
        public object GetAttachedEntities3() => throw new NotSupportedException();
        public int GetArrowHeadCount() => throw new NotSupportedException();
        public bool GetArrowHeadSizeAtIndex(int Index, ref bool UseDoc, ref double Length, ref double Width, ref double Height) => throw new NotSupportedException();
        public bool SetArrowHeadSizeAtIndex(int Index, bool UseDoc, double Length, double Width, double Height) => throw new NotSupportedException();
        public int GetMultiJogLeaderCount() => throw new NotSupportedException();
        public object GetMultiJogLeaders() => throw new NotSupportedException();
        public MultiJogLeader IGetMultiJogLeaders(int Count) => throw new NotSupportedException();
        public bool IsDangling() => throw new NotSupportedException();
        public string GetStyleName() => throw new NotSupportedException();
        public bool SetStyleName(string StyleName) => throw new NotSupportedException();
        public bool ApplyDefaultStyleAttributes() => throw new NotSupportedException();
        public bool AddOrUpdateStyle(string StyleName, bool BreakLinks) => throw new NotSupportedException();
        public bool DeleteStyle(string StyleName) => throw new NotSupportedException();
        public bool SaveStyle(string StyleName, string PathName) => throw new NotSupportedException();
        public bool LoadStyle(string PathName) => throw new NotSupportedException();
        public bool IsDimXpert() => throw new NotSupportedException();
        public object GetDimXpertFeature() => throw new NotSupportedException();
        public bool SetPosition2(double X, double Y, double Z) => throw new NotSupportedException();
        public object GetPlane() => throw new NotSupportedException();
        public MathTransform GetFlipPlaneTransform() => throw new NotSupportedException();
        public bool CanShowInAnnotationView(string AnnotationViewName) => throw new NotSupportedException();
        public bool CanShowInMultipleAnnotationViews() => throw new NotSupportedException();
        public object GetParagraphs() => throw new NotSupportedException();
        public string GetDimXpertName() => throw new NotSupportedException();
        public bool SetLeaderAttachmentPointAtIndex(int Index, double X, double Y, double Z) => throw new NotSupportedException();
        public int GetPMIType() => throw new NotSupportedException();
        public object GetPMIData() => throw new NotSupportedException();
        public string Layer { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public int LayerOverride { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public int Color { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public int Style { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public int Width { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public int Visible { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public object Owner { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public int OwnerType { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public AnnotationView AnnotationView => throw new NotSupportedException();
        public bool UseDocDispFrame { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public int FrameThickness { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public double FrameThicknessCustom { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public int FrameLineStyle { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public bool UseDocDispLeader { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public int LeaderThickness { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public double LeaderThicknessCustom { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public int LeaderLineStyle { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public double BentLeaderLength { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        #endregion
    }
}
