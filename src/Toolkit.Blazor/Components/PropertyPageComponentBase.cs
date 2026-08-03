//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Microsoft.AspNetCore.Components;

namespace Xarial.XCad.Toolkit.Blazor.Components
{
    /// <summary>
    /// Base class shared by <see cref="PropertyPageView{TModel}"/> and <see cref="PropertyPageDialog{TModel}"/>:
    /// subscribes to <see cref="BlazorPropertyManagerPage{TModel}.Invalidated"/> for as long as the
    /// <see cref="Page"/> parameter is set, re-rendering whenever the page's control tree changes, and
    /// unsubscribes on disposal or when a different page instance is supplied
    /// </summary>
    /// <typeparam name="TModel">Data model type</typeparam>
    public abstract class PropertyPageComponentBase<TModel> : ComponentBase, IDisposable
    {
        [Parameter, EditorRequired]
        public BlazorPropertyManagerPage<TModel> Page { get; set; }

        private BlazorPropertyManagerPage<TModel> m_SubscribedPage;

        protected override void OnParametersSet()
        {
            if (!ReferenceEquals(m_SubscribedPage, Page))
            {
                if (m_SubscribedPage != null)
                {
                    m_SubscribedPage.Invalidated -= Refresh;
                }

                m_SubscribedPage = Page;

                if (m_SubscribedPage != null)
                {
                    m_SubscribedPage.Invalidated += Refresh;
                }
            }
        }

        private void Refresh() => InvokeAsync(StateHasChanged);

        public void Dispose()
        {
            if (m_SubscribedPage != null)
            {
                m_SubscribedPage.Invalidated -= Refresh;
            }
        }
    }
}
