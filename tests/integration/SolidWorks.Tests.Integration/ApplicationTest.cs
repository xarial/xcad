using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.SolidWorks;

namespace SolidWorks.Tests.Integration
{
    public class ApplicationTest : IntegrationTests
    {
        [Test]
        public void MaterialDatabaseTest() 
        {
            string dbFileName;
            string category;
            double elasticModulus;
            double poissonRatio;
            double shearModulus;
            double thermalExpansionCoefficient;
            double massDensity;
            double thermalConductivity;
            double specificHeat;
            double tensileStrength;
            double yieldStrength;
            double hardeningFactor;

            var db = (ISwMaterialsDatabase)Application.MaterialDatabases[""];
            dbFileName = Path.GetFileName(db.FilePath);

            var absPcMat = db["ABS PC"];

            category = absPcMat.Category;
            elasticModulus = absPcMat.ElasticModulus;
            poissonRatio = absPcMat.PoissonRatio;
            shearModulus = absPcMat.ShearModulus;
            thermalExpansionCoefficient = absPcMat.ThermalExpansionCoefficient;
            massDensity = absPcMat.MassDensity;
            thermalConductivity = absPcMat.ThermalConductivity;
            specificHeat = absPcMat.SpecificHeat;
            tensileStrength = absPcMat.TensileStrength;
            yieldStrength = absPcMat.YieldStrength;
            hardeningFactor = absPcMat.HardeningFactor;

            Assert.That(string.Equals("solidworks materials.sldmat", dbFileName, StringComparison.CurrentCultureIgnoreCase));
            Assert.AreEqual("Plastics", category);
            
            AssertCompareDoubles(elasticModulus, 2410000000);
            AssertCompareDoubles(poissonRatio, 0.3897);
            AssertCompareDoubles(shearModulus, 862200000);
            AssertCompareDoubles(thermalExpansionCoefficient, double.NaN);
            AssertCompareDoubles(massDensity, 1070);
            AssertCompareDoubles(thermalConductivity, 0.2618);
            AssertCompareDoubles(specificHeat, 1900);
            AssertCompareDoubles(tensileStrength, 40000000);
            AssertCompareDoubles(yieldStrength, double.NaN);
            AssertCompareDoubles(hardeningFactor, double.NaN);
        }
    }
}
