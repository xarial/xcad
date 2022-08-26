//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Documents.Services;

namespace Xarial.XCad.SolidWorks.Utils
{
    internal class BatchComponentsInserter
    {
        private class AutoComponentsDispatcher : IDisposable
        {
            private readonly SwDocumentDispatcher m_Dispatcher;
            private readonly ISwDocument3D[] m_NonCommitedDocs;
            private readonly Dictionary<ISwDocument3D, IModelDoc2> m_Map;

            internal AutoComponentsDispatcher(SwAssembly assm, SwComponent[] comps) 
            {
                m_Map = new Dictionary<ISwDocument3D, IModelDoc2>();

                var docs = (SwDocumentCollection)assm.OwnerApplication.Documents;

                m_Dispatcher = docs.Dispatcher;

                m_NonCommitedDocs = comps.Where(c => !c.ReferencedDocument.IsCommitted).Select(c => c.ReferencedDocument).Distinct().ToArray();

                foreach (var nonCommDoc in m_NonCommitedDocs)
                {
                    if (docs.TryFindExistingDocumentByPath(nonCommDoc.Path, out _))
                    {
                        throw new DocumentAlreadyOpenedException(nonCommDoc.Path);
                    }

                    m_Dispatcher.BeginDispatch((SwDocument)nonCommDoc);
                }
            }

            internal void Map(ISwDocument3D doc, IModelDoc2 model) 
            {
                if (!m_Map.ContainsKey(doc))
                {
                    m_Map.Add(doc, model);
                }
                else 
                {
                    throw new Exception("Document is already mapped");
                }
            }

            public void Dispose()
            {
                foreach (var nonCommDoc in m_NonCommitedDocs)
                {
                    if (!m_Map.TryGetValue(nonCommDoc, out IModelDoc2 model))
                    {
                        model = null;
                    }

                    m_Dispatcher.EndDispatch((SwDocument)nonCommDoc, model);
                }
            }
        }

        internal void BatchAdd(SwAssembly assm, SwComponent[] comps, bool commitComps)
        {
            if (comps?.Any() != true)
            {
                throw new ArgumentNullException(nameof(comps));
            }

            SwComponent[] fileComps;
            SwComponent[] virtComps;

            GroupComponents(comps, out fileComps, out virtComps);

            InsertFileComponents(assm, fileComps);
            InsertVirtualComponents(assm, virtComps);

            foreach (var comp in comps)
            {
                if (comp.ReferencedConfiguration != null)
                {
                    comp.BatchComponentBuffer.ReferencedConfiguration = comp.ReferencedConfiguration.Name;
                }
            }

            SetStates(assm, comps);

            if (commitComps)
            {
                foreach (var comp in comps)
                {
                    comp.Commit();
                }
            }
        }

        private void GroupComponents(SwComponent[] comps, out SwComponent[] fileComps, out SwComponent[] virtComps)
        {
            if (comps.Any(c => c.IsCommitted))
            {
                throw new Exception("Only not committed components can be added to the assembly");
            }

            if (comps.Any(c => c.State.HasFlag(ComponentState_e.Envelope)))
            {
                throw new NotSupportedException("Envelope state cannot be set to components");
            }

            fileComps = comps.Where(c => !string.IsNullOrEmpty(c.ReferencedDocument.Path)).ToArray();
            virtComps = comps.Except(fileComps).ToArray();

            if (virtComps.Any(c => !c.State.HasFlag(ComponentState_e.Embedded)))
            {
                throw new Exception($"Component which have referenced documents without path set must have a {nameof(IXComponent.State)} set to {nameof(ComponentState_e.Embedded)}");
            }

            if (fileComps.Any(c => c.State.HasFlag(ComponentState_e.Embedded)))
            {
                throw new Exception($"Component which have referenced documents with path set must not have a {nameof(IXComponent.State)} set to {nameof(ComponentState_e.Embedded)}");
            }
        }

        private void InsertFileComponents(SwAssembly assm, SwComponent[] modelComps)
        {
            if (modelComps.Any())
            {
                var nonCommitedDocs = modelComps.Where(c => !c.ReferencedDocument.IsCommitted).Select(c => c.ReferencedDocument).Distinct().ToArray();

                object[] insertedComps;

                using (var compsDisp = new AutoComponentsDispatcher(assm, modelComps))
                {
                    insertedComps = (object[])assm.Assembly.AddComponents3(
                        modelComps.Select(c => c.ReferencedDocument.Path).ToArray(),
                        modelComps.SelectMany(c => (c.Transformation ?? TransformMatrix.Identity).ToMathTransformData()).ToArray(),
                        new string[modelComps.Length]);
                }

                if (insertedComps == null)
                {
                    throw new Exception("Failed to insert components");
                }

                if (insertedComps.Length != modelComps.Length)
                {
                    throw new Exception("Failed to insert the correct number of components");
                }

                for (int i = 0; i < modelComps.Length; i++)
                {
                    modelComps[i].BatchComponentBuffer = (IComponent2)insertedComps[i];
                }
            }
        }

