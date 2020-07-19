using System;
using System.Windows.Forms;
using Xarial.XCad.UI.PropertyPage;

namespace Xarial.XCad.Documentation.PropertyPage.Controls
{
    public partial class CustomWinFormsControl : UserControl, IXCustomControl
    {
        public event Action<IXCustomControl, object> DataContextChanged;

        private CustomControlWinFormsModel m_Model;

        public CustomWinFormsControl()
        {
            InitializeComponent();
        }

        public object DataContext 
        {
            get
            {
                if (m_Model != null)
                {
                    m_Model.Text = lblMessage.Text;
                }
                return m_Model;
            }
            set
            {
                m_Model = (CustomControlWinFormsModel)value;
                lblMessage.Text = m_Model.Text;
                DataContextChanged?.Invoke(this, m_Model);
            }
        }
    }
}
