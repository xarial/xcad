using System;
using System.Windows;
using Xarial.XCad;
using Xarial.XCad.BimVision.UI.PropertyPage.Toolkit.Templates;
using Xarial.XCad.Toolkit.Windows.UI.PropertyPage;

namespace Tester
{
    public class TestWpfPropertyManagerPage<TModel> : WpfPropertyManagerPage<TModel>
    {
        private readonly Window m_Wnd;

        public TestWpfPropertyManagerPage(IXApplication app, IServiceProvider svcProvider) : base(app, svcProvider)
        {
            m_Wnd = new Window();
        }

        protected override void ClosePage(bool cancel)
        {
            m_Wnd.Hide();
        }

        protected override void ShowPage(TModel model, PropertyManagerPageLayout layout, string name)
        {
            m_Wnd.Content = layout;
            m_Wnd.Title = name;
            m_Wnd.Show();
        }
    }
}
