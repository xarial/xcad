//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.SolidWorks.Documents;

namespace Xarial.XCad.SolidWorks
{
    /// <summary>
    /// Factory for xCAD objects
    /// </summary>
    public static class SwObjectFactory 
    {
        /// <summary>
        /// Wraps the SOLIDWORKS specific dispatch to xCAD object
        /// </summary>
        /// <typeparam name="TObj">Type of the object</typeparam>
        /// <param name="disp">SOLIDWORKS specific dispatch</param>
        /// <param name="doc">Owner document</param>
        /// <returns>xCAD specific object</returns>
        public static TObj FromDispatch<TObj>(object disp, ISwDocument doc)
            where TObj : IXObject
        {
            if (typeof(ISwSelObject).IsAssignableFrom(typeof(TObj))) 
            {
                return (TObj)SwObject.FromDispatch(disp, doc, d => new SwSelObject(disp, doc));
            }
            else
            {
                return (TObj)SwObject.FromDispatch(disp, doc, d => new SwObject(disp, doc));
            }
        }
    }
}