        private void InsertVirtualComponents(SwAssembly assm, SwComponent[] virtComps)
        {
            if (virtComps.Any())
            {
                var mathUtils = assm.OwnerApplication.Sw.IGetMathUtility();

                foreach (var virtCompsGroup in virtComps.GroupBy(c => c.ReferencedDocument))
                {
                    Component2 swVirtComp;
                    swInsertNewPartErrorCode_e res;
                    var lastFeat = assm.Model.Extension.GetLastFeatureAdded();

                    using (var compsDisp = new AutoComponentsDispatcher(assm, virtComps))
                    {
                        switch (virtCompsGroup.Key)
                        {
                            case IXPart _:
                                res = (swInsertNewPartErrorCode_e)assm.Assembly.InsertNewVirtualPart(null, out swVirtComp);
                                break;

                            case IXAssembly _:
                                res = (swInsertNewPartErrorCode_e)assm.Assembly.InsertNewVirtualAssembly(out swVirtComp);
                                break;

                            default:
                                throw new NotSupportedException();
                        }

                        if (res != swInsertNewPartErrorCode_e.swInsertNewPartError_NoError)
                        {
                            throw new Exception($"Failed to insert virtual component: {res}");
                        }
                        else if (swVirtComp == null)//NOTE: InsertNewVirtualAssembly does not return the pointer to the component
                        {
                            var lastCompFeat = assm.Model.Extension.GetLastFeatureAdded();

                            if (lastFeat != lastCompFeat)
                            {
                                swVirtComp = (Component2)lastCompFeat.GetSpecificFeature2();

                                if (swVirtComp == null)
                                {
                                    throw new Exception("Virtual component is null");
                                }
                            }
                            else
                            {
                                throw new Exception("Failed to find last inserted virtual component");
                            }
                        }

                        compsDisp.Map(virtCompsGroup.Key, (IModelDoc2)swVirtComp.GetModelDoc2());
                    }

                    if (swVirtComp.IsFixed())
                    {
                        SelectComponents(assm.Model, swVirtComp);
                        assm.Assembly.UnfixComponent();
                    }

                    var docVirtComps = virtCompsGroup.ToArray();

                    swVirtComp.Transform2 = (MathTransform)mathUtils.ToMathTransform(docVirtComps.First().Transformation ?? TransformMatrix.Identity);

                    docVirtComps.First().BatchComponentBuffer = swVirtComp;

                    foreach (var virtComp in docVirtComps.Skip(1))
                    {
                        var newComp = CopyComponent(assm.Model, swVirtComp);
                        newComp.Transform2 = (MathTransform)mathUtils.ToMathTransform(virtComp.Transformation ?? TransformMatrix.Identity);
                        virtComp.BatchComponentBuffer = newComp;
                    }
                }
            }
        }

        private void SetStates(SwAssembly assm, SwComponent[] comps)
        {
            var fixedComps = comps.Where(c => c.State.HasFlag(ComponentState_e.Fixed));

            if (fixedComps.Any())
            {
                SelectComponents(assm.Model, fixedComps.Select(c => c.BatchComponentBuffer).ToArray());
                assm.Assembly.FixComponent();
            }

            var suppressedComps = comps.Where(c => c.State.HasFlag(ComponentState_e.Suppressed));

            if (suppressedComps.Any())
            {
                SelectComponents(assm.Model, suppressedComps.Select(c => c.BatchComponentBuffer).ToArray());
                if (!assm.Assembly.SetComponentSuppression((int)swComponentSuppressionState_e.swComponentSuppressed))
                {
                    throw new Exception("Failed to suppress components");
                }
            }

            var hiddenComps = comps.Where(c => c.State.HasFlag(ComponentState_e.Hidden));

            if (hiddenComps.Any())
            {
                SelectComponents(assm.Model, hiddenComps.Select(c => c.BatchComponentBuffer).ToArray());
                assm.Assembly.HideComponent();
            }

            var lightweightComps = comps.Where(c => c.State.HasFlag(ComponentState_e.Lightweight));

            if (lightweightComps.Any())
            {
                SelectComponents(assm.Model, lightweightComps.Select(c => c.BatchComponentBuffer).ToArray());
                if (!assm.Assembly.SetComponentSuppression((int)swComponentSuppressionState_e.swComponentLightweight))
                {
                    throw new Exception("Failed to set components to lightweight");
                }
            }

            var exclFromBomComps = comps.Where(c => c.State.HasFlag(ComponentState_e.ExcludedFromBom));

            if (exclFromBomComps.Any())
            {
                foreach (var exclFromBomComp in exclFromBomComps)
                {
                    exclFromBomComp.BatchComponentBuffer.ExcludeFromBOM = true;
                }
            }
        }

        private IComponent2 CopyComponent(IModelDoc2 assm, IComponent2 src)
        {
            var lastFeat = assm.Extension.GetLastFeatureAdded();

            SelectComponents(assm, src);

            assm.EditCopy();
            assm.Paste();

            var newComp = assm.Extension.GetLastFeatureAdded();

            if (newComp != lastFeat)
            {
                return (IComponent2)newComp.GetSpecificFeature2();
            }
            else
            {
                throw new Exception("Failed to copy the component");
            }
        }

        private void SelectComponents(IModelDoc2 model, params IComponent2[] comps)
        {
            if (model.Extension.MultiSelect2(comps, false, null) != comps.Length)
            {
                throw new Exception("Failed to select components");
            }
        }
    }
}
