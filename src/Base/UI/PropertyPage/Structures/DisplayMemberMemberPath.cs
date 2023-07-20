//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

namespace Xarial.XCad.UI.PropertyPage.Structures
{
    /// <summary>
    /// Binding of the display member path
    /// </summary>
    public class DisplayMemberMemberPath 
    {
        /// <summary>
        /// Path to the property holding display member value
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path">Path to display member</param>
        public DisplayMemberMemberPath(string path) 
        {
            Path = path;
        }

        internal string GetDisplayName(object value) 
        {
            if (!string.IsNullOrEmpty(Path) && value != null)
            {
                var prps = Path.Split('.');

                var curVal = value;

                for (int i = 0; i < prps.Length; i++) 
                {
                    curVal = GetPropertyValue(curVal, prps[i]);
                }

                return curVal?.ToString() ?? "";
            }
            else 
            {
                return value?.ToString() ?? "";
            }
        }

        private object GetPropertyValue(object value, string prpName)
        {
            if (value != null)
            {
                var prp = value.GetType().GetProperty(prpName);

                if (prp != null)
                {
                    return prp.GetValue(value, null);
                }
                else
                {
                    return null;
                }
            }
            else 
            {
                return null;
            }
        }
    }
}
