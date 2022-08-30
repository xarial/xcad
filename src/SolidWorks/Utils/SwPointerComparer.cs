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
using System.Text;

namespace Xarial.XCad.SolidWorks.Utils
{
    internal class SwPointerEqualityComparer<T> : IEqualityComparer<T>
        where T : class
    {
        private readonly ISldWorks m_App;

        internal SwPointerEqualityComparer(ISldWorks app) 
        {
            m_App = app;
        }

        public bool Equals(T x, T y)
        {
            if (object.ReferenceEquals(x, y)) 
            {
                return true;
            }

            if (x == null || y == null) 
            {
                return false;
            }

            try
            {
                //Note: ISldWorks::IsSame can crash if pointer is disconnected

                if (IsAlive(x) && IsAlive(y))
                {
                    return m_App.IsSame(x, y) == (int)swObjectEquality.swObjectSame;
                }
                else 
                {
                    return false;
                }
            }
            catch 
            {
                return false;
            }
        }

        protected virtual bool IsAlive(T obj) => true;

        public int GetHashCode(T obj)
        {
            return 0;
        }
    }

    internal class SwModelPointerEqualityComparer : IEqualityComparer<IModelDoc2>
    {
        private readonly List<IModelDoc2> m_DanglingModelPointers;

        private readonly ISldWorks m_App;

        internal SwModelPointerEqualityComparer(ISldWorks app, List<IModelDoc2> danglingModelsPtrs) 
        {
            m_App = app;
            m_DanglingModelPointers = danglingModelsPtrs;
        }

        internal static bool AreEqual(ISldWorks app, IModelDoc2 x, IModelDoc2 y)
            => AreEqual(app, x, y, null);

        internal static bool AreEqual(ISldWorks app, IModelDoc2 x, IModelDoc2 y, List<IModelDoc2> corruptedModels)
        {
            if (object.ReferenceEquals(x, y))
            {
                return true;
            }

            string title1;
            string title2;

            try
            {
                title1 = x.GetTitle();
            }
            catch
            {
                RegisterDanglingModelPointer(x, corruptedModels);
                return false;
            }

            try
            {
                title2 = y.GetTitle();
            }
            catch
            {
                RegisterDanglingModelPointer(y, corruptedModels);
                return false;
            }

            //NOTE: in some cases drawings can have the same title so it might not be safe to only compare by titles
            if (string.Equals(title1, title2, StringComparison.CurrentCultureIgnoreCase))
            {
                return app.IsSame(x, y) == (int)swObjectEquality.swObjectSame;
            }
            else 
            {
                return false;
            }
        }

        private static void RegisterDanglingModelPointer(IModelDoc2 model, List<IModelDoc2> danglingModelPtrs)
        {
            if (danglingModelPtrs != null && !danglingModelPtrs.Contains(model))
            {
                danglingModelPtrs.Add(model);
            }
        }

        public bool Equals(IModelDoc2 x, IModelDoc2 y)
            => AreEqual(m_App, x, y, m_DanglingModelPointers);

        public int GetHashCode(IModelDoc2 obj)
        {
            if (obj is IPartDoc)
            {
                return 1;
            }
            else if (obj is IAssemblyDoc)
            {
                return 2;
            }
            else if (obj is IDrawingDoc)
            {
                return 3;
            }
            else
            {
                return 0;
            }
        }
    }
}
