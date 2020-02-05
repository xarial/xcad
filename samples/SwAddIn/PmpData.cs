using System.Collections.Generic;
using System.Runtime.InteropServices;
using Xarial.XCad.Features.CustomFeature.Attributes;
using Xarial.XCad.Features.CustomFeature.Enums;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.SolidWorks.UI.PropertyPage;
using Xarial.XCad.SolidWorks;

namespace SwAddInExample
{
    public enum Opts
    {
        Opt1,
        Opt2
    }

    [ComVisible(true)]
    public class PmpData : SwPropertyManagerPageHandler
    {
        public string Text { get; set; }

        [ParameterDimension(CustomFeatureDimensionType_e.Linear)]
        public double Number { get; set; }

        [OptionBox]
        public Opts Options { get; set; }

        [SelectionBoxOptions(SelectType_e.Faces)]
        public List<SwSelObject> Selection { get; set; }
    }
}
