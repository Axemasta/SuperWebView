using System;
using NUnit.Framework;

namespace Axemasta.SuperWebView.UnitTests
{
    [TestFixture]
    public class EmbeddedResourceHelperTests
    {
        [Test]
        public void Load_NoResourceName_Should_ThrowArgumentException()
        {
            var expectedException = new ArgumentNullException("resourceName");

            var ex = Assert.Throws<ArgumentNullException>(() => EmbeddedResourceHelper.Load(null, null));

            Assert.AreEqual(expectedException.Message, ex.Message);
        }

        [Test]
        public void Load_NoAssemblyName_Should_ThrowArgumentException()
        {
            var expectedException = new ArgumentNullException("assemblyName");

            var ex = Assert.Throws<ArgumentNullException>(() => EmbeddedResourceHelper.Load("myResource", null));

            Assert.AreEqual(expectedException.Message, ex.Message);
        }
    }
}
