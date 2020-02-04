//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.Features.CustomFeature;
using Xarial.XCad.Features.CustomFeature.Delegates;
using Xarial.XCad.UI.Commands;
using Xarial.XCad.UI.PropertyPage;

namespace Xarial.XCad.Extensions
{
    public interface IXExtension
    {
        bool OnConnect();

        bool OnDisconnect();

        IXApplication Application { get; }
        IXCommandManager CommandManager { get; }

        IXPropertyPage<TData> CreatePage<TData>();

        IXCustomFeatureEditor<TData, TPage> CreateCustomFeatureEditor<TData, TPage>(Type defType,
            DataConverterDelegate<TPage, TData> pageToDataConv, DataConverterDelegate<TData, TPage> dataToPageConv,
            CreateGeometryDelegate<TData> geomCreator)
            where TData : class, new()
            where TPage : class, new();
    }
}