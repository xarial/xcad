//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Exceptions;

namespace Xarial.XCad.Base
{
    public static class IXRepositoryExtension
    {
        public static void Add<TEnt>(this IXRepository<TEnt> repo, params TEnt[] ents)
        {
            repo.AddRange(ents);
        }

        public static TEnt Get<TEnt>(this IXRepository<TEnt> repo, string name) 
        {
            if (repo.TryGet(name, out TEnt ent))
            {
                return ent;
            }
            else
            {
                throw new EntityNotFoundException(name);
            }
        }
    }
}