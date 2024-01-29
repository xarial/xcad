//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.Binders;

namespace Xarial.XCad.Utils.PageBuilder.Core
{
    public class AttributeSet : IAttributeSet
    {
        private readonly Dictionary<Type, List<IAttribute>> m_Attributes;

        public IControlDescriptor ControlDescriptor { get; }
        public Type ContextType { get; }
        public string Description { get; }
        public int Id { get; }
        public string Name { get; }
        public object Tag { get; }

        internal AttributeSet(int ctrlId, string ctrlName, string desc, Type contextType, object tag, IControlDescriptor ctrlDesc = null)
        {
            Id = ctrlId;
            Name = ctrlName;
            Description = desc;
            ContextType = contextType;
            ControlDescriptor = ctrlDesc;
            Tag = tag;

            m_Attributes = new Dictionary<Type, List<IAttribute>>();
        }

        public void Add<TAtt>(TAtt att) where TAtt : IAttribute
        {
            if (att == null)
            {
                throw new ArgumentNullException(nameof(att));
            }

            List<IAttribute> atts;

            if (!m_Attributes.TryGetValue(att.GetType(), out atts))
            {
                atts = new List<IAttribute>();
                m_Attributes.Add(att.GetType(), atts);
            }

            atts.Add(att);
        }

        public TAtt Get<TAtt>()
            where TAtt : IAttribute
        {
            return GetAll<TAtt>().First();
        }

        public IEnumerable<TAtt> GetAll<TAtt>()
            where TAtt : IAttribute
        {
            var atts = new List<IAttribute>();

            foreach (var attGrp in m_Attributes.Where(
                a => typeof(TAtt).IsAssignableFrom(a.Key)))
            {
                atts.AddRange(attGrp.Value);
            }

            if (atts.Any())
            {
                return atts.Cast<TAtt>();
            }
            else
            {
                //throw exception
                throw new Exception();
            }
        }

        public bool Has<TAtt>()
            where TAtt : IAttribute
        {
            return m_Attributes.Keys.Any(
                t => typeof(TAtt).IsAssignableFrom(t));
        }
    }
}