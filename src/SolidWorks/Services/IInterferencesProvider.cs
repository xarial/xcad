using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Geometry.Exceptions;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Utils;

namespace Xarial.XCad.SolidWorks.Services
{
    /// <summary>
    /// Represents the collection of interferences
    /// </summary>
    public interface IInterferences : IEnumerable<IInterference>, IDisposable 
    {
    }

    /// <summary>
    /// Provides interferences for the specified assembly and components
    /// </summary>
    public interface IInterferencesProvider
    {
        /// <summary>
        /// Gets interferences
        /// </summary>
        /// <param name="assm">Assembly</param>
        /// <param name="comps">Components or null to get all interferences</param>
        /// <param name="visibleOnly">Get visible only components</param>
        /// <returns>Interferences</returns>
        IInterferences GetInterferences(ISwAssembly assm, IXComponent[] comps, bool visibleOnly);
    }

    internal class InterferencesProvider : IInterferencesProvider 
    {
        private class Interferences : IInterferences
        {
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            private readonly UiFreeze m_UiFreeze;
            private readonly SelectionGroup m_SelGrp;
            private readonly IInterferenceDetectionMgr m_InterfDetectMgr;
            private readonly IEnumerable<IInterference> m_Interferences;

            internal Interferences(ISwAssembly assm, IXComponent[] comps, bool visibleOnly)
            {
                m_UiFreeze = new UiFreeze(assm);
                m_InterfDetectMgr = assm.Assembly.InterferenceDetectionManager;
                m_SelGrp = new SelectionGroup((SwDocument)assm, true);

                if (comps?.Any() == true)
                {
                    var swComps = comps.Cast<ISwComponent>().Select(c => c.Component).ToArray();

                    //NOTE: IInterferenceDetectionManager::SetComponentsAndTransforms corrupts the original assembly and its transforms are changed, but seelcting components works correctly
                    m_SelGrp.AddRange(swComps);
                }

                m_InterfDetectMgr.TreatCoincidenceAsInterference = true;
                m_InterfDetectMgr.UseTransform = false;
                m_InterfDetectMgr.IncludeMultibodyPartInterferences = true;
                m_InterfDetectMgr.MakeInterferingPartsTransparent = false;
                m_InterfDetectMgr.NonInterferingComponentDisplay = (int)swNonInterferingComponentDisplay_e.swNonInterferingComponentDisplay_Current;
                m_InterfDetectMgr.ShowIgnoredInterferences = false;
                m_InterfDetectMgr.TreatSubAssembliesAsComponents = false;
                m_InterfDetectMgr.IgnoreHiddenBodies = visibleOnly;

                m_Interferences = ((object[])m_InterfDetectMgr.GetInterferences() ?? Array.Empty<object>()).Cast<IInterference>().ToArray();
            }

            public IEnumerator<IInterference> GetEnumerator() => m_Interferences.GetEnumerator();

            public void Dispose()
            {
                try
                {
                    m_UiFreeze?.Dispose();
                }
                catch 
                {
                }

                try
                {
                    m_SelGrp?.Dispose();
                }
                catch 
                {
                }

                try
                {
                    //interference detection must be released after the suspend selection, otherwise not correct results may be returned
                    m_InterfDetectMgr?.Done();
                }
                catch 
                {
                }
            }
        }

        private bool m_IsInit;

        public InterferencesProvider() 
        {
            m_IsInit = false;
        }

        public IInterferences GetInterferences(ISwAssembly assm, IXComponent[] comps, bool visibleOnly)
        {
            //NOTE: when IInterferenceDetectionManager is invoked first time in the sesssion all the contact interferences (e.g. sheet body volume) are not returned
            //to fix invoking first time with dummy data
            if (!m_IsInit) 
            {
                try
                {
                    var testComps = assm.Configurations.Active
                        .Components.OfType<IXPartComponent>().Where(c =>
                        {
                            var state = c.State;
                            return !state.HasFlag(ComponentState_e.Suppressed) && !state.HasFlag(ComponentState_e.SuppressedIdMismatch);
                        }).Take(2).ToArray();

                    using (new Interferences(assm, testComps, visibleOnly))
                    {
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to init Interference Manager. It should be at least 2 non-suppressed part components", ex);
                }

                m_IsInit = true;
            }

            return new Interferences(assm, comps, visibleOnly);
        }
    }
}
