//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Features.CustomFeature;

namespace Xarial.XCad.Toolkit.CustomFeature
{
    /// <summary>
    /// Manages the cache of custom feature servers
    /// </summary>
    public static class CustomFeatureDefinitionInstanceCache
    {
        private static Dictionary<Type, IXCustomFeatureDefinition> m_Instances
            = new Dictionary<Type, IXCustomFeatureDefinition>();

        /// <summary>
        /// Registers instance of the custom feature server
        /// </summary>
        /// <param name="inst">Instance to register</param>
        public static void RegisterInstance(IXCustomFeatureDefinition inst)
        {
            var type = inst.GetType();

            if (!m_Instances.ContainsKey(type))
            {
                m_Instances.Add(type, inst);
            }
        }

        /// <summary>
        /// Returns the instance of custom feature server
        /// </summary>
        /// <param name="defType">Type of custom feature definition</param>
        /// <returns>Instance of the custom feature server</returns>
        public static IXCustomFeatureDefinition GetInstance(Type defType)
        {
            if (!typeof(IXCustomFeatureDefinition).IsAssignableFrom(defType)) 
            {
                throw new InvalidCastException($"{defType.FullName} must implement {typeof(IXCustomFeatureDefinition).FullName}");
            }

            IXCustomFeatureDefinition inst;

            if (!m_Instances.TryGetValue(defType, out inst))
            {
                //TODO: validate that default constructor is available

                inst = (IXCustomFeatureDefinition)Activator.CreateInstance(defType);
                RegisterInstance(inst);
            }

            return inst;
        }
    }
}
