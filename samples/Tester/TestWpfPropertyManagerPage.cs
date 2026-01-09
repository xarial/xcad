using System;
using System.Windows;
using Xarial.XCad;
using Xarial.XCad.Toolkit.Windows.UI.PropertyPage;
using Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Templates;

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
