using System;
using System.Collections.Generic;
using Xarial.XCad.Documents;
using Xarial.XCad.Features.CustomFeature;
using Xarial.XCad.Features.CustomFeature.Attributes;
using Xarial.XCad.Features.CustomFeature.Services;

namespace Xarial.XCad.Documentation
{
    namespace V1
    {
        //--- OldParams
        [ParametersVersion("1.0", typeof(MacroFeatureParamsVersionConverter))]
        public class MacroFeatureParams
        {
            public string Param1 { get; set; }
            public int Param2 { get; set; }
        }
        //---
    }

    namespace V2
    {
        //--- NewParams
        [ParametersVersion("2.0", typeof(MacroFeatureParamsVersionConverter))]
        public class MacroFeatureParams
        {
            public string Param1A { get; set; }//parameter renamed
            public int Param2 { get; set; }
            public string Param3 { get; set; }//new parameter added
        }
        //---
    }

    //--- Converter
    public class MacroFeatureParamsVersionConverter : ParametersVersionConverter
    {
        private class VersConv_1_0To2_0 : ParameterConverter
        {
            public override Dictionary<string, string> ConvertParameters(IXDocument model, IXCustomFeature feat, Dictionary<string, string> parameters)
            {
                var paramVal = parameters["Param1"];
                parameters.Remove("Param1");
                parameters.Add("Param1A", paramVal);//renaming parameter
                parameters.Add("Param3", "Default");//adding new parameter with default value
                return parameters;
            }
        }

        public MacroFeatureParamsVersionConverter()
        {
            //conversion from version 1.0 to 2.0
            Add(new Version("2.0"), new VersConv_1_0To2_0());
            //add more version converters
        }
    }
    //---
}
