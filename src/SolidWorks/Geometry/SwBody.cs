//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Media.Media3D;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Exceptions;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Geometry.Curves;
using Xarial.XCad.SolidWorks.Geometry.Primitives;
using Xarial.XCad.SolidWorks.Utils;

namespace Xarial.XCad.SolidWorks.Geometry
{
    public interface ISwBody : ISwSelObject, IXBody, ISupportsResilience<ISwBody>
    {
        IBody2 Body { get; }

        new ISwComponent Component { get; }
    }

    [DebuggerDisplay("{" + nameof(Name) + "}")]
    internal class SwBody : SwSelObject, ISwBody
    {       
        IXComponent IXBody.Component => Component;
        IXObject ISupportsResilience.CreateResilient() => CreateResilient();

        public virtual IBody2 Body 
        {
            get 
            {
                if (IsResilient)
                {
                    try
                    {
                        var testNameAlive = m_Body.Name;

                        if (string.IsNullOrEmpty(testNameAlive) && !m_Body.IsTemporaryBody()) 
                        {
                            throw new Exception("Permanent body is not alive");
                        }
                    }
                    catch 
                    {
                        var body = (IBody2)OwnerDocument.Model.Extension.GetObjectByPersistReference3(m_PersistId, out _);

                        if (body != null)
                        {
                            m_Body = body;
                        }
                        else 
                        {
                            throw new NullReferenceException("Pointer to the body cannot be restored");
                        }
                    }
                }

                return m_Body;
            }
        }

        public override object Dispatch => Body;

        public virtual bool Visible
        {
            get => Body.Visible;
            set => Body.HideBody(!value);
        }

        public virtual string Name
        {
            get => Body.Name;
            set => Body.Name = value;
        }

        public ISwComponent Component 
        {
            get
            {
                var comp = (Body.IGetFirstFace() as IEntity)?.GetComponent() as IComponent2;

                if (comp != null)
                {
                    return OwnerDocument.CreateObjectFromDispatch<ISwComponent>(comp);
                }
                else
                {
                    if (IsResilient)
                    {
                        //NOTE: in some cases component reference may be lost
                        return m_PersistComponent;
                    }
                    else
                    {
                        return m_ContextComponent;
                    }
                }
            }
        }

        public Color? Color
        {
            get => SwColorHelper.FromMaterialProperties(Body.MaterialPropertyValues2 as double[]);
            set
            {
                if (value.HasValue)
                {
                    var matPrps = SwColorHelper.ToMaterialProperties(value.Value);
                    Body.MaterialPropertyValues2 = matPrps;
                }
                else 
                {
                    SwColorHelper.GetColorScope(Component?.Component, 
                        out swInConfigurationOpts_e confOpts, out string[] confs);

                    Body.RemoveMaterialProperty((int)confOpts, confs);
                }
            }
        }

        public IEnumerable<IXFace> Faces 
        {
            get 
            {
                var face = Body.IGetFirstFace();

                while (face != null) 
                {
                    yield return OwnerApplication.CreateObjectFromDispatch<ISwFace>(face, OwnerDocument);
                    face = face.IGetNextFace();
                }
            }
        }

        public IEnumerable<IXEdge> Edges
        {
            get
            {
                var edges = Body.GetEdges() as object[];

                if (edges != null) 
                {
                    foreach (IEdge edge in edges) 
                    {
                        yield return OwnerApplication.CreateObjectFromDispatch<ISwEdge>(edge, OwnerDocument);
                    }
                }
            }
        }

