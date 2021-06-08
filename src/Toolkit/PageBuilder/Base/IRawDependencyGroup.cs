//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Services;

namespace Xarial.XCad.Utils.PageBuilder.Base
{
    public interface IRawDependencyGroup
    {
        IReadOnlyDictionary<object, IBinding> TaggedBindings { get; }
        IReadOnlyDictionary<IBinding, Tuple<object[], IDependencyHandler>> DependenciesTags { get; }
        IReadOnlyDictionary<IControl, Tuple<IMetadata[], IMetadataDependencyHandler>> MetadataDependencies { get; }

        void RegisterBindingTag(IBinding binding, object tag);

        void RegisterDependency(IBinding binding, object[] dependentOnTags, IDependencyHandler dependencyHandler);

        void RegisterMetadataDependency(IControl ctrl, IMetadata[] metadata, IMetadataDependencyHandler dependencyHandler);
    }
}