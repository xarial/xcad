//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

namespace Xarial.XCad.Features.CustomFeature.Delegates
{
    public delegate TOut DataConverterDelegate<TIn, TOut>(TIn data)
        where TIn : class, new()
        where TOut : class, new();
}