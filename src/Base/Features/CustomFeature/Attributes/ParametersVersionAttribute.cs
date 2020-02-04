//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.Features.CustomFeature.Services;

namespace Xarial.XCad.Features.CustomFeature.Attributes
{
    /// <summary>
    /// Attributes specifies the current version of the macro feature parameters data model.
    /// This allows to implement backward compatibility for the macro feature parameters
    /// for future versions of macro feature
    /// </summary>
    public class ParametersVersionAttribute : Attribute
    {
        public Version Version { get; private set; }
        public IParametersVersionConverter VersionConverter { get; private set; }

        /// <summary>
        /// Specifies the current version of the parameters data model
        /// </summary>
        /// <param name="major">Major version</param>
        /// <param name="minor">Minor version</param>
        /// <param name="versionConverterType">Type of the parameters converter between versions which implements
        /// <see cref="IParametersVersionConverter"/> interface</param>
        public ParametersVersionAttribute(int major, int minor, Type versionConverterType)
            : this(versionConverterType)
        {
            Version = new Version(major, minor);
        }

        /// <inheritdoc cref="ParametersVersionAttribute(int, int, Type)"/>
        /// <param name="version">Full version</param>
        public ParametersVersionAttribute(string version, Type versionConverterType)
            : this(versionConverterType)
        {
            Version = new Version(version);
        }

        private ParametersVersionAttribute(Type versionConverterType)
        {
            if (!typeof(IParametersVersionConverter).IsAssignableFrom(versionConverterType))
            {
                throw new InvalidCastException(
                    $"{versionConverterType.FullName} must implement {typeof(IParameterConverter).FullName}");
            }

            VersionConverter = (IParametersVersionConverter)Activator.CreateInstance(versionConverterType);
        }
    }
}