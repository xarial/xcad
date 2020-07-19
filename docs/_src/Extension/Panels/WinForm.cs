using System.Drawing;
using System.Windows.Forms;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Documentation.Properties;

namespace Xarial.XCad.Documentation.Extension.Panels
{
    [Title("Windows Form")]
    [Icon(typeof(Resources), nameof(Resources.winforms_icon))]
    public partial class WinForm : Form
    {
        public WinForm()
        {
            InitializeComponent();
        }
    }
}
