//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Xarial.XCad.Annotations;

namespace Xarial.XCad.SolidWorks
{
    /// <summary>
    /// SOLIDWORKS specific material
    /// </summary>
    public interface ISwMaterial : IXMaterial
    {
    }

    [DebuggerDisplay("{" + nameof(Name) + "}")]
    internal class SwMaterial : ISwMaterial
    {
        [DebuggerDisplay("{" + nameof(Name) + "}")]
        private class SwMaterialCrossHatch : IXCrossHatch
        {
            public string Name { get; }
            public double Angle { get; }
            public double Scale { get; }

            internal SwMaterialCrossHatch(string name, double angle, double scale) 
            {
                Name = name;
                Angle = angle;
                Scale = scale;
            }
        }

        public string Name { get; }

        public IXMaterialsDatabase Database => m_Database;

        public string Category => m_MaterialNodeLazy.Value?.ParentNode?.Attributes["name"]?.Value;
        public double ElasticModulus => GetPhysicalPropertyValue("EX");
        public double PoissonRatio => GetPhysicalPropertyValue("NUXY");
        public double ShearModulus => GetPhysicalPropertyValue("GXY");
        public double ThermalExpansionCoefficient => GetPhysicalPropertyValue("ALPX");
        public double MassDensity => GetPhysicalPropertyValue("DENS");
        public double ThermalConductivity => GetPhysicalPropertyValue("KX");
        public double SpecificHeat => GetPhysicalPropertyValue("C");
        public double TensileStrength => GetPhysicalPropertyValue("SIGXT");
        public double YieldStrength => GetPhysicalPropertyValue("SIGYLD");
        public double HardeningFactor => GetPhysicalPropertyValue("RK");

        public bool IsCommitted => true;

        public IXCrossHatch CrossHatch 
        {
            get 
            {
                var crossHatchNode = m_XHatchPropertiesNodeLazy.Value;

                if (crossHatchNode != null)
                {
                    var name = crossHatchNode.Attributes["name"]?.Value;

                    var angleStr = crossHatchNode.Attributes["angle"]?.Value;

                    if (string.IsNullOrEmpty(angleStr) || !double.TryParse(angleStr, out var angle))
                    {
                        angle = double.NaN;
                    }

                    var scaleStr = crossHatchNode.Attributes["scale"]?.Value;

                    if (string.IsNullOrEmpty(scaleStr) || !double.TryParse(scaleStr, out var scale))
                    {
                        scale = double.NaN;
                    }

                    return new SwMaterialCrossHatch(name, angle, scale);
                }
                else 
                {
                    return null;
                }
            }
        }

        private readonly SwMaterialsDatabase m_Database;

        private readonly Lazy<XmlNode> m_MaterialNodeLazy;
        private readonly Lazy<XmlNode> m_MaterialPhysicalPropertiesNodeLazy;
        private readonly Lazy<XmlNode> m_XHatchPropertiesNodeLazy;

        private double GetPhysicalPropertyValue(string name)
        {
            var valStr = m_MaterialPhysicalPropertiesNodeLazy.Value?[name]?.Attributes["value"]?.Value;

            if (!string.IsNullOrEmpty(valStr) && double.TryParse(valStr, out var val))
            {
                return val;
            }
            else
            {
                return double.NaN;
            }
        }

        internal SwMaterial(string name, SwMaterialsDatabase database) 
            : this(name, database, () => database?.FindMaterialXmlNode(name))
        {
        }

        internal SwMaterial(XmlNode matXmlNode, SwMaterialsDatabase database)
            : this(matXmlNode.Attributes["name"]?.Value, database, () => matXmlNode)
        {
        }

        private SwMaterial(string name, SwMaterialsDatabase database, Func<XmlNode> matNodeFunc)
        {
            Name = name;
            m_Database = database;
            m_MaterialNodeLazy = new Lazy<XmlNode>(matNodeFunc);
            m_MaterialPhysicalPropertiesNodeLazy = new Lazy<XmlNode>(() => m_MaterialNodeLazy.Value?["physicalproperties"]);
            m_XHatchPropertiesNodeLazy = new Lazy<XmlNode>(() => m_MaterialNodeLazy.Value?["xhatch"]);
        }

        public void Commit(CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}
