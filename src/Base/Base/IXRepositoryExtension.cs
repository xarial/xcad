//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

namespace Xarial.XCad.Base
{
    public static class IXRepositoryExtension
    {
        public static void Add<TEnt>(this IXRepository<TEnt> repo, params TEnt[] ents)
        {
            repo.AddRange(ents);
        }
    }
}