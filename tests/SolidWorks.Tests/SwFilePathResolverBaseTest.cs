using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Documents.Exceptions;
using Xarial.XCad.SolidWorks.Exceptions;
using Xarial.XCad.SolidWorks.Services;

namespace SolidWorks.Tests
{
    public class MockSwFilePathResolverBase : SwFilePathResolverBase
    {
        private readonly string[] m_SearchFolders;
        private readonly Predicate<string> m_RefExistsPred;
        private readonly Func<string> m_MatchingLoadedDocPath;

        public MockSwFilePathResolverBase(string[] searchFolders, Predicate<string> refExistsPred, Func<string> matchingLoadedDocPath) 
        {
            m_SearchFolders = searchFolders;
            m_RefExistsPred = refExistsPred;
            m_MatchingLoadedDocPath = matchingLoadedDocPath;
        }

        protected override string[] GetSearchFolders() => m_SearchFolders;

        protected override bool IsReferenceExists(string path) => m_RefExistsPred.Invoke(path);

        protected override bool TryGetLoadedDocumentPath(string path, out string loadedPath)
        {
            loadedPath = m_MatchingLoadedDocPath.Invoke();
            return !string.IsNullOrEmpty(loadedPath);
        }
    }

    public class SwFilePathResolverBaseTest
    {
        [Test]
        public void ResolvePath_AllRoutes() 
        {
            var paths = new List<string>();

            var res = new MockSwFilePathResolverBase(new string[]
            {
                @"D:\aa\bb\",
                @"E:\cc\dd\"
            }, p =>
            {
                paths.Add(p);
                return false;
            }, () => "");

            var expPaths = new string[]
            {
                @"D:\aa\bb\p2.sldprt",
                @"D:\aa\bb\xx\p2.sldprt",
                @"D:\aa\bb\yy\xx\p2.sldprt",
                @"D:\aa\bb\zz\yy\xx\p2.sldprt",
                @"D:\aa\xx\p2.sldprt",
                @"D:\aa\yy\xx\p2.sldprt",
                @"D:\aa\zz\yy\xx\p2.sldprt",
                @"D:\xx\p2.sldprt",
                @"D:\yy\xx\p2.sldprt",
                @"D:\zz\yy\xx\p2.sldprt",
                @"E:\cc\dd\p2.sldprt",
                @"E:\cc\dd\xx\p2.sldprt",
                @"E:\cc\dd\yy\xx\p2.sldprt",
                @"E:\cc\dd\zz\yy\xx\p2.sldprt",
                @"E:\cc\xx\p2.sldprt",
                @"E:\cc\yy\xx\p2.sldprt",
                @"E:\cc\zz\yy\xx\p2.sldprt",
                @"E:\xx\p2.sldprt",
                @"E:\yy\xx\p2.sldprt",
                @"E:\zz\yy\xx\p2.sldprt",
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

            Assert.Throws<FilePathResolveFailedException>(() => res.ResolvePath(@"D:\ss\tt\a1.sldasm", @"C:\zz\yy\xx\p2.sldprt"));
            CollectionAssert.AreEqual(expPaths, paths);
        }

        [Test]
        public void ResolvePath_Test() 
        {
            var res = new MockSwFilePathResolverBase(new string[]
            {
                @"D:\aa\bb\",
                @"E:\cc\dd\"
            }, p => p == @"E:\yy\xx\p2.sldprt", () => "");

            var p1 = res.ResolvePath(@"D:\ss\tt\a1.sldasm", @"C:\zz\yy\xx\p2.sldprt");

            Assert.AreEqual(@"E:\yy\xx\p2.sldprt", p1);
        }

        [Test]
        public void ResolvePath_Active()
        {
            var res = new MockSwFilePathResolverBase(new string[]
            {
                @"D:\aa\bb\",
                @"E:\cc\dd\"
            }, p => false, () => "X:\\p2.sldprt");

            var p1 = res.ResolvePath(@"D:\ss\tt\a1.sldasm", @"C:\zz\yy\xx\p2.sldprt");

            Assert.AreEqual("X:\\p2.sldprt", p1);
        }

        [Test]
        public void ResolvePath_Initial()
        {
            var res = new MockSwFilePathResolverBase(new string[0], p => p == @"C:\zz\yy\xx\p2.sldprt", () => "");

            var p1 = res.ResolvePath(@"D:\ss\tt\a1.sldasm", @"C:\zz\yy\xx\p2.sldprt");

            Assert.AreEqual(@"C:\zz\yy\xx\p2.sldprt", p1);
        }
    }
}