        public IXMaterial Material
        {
            get
            {
                var confName = "";

                var comp = Component;

                if (comp != null)
                {
                    confName = comp.ReferencedConfiguration.Name;
                }

                var materialName = Body.GetMaterialPropertyName(confName, out var database);

                if (!string.IsNullOrEmpty(materialName))
                {
                    return new SwMaterial(materialName, OwnerApplication.MaterialDatabases[database]);
                }
                else
                {
                    return null;
                }
            }
            set 
            {
                string confName;

                var comp = Component;

                if (comp != null)
                {
                    confName = comp.ReferencedConfiguration.Name;
                }
                else
                {
                    confName = ((SwDocument3D)OwnerDocument).Configurations.Active.Name;
                }

                swBodyMaterialApplicationError_e res;

                using (var sel = new SelectionGroup(OwnerDocument, true))
                {
                    //NOTE: API only works if body is selected, otherwise returned result is correct, but material is not set
                    sel.Add(Body);

                    if (value != null)
                    {
                        res = (swBodyMaterialApplicationError_e)Body.SetMaterialProperty(confName, value.Database.Name, value.Name);
                    }
                    else
                    {
                        res = (swBodyMaterialApplicationError_e)Body.SetMaterialProperty(confName, "", "");
                    }

                    //NOTE: external reference body (e.g. from the inserted part) cannot be assigned using the above API, but still can be assigned with an old method
                    if (res == swBodyMaterialApplicationError_e.swBodyMaterialApplicationError_ExternalReference) 
                    {
                        if (OwnerDocument is IXPart)
                        {
                            var part = ((ISwPart)OwnerDocument).Part;

                            if (value != null)
                            {
                                part.SetMaterialPropertyName2(confName, value.Database.Name, value.Name);
                            }
                            else
                            {
                                part.SetMaterialPropertyName2(confName, "", "");
                            }

                            var materialName = Body.GetMaterialPropertyName(confName, out var database);

                            if (string.Equals(database, SwMaterialsDatabase.SYSTEM_DB_NAME, StringComparison.CurrentCultureIgnoreCase))
                            {
                                database = "";
                            }

                            if (value != null)
                            {
                                if (string.Equals(value.Name, materialName, StringComparison.CurrentCultureIgnoreCase)
                                    && (string.Equals(value.Database.Name, database, StringComparison.CurrentCultureIgnoreCase)
                                        || string.Equals(System.IO.Path.GetFileNameWithoutExtension(value.Database.Name), System.IO.Path.GetFileNameWithoutExtension(database), StringComparison.CurrentCultureIgnoreCase)))
                                {
                                    return;
                                }
                                else
                                {
                                    throw new Exception("Failed to change the material of the external reference body");
                                }
                            }
                            else 
                            {
                                if (string.IsNullOrEmpty(materialName) && string.IsNullOrEmpty(database))
                                {
                                    return;
                                }
                                else 
                                {
                                    throw new Exception("Failed to remove the material of the external reference body");
                                }
                            }
                        }
                    }
                }

                if (res != swBodyMaterialApplicationError_e.swBodyMaterialApplicationError_NoError)
                {
                    throw new Exception($"Failed to set material. Error code: {res}");
                }
            }
        }

        public bool IsResilient { get; private set; }

        private byte[] m_PersistId;
        private ISwComponent m_PersistComponent;
        private ISwComponent m_ContextComponent;

        private IBody2 m_Body;

        internal SwBody(IBody2 body, SwDocument doc, SwApplication app)
            : base(body, doc, app ?? doc?.OwnerApplication)
        {
            m_Body = body;
        }

        internal override void Select(bool append, ISelectData selData)
        {
            if (!Body.Select2(append, (SelectData)selData))
            {
                throw new Exception("Failed to select body");
            }
        }

        /// <summary>
        /// Bodies returned from the IComponent2 are not in the component's context. This method assigns this explicitly
        /// </summary>
        /// <param name="comp">context component</param>
        internal void SetContextComponent(ISwComponent comp) 
        {
            m_ContextComponent = comp;
        }

        public IXMemoryBody Copy()
        {
            var copy = Body.CreateCopy(OwnerApplication);

            return OwnerApplication.CreateObjectFromDispatch<SwTempBody>(copy, OwnerDocument);
        }

        public virtual ISwBody CreateResilient()
        {
            if (OwnerDocument == null) 
            {
                throw new NullReferenceException("Owner document is not set");
            }

            var id = (byte[])OwnerDocument.Model.Extension.GetPersistReference3(Body);

            if (id != null)
            {
                var body = OwnerDocument.CreateObjectFromDispatch<SwBody>(Body);
                body.MakeResilient(id);
                return body;
            }
            else 
            {
                throw new Exception("Failed to create resilient body");
            }
        }

