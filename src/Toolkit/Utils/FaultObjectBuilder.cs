using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Xarial.XCad.Toolkit.Utils
{
    /// <summary>
    /// Creates fault object of the specified type
    /// </summary>
    public class FaultObjectFactory
    {
        private readonly AssemblyBuilder m_AssmBuilder;
        private readonly ModuleBuilder m_ModuleBuilder;

        private readonly Dictionary<Type, Type> m_Cache;

        public FaultObjectFactory() 
        {
            m_AssmBuilder = AssemblyBuilder.DefineDynamicAssembly(
                    new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);

            m_ModuleBuilder = m_AssmBuilder.DefineDynamicModule(Guid.NewGuid().ToString());

            m_Cache = new Dictionary<Type, Type>();
        }

        public T CreateFaultObject<T>()
            where T : IXObject 
            => (T)CreateFaultObject(typeof(T));

        public IFaultObject CreateFaultObject(Type type)
        {
            if (!m_Cache.TryGetValue(type, out Type impType))
            {
                if (!type.IsInterface)
                {
                    throw new NotSupportedException($"Only interfaces are supported");
                }

                if (!typeof(IXObject).IsAssignableFrom(type))
                {
                    throw new NotSupportedException($"Only interfaces derived from {nameof(IXObject)} are supported");
                }

                var typeBuilder = m_ModuleBuilder.DefineType
                    (type.Name + "Fault", TypeAttributes.Class | TypeAttributes.Public);

                typeBuilder.AddInterfaceImplementation(type);
                typeBuilder.AddInterfaceImplementation(typeof(IFaultObject));

                ImplementAllMethods(type, typeBuilder, new List<Type>(), new List<MethodInfo>());

                impType = typeBuilder.CreateType();

                m_Cache.Add(type, impType);
            }

            return (IFaultObject)Activator.CreateInstance(impType);
        }

        private void ImplementAllMethods(Type type, TypeBuilder typeBuilder, List<Type> processedInterfaces, List<MethodInfo> processedMethods)
        {
            if (!processedInterfaces.Contains(type))
            {
                processedInterfaces.Add(type);

                foreach (var method in type.GetMethods())
                {
                    ImplementMethod(method, typeBuilder, processedMethods);
                }

                foreach (var subInterface in type.GetInterfaces())
                {
                    ImplementAllMethods(subInterface, typeBuilder, processedInterfaces, processedMethods);
                }
            }
        }

        private void ImplementMethod(MethodInfo methodInfo, TypeBuilder typeBuilder, List<MethodInfo> processedMethods)
        {
            if (!processedMethods.Contains(methodInfo))
            {
                var returnType = methodInfo.ReturnType;

                var paramTypes = new List<Type>();

                foreach (var parameterInfo in methodInfo.GetParameters())
                {
                    paramTypes.Add(parameterInfo.ParameterType);
                }

                var methodBuilder = typeBuilder.DefineMethod
                    (methodInfo.Name, MethodAttributes.Public |
                    MethodAttributes.Virtual, returnType, paramTypes.ToArray());

                var ilGenerator = methodBuilder.GetILGenerator();

                ilGenerator.ThrowException(typeof(NotSupportedException));

                typeBuilder.DefineMethodOverride(methodBuilder, methodInfo);

                processedMethods.Add(methodInfo);
            }
        }
    }
}
