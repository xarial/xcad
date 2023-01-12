//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Data;

namespace Xarial.XCad.Toolkit.Data
{
    /// <summary>
    /// Manages tags locally on the object
    /// </summary>
    public class LocalTagsManager : ITagsManager
    {
        private readonly Dictionary<string, object> m_Tags;

        public LocalTagsManager()
            : this(StringComparer.CurrentCultureIgnoreCase)
        {
        }

        public LocalTagsManager(StringComparer comparer)
        {
            m_Tags = new Dictionary<string, object>(comparer);
        }

        public bool IsEmpty => !m_Tags.Any();

        public bool Contains(string name) => m_Tags.ContainsKey(name);

        public T Get<T>(string name)
        {
            if (m_Tags.TryGetValue(name, out object val))
            {
                return (T)val;
            }
            else
            {
                throw new KeyNotFoundException("Specified tag is not registered");
            }
        }

        public T Pop<T>(string name)
        {
            var val = Get<T>(name);
            m_Tags.Remove(name);
            return val;
        }

        public void Put<T>(string name, T value)
        {
            m_Tags[name] = value;
        }
    }
}
