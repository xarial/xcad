using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Attributes;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.Constructors;
using Xarial.XCad.Utils.PageBuilder.Core;

namespace Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Constructors
{
    [DefaultType(typeof(SpecialTypes.ComplexType))]
    internal class WpfPropertyManagerPageGroupConstructor : GroupConstructor<WpfPropertyManagerPageGroup, WpfPropertyManagerPagePage>
    {
        protected override WpfPropertyManagerPageGroup Create(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            => new WpfPropertyManagerPageGroup(parentGroup, atts, metadata);
    }
}
