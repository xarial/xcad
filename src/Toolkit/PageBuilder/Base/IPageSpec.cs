//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Enums;

namespace Xarial.XCad.Utils.PageBuilder.Base
{
    /// <summary>
    /// Specification of the page
    /// </summary>
    public interface IPageSpec
    {
        /// <summary>
        /// Page title
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Page options
        /// </summary>
        PageOptions_e Options { get; }

        /// <summary>
        /// Page buttons
        /// </summary>
        PageButtons_e Buttons { get; }

        /// <summary>
        /// Lock page strategy
        /// </summary>
        LockPageStrategy_e LockPageStrategy { get; }
    }

    /// <summary>
    /// Additional methods of <see cref="IPageSpec"/>
    /// </summary>
    public static class PageSpecExtension 
    {
        private class PageSpecAttributeSet : IAttributeSet
        {
            private readonly IAttributeSet m_BaseAttSet;

            public Type ContextType => m_BaseAttSet.ContextType;
            public string Description => m_BaseAttSet.Description;
            public int Id => m_BaseAttSet.Id;
            public string Name { get; }
            public object Tag => m_BaseAttSet.Tag;
            public IControlDescriptor ControlDescriptor => m_BaseAttSet.ControlDescriptor;

            public void Add<TAtt>(TAtt att) where TAtt : IAttribute
                => m_BaseAttSet.Add<TAtt>(att);

            public TAtt Get<TAtt>() where TAtt : IAttribute
                => m_BaseAttSet.Get<TAtt>();

            public IEnumerable<TAtt> GetAll<TAtt>() where TAtt : IAttribute
                => m_BaseAttSet.GetAll<TAtt>();

            public bool Has<TAtt>() where TAtt : IAttribute
                => m_BaseAttSet.Has<TAtt>();

            internal PageSpecAttributeSet(IAttributeSet baseAttSet, IPageSpec pageSpec)
            {
                m_BaseAttSet = baseAttSet;

                if (!Has<PageOptionsAttribute>())
                {
                    //TODO: process pageSpec.Icon
                    Add(new PageOptionsAttribute(pageSpec.Options));
                }

                if (!Has<PageButtonsAttribute>())
                {
                    Add(new PageButtonsAttribute(pageSpec.Buttons));
                }

                if (!Has<LockedPageAttribute>())
                {
                    Add(new LockedPageAttribute(pageSpec.LockPageStrategy));
                }

                if (string.IsNullOrEmpty(baseAttSet.Name)
                    || baseAttSet.Name == ContextType.Name)
                {
                    Name = pageSpec.Title;
                }
                else
                {
                    Name = baseAttSet.Name;
                }
            }
        }

        /// <summary>
        /// Creates attributes set based on the page specification
        /// </summary>
        /// <param name="pageSpec">Page specification</param>
        /// <param name="baseAttSet">Attributes set</param>
        /// <returns></returns>
        public static IAttributeSet ToAttributeSet(this IPageSpec pageSpec, IAttributeSet baseAttSet)
            => new PageSpecAttributeSet(baseAttSet, pageSpec);
    }
}