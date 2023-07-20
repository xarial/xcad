//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
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

namespace Xarial.XCad.SolidWorks
{
    public interface ISwMaterial : IXMaterial
    {
    }

    [DebuggerDisplay("{" + nameof(Name) + "}")]
    internal class SwMaterial : ISwMaterial
    {
        public string Name { get; }

        public IXMaterialsDatabase Database => m_Database;

        public string Category => m_MaterialNodeLazy.Value.ParentNode?.Attributes["name"]?.Value;
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

        private readonly SwMaterialsDatabase m_Database;

        private readonly Lazy<XmlNode> m_MaterialNodeLazy;
        private readonly Lazy<XmlNode> m_MaterialPhysicalPropertiesNodeLazy;

        private double GetPhysicalPropertyValue(string name)
        {
            var valStr = m_MaterialPhysicalPropertiesNodeLazy.Value[name]?.Attributes["value"]?.Value;

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
            : this(name, database, () => database.FindMaterialXmlNode(name))
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
            m_MaterialPhysicalPropertiesNodeLazy = new Lazy<XmlNode>(() => m_MaterialNodeLazy.Value["physicalproperties"]);
        }

        public void Commit(CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}
