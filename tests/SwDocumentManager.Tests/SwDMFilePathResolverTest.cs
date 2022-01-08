using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Documents.Exceptions;
using Xarial.XCad.SwDocumentManager.Services;

namespace SolidWorks.Tests
{
    public class MockSwDmFilePathResolver : SwDmFilePathResolver
    {
        private readonly Predicate<string> m_RefExistsPred;

        public MockSwDmFilePathResolver(Predicate<string> refExistsPred) 
        {
            m_RefExistsPred = refExistsPred;
        }

        protected override bool IsReferenceExists(string path) => m_RefExistsPred.Invoke(path);
    }

    public class SwDmFilePathResolverTest
    {
        [Test]
        public void ResolvePath_AllRoutes() 
        {
            var paths = new List<string>();

            var res = new MockSwDmFilePathResolver(p =>
            {
                paths.Add(p);
                return false;
            });

            var expPaths = new string[]
            {
                @"D:\ss\tt\p2.sldprt",
                @"D:\ss\tt\xx\p2.sldprt",
                @"D:\ss\tt\yy\xx\p2.sldprt",
                @"D:\ss\tt\zz\yy\xx\p2.sldprt",
                @"D:\ss\xx\p2.sldprt",
                @"D:\ss\yy\xx\p2.sldprt",
                @"D:\ss\zz\yy\xx\p2.sldprt",
                @"D:\xx\p2.sldprt",
                @"D:\yy\xx\p2.sldprt",
                @"D:\zz\yy\xx\p2.sldprt",
                @"C:\zz\yy\xx\p2.sldprt"
            };

            Assert.Throws<FilePathResolveFailedException>(() => res.ResolvePath(@"D:\ss\tt", @"C:\zz\yy\xx\p2.sldprt"));
            CollectionAssert.AreEqual(expPaths, paths);
        }

        [Test]
        public void ResolvePath_Test() 
        {
            var res = new MockSwDmFilePathResolver(p => p == @"D:\ss\tt\yy\xx\p2.sldprt");

            var p1 = res.ResolvePath(@"D:\ss\tt\a1.sldasm", @"C:\zz\yy\xx\p2.sldprt");

            Assert.AreEqual(@"D:\ss\tt\yy\xx\p2.sldprt", p1);
        }

        [Test]
        public void ResolvePath_Initial()
        {
            var res = new MockSwDmFilePathResolver(p => p == @"C:\zz\yy\xx\p2.sldprt");

            var p1 = res.ResolvePath(@"D:\ss\tt\a1.sldasm", @"C:\zz\yy\xx\p2.sldprt");

            Assert.AreEqual(@"C:\zz\yy\xx\p2.sldprt", p1);
        }
    }
}
