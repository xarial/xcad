//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Attributes;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.Core;
using Xarial.XCad.Utils.PageBuilder.Exceptions;

namespace Xarial.XCad.Utils.PageBuilder.Internal
{
    internal class ConstructorsContainer<TPage, TGroup>
        where TPage : IPage
        where TGroup : IGroup
    {
        /// <summary>
        /// Constructors for the default data types (i.e. int, double, bool etc.)
        /// </summary>
        private readonly Dictionary<Type, IPageElementConstructor> m_DefaultConstructors;

        /// <summary>
        /// Constructors for the special types (i.e. complex, enums, etc.)
        /// </summary>
        private readonly Dictionary<Type, IPageElementConstructor> m_SpecialTypeConstructors;

        /// <summary>
        /// Specific constructor for specific data types
        /// </summary>
        private readonly Dictionary<Type, IPageElementConstructor> m_SpecificConstructors;

        internal ConstructorsContainer(params IPageElementConstructor[] constructors)
        {
            m_DefaultConstructors = new Dictionary<Type, IPageElementConstructor>();
            m_SpecificConstructors = new Dictionary<Type, IPageElementConstructor>();
            m_SpecialTypeConstructors = new Dictionary<Type, IPageElementConstructor>();

            foreach (var constr in constructors)
            {
                var dataTypeAtts = constr.GetType().GetCustomAttributes(
                    typeof(DefaultTypeAttribute), true).OfType<DefaultTypeAttribute>();

                var isDefaultConstr = dataTypeAtts.Any();

                if (isDefaultConstr)
                {
                    foreach (var dataTypeAtt in dataTypeAtts)
                    {
                        var type = dataTypeAtt.Type;

                        if (typeof(SpecialTypes.ISpecialType).IsAssignableFrom(type))
                        {
                            if (!m_SpecialTypeConstructors.ContainsKey(type))
                            {
                                m_SpecialTypeConstructors.Add(type, constr);
                            }
                            else
                            {
                                throw new OverdefinedConstructorException(constr.GetType(), type);
                            }
                        }
                        else
                        {
                            if (!m_DefaultConstructors.ContainsKey(dataTypeAtt.Type))
                            {
                                m_DefaultConstructors.Add(dataTypeAtt.Type, constr);
                            }
                            else
                            {
                                throw new OverdefinedConstructorException(constr.GetType(), dataTypeAtt.Type);
                            }
                        }
                    }
                }
                else
                {
                    if (!m_SpecificConstructors.ContainsKey(constr.GetType()))
                    {
                        m_SpecificConstructors.Add(constr.GetType(), constr);
                    }
                    else
                    {
                        throw new OverdefinedConstructorException(constr.GetType(), constr.GetType());
                    }
                }
            }
        }

        internal IControl CreateElement(Type type, IGroup parent, IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (atts == null)
            {
                throw new ArgumentNullException(nameof(atts));
            }

            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            var constr = FindConstructor(type, atts);

            return constr.Create(parent, atts, metadata, ref numberOfUsedIds);
        }

        private IPageElementConstructor FindConstructor(Type type, IAttributeSet atts)
        {
            if (atts == null)
            {
                throw new ArgumentNullException(nameof(atts));
            }

            IPageElementConstructor constr = null;

            if (atts.Has<ISpecificConstructorAttribute>())
            {
                var constrType = atts.Get<ISpecificConstructorAttribute>().ConstructorType;

                var constrs = m_SpecificConstructors.Where(c => constrType.IsAssignableFrom(c.Key));

                if (constrs.Count() == 1)
                {
                    constr = constrs.First().Value;
                }
                else if (!constrs.Any())
                {
                    throw new ConstructorNotFoundException(type, "Specific constructor is not registered");
                }
                else
                {
                    throw new ConstructorNotFoundException(type, "Too many constructors registered");
                }
            }
            else
            {
                if (!m_DefaultConstructors.TryGetValue(type, out constr))
                {
                    constr = m_DefaultConstructors.FirstOrDefault(
                        t => t.Key.IsAssignableFrom(type)).Value;

                    if (constr == null)
                    {
                        foreach (var specType in SpecialTypes.FindMathingSpecialTypes(type))
                        {
                            if (m_SpecialTypeConstructors.TryGetValue(specType, out constr))
                            {
                                break;
                            }
                        }
                    }
                }
            }

            if (constr != null)
            {
                return constr;
            }
            else
            {
                throw new ConstructorNotFoundException(type);
            }
        }
    }
}