        private void MakeResilient(byte[] persistId) 
        {
            IsResilient = true;
            m_PersistId = persistId;
            m_PersistComponent = Component;
        }
    }

    public interface ISwSheetBody : ISwBody, IXSheetBody
    {
    }

    internal class SwSheetBody : SwBody, ISwSheetBody
    {
        internal SwSheetBody(IBody2 body, SwDocument doc, SwApplication app) : base(body, doc, app)
        {
        }
    }

    public interface ISwPlanarSheetBody : ISwSheetBody, IXPlanarSheetBody, ISwPlanarRegion
    {
    }

    internal class SwPlanarSheetBody : SwSheetBody, ISwPlanarSheetBody
    {
        IXLoop IXRegion.OuterLoop { get => OuterLoop; set => throw new NotSupportedException(); }
        IXLoop[] IXRegion.InnerLoops { get => InnerLoops; set => throw new NotSupportedException(); }

        internal SwPlanarSheetBody(IBody2 body, SwDocument doc, SwApplication app) : base(body, doc, app)
        {
        }

        public Plane Plane => this.GetPlane();

        public virtual ISwTempPlanarSheetBody PlanarSheetBody => (ISwTempPlanarSheetBody)this.Copy();

        public ISwLoop OuterLoop { get => this.GetOuterLoop(); set => throw new NotImplementedException(); }
        public ISwLoop[] InnerLoops { get => this.GetInnerLoops(); set => throw new NotImplementedException(); }
    }

    internal static class ISwPlanarSheetBodyExtension 
    {
        internal static Plane GetPlane(this ISwPlanarSheetBody body)
        {
            var planarFace = ((SwObject)body).OwnerApplication.CreateObjectFromDispatch<SwPlanarFace>(
                body.Body.IGetFirstFace(), ((SwObject)body).OwnerDocument);

            return planarFace.Definition.Plane;
        }

        internal static SwLoop GetOuterLoop(this ISwPlanarSheetBody body) => IterateLoops((SwFace)body.Faces.First()).First(l => l.Loop.IsOuter());
        internal static SwLoop[] GetInnerLoops(this ISwPlanarSheetBody body) => IterateLoops((SwFace)body.Faces.First()).Where(l => !l.Loop.IsOuter()).ToArray();

        private static IEnumerable<SwLoop> IterateLoops(SwFace face) 
        {
            var loops = (object[])face.Face.GetLoops();

            for (int i = 0; i < loops.Length; i++)
            {
                var loop = (ILoop2)loops[i];
                yield return face.OwnerApplication.CreateObjectFromDispatch<SwLoop>(loop, face.OwnerDocument);
            }
        }
    }

    public interface ISwSolidBody : IXSolidBody, ISwBody
    {
    }

    internal class SwSolidBody : SwBody, ISwBody, ISwSolidBody
    {
        internal SwSolidBody(IBody2 body, SwDocument doc, SwApplication app) : base(body, doc, app)
        {
        }

        public double Volume => this.GetVolume();
    }

    internal class SwSheetMetalBody : SwSolidBody, IXSheetMetalBody
    {
        internal SwSheetMetalBody(IBody2 body, SwDocument doc, SwApplication app) : base(body, doc, app)
        {
            if (!body.IsSheetMetal()) 
            {
                throw new Exception("Body is not sheet metal");
            }
        }
    }

    internal static class ISwBodyExtension
    { 
        public static double GetVolume(this ISwBody body)
        {
            var massPrps = body.Body.GetMassProperties(0) as double[];
            return massPrps[3];
        }
    }

    public interface ISwWireBody : ISwBody, IXWireBody 
    {
    }

    internal class SwWireBody : SwBody, ISwWireBody
    {
        internal SwWireBody(IBody2 body, SwDocument doc, SwApplication app) : base(body, doc, app)
        {
        }

        public IXSegment[] Segments { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}