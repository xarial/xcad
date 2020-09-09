using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Annotations
{
    public static class IXDimensionRepositoryExtension
    {
        /// <summary>
        /// Tries to get the value of the dimension
        /// </summary>
        /// <param name="repo">Repository</param>
        /// <param name="dimName">Name of the dimension</param>
        /// <param name="confName">Optional name of the configuration</param>
        /// <returns>Value of the dimension or NaN if dimension does not exist</returns>
        public static double TryGetDimensionValue(this IXDimensionRepository repo, string dimName, string confName = "") 
        {
            if (repo.TryGet(dimName, out IXDimension dim))
            {
                return dim.GetValue(confName);
            }
            else 
            {
                return double.NaN;
            }
        }

        /// <summary>
        /// Tries to set the value of the dimension
        /// </summary>
        /// <param name="repo">Repository</param>
        /// <param name="dimName">Name of the dimension</param>
        /// <param name="value">Value to set</param>
        /// <param name="confName"></param>
        /// <returns></returns>
        public static bool TrySetDimensionValue(this IXDimensionRepository repo, string dimName, double value, string confName = "") 
        {
            if (repo.TryGet(dimName, out IXDimension dim))
            {
                dim.SetValue(value, confName);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
