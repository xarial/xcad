using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xarial.XCad.Utils.Reflection;

namespace Toolkit.Tests
{
    public class TypeExtensionTests
    {
        public class UserControl1 : UserControl
        {
        }

        [ComVisible(false)]
        public class UserControl2 : UserControl
        {
        }

        [ComVisible(true)]
        public class UserControl3 : UserControl
        {
        }

        [Test]
        public void IsComVisibleTest()
        {
            var r1 = typeof(UserControl1).IsComVisible();
            var r2 = typeof(UserControl2).IsComVisible();
            var r3 = typeof(UserControl3).IsComVisible();
            var r4 = CreateTypeInComVisibleAssm().IsComVisible();

            Assert.IsFalse(r1);
            Assert.IsFalse(r2);
            Assert.IsTrue(r3);
            Assert.IsTrue(r4);
        }

        private Type CreateTypeInComVisibleAssm()
        {
            var assmName = new AssemblyName(Guid.NewGuid().ToString());
            var assmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assmName, AssemblyBuilderAccess.RunAndSave);

            var moduleBuilder = assmBuilder.DefineDynamicModule(assmName.Name, assmName.Name + ".dll");

            var typeBuilder = moduleBuilder.DefineType("UserControl4", TypeAttributes.Public);

            var attBuilder = new CustomAttributeBuilder(typeof(ComVisibleAttribute).GetConstructor(new Type[] { typeof(bool) }), new object[] { true });

            assmBuilder.SetCustomAttribute(attBuilder);

            typeBuilder.SetParent(typeof(UserControl));

            return typeBuilder.CreateType();
        }
    }
}
