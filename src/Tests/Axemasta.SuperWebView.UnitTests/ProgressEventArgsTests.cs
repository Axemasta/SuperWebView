using System;
using NUnit.Framework;

namespace Axemasta.SuperWebView.UnitTests
{
    [TestFixture]
    public class ProgressEventArgsTests
    {
        [Test]
        // iOS Tests
        [TestCase(0.1, 1, 0.1, 10)]        
        [TestCase(1, 1, 1, 100)]
        [TestCase(0, 1, 0, 0)]
        [TestCase(0.1454833984375, 1, 0.1455, 14.55)]
        [TestCase(0.752, 1, 0.752, 75.2)]
        [TestCase(0.897883218134872, 1, 0.8979, 89.79)]

        // Android Tests
        [TestCase(10, 100, 0.1, 10)]
        [TestCase(0, 100, 0, 0)]
        [TestCase(23, 100, 0.23, 23)]
        [TestCase(26, 100, 0.26, 26)]
        [TestCase(35, 100, 0.35, 35)]
        [TestCase(48, 100, 0.48, 48)]
        [TestCase(50, 100, 0.5, 50)]
        [TestCase(70, 100, 0.7, 70)]
        [TestCase(80, 100, 0.8, 80)]
        [TestCase(100, 100, 1, 100)]
        public void Constructor_Should_SetFieldsCorrectly(
            double progressValue, int progressLimit,
            double expectedNormalized, double expectedPercentage)
        {
            var progressArgs = new ProgressEventArgs(progressValue, progressLimit);

            Assert.IsTrue(progressArgs.NormalisedProgress <= 1, "Normalized progress was greater than 1");
            Assert.IsTrue(progressArgs.PercentageComplete <= 100, "Percentage progress was greater than 100");

            Assert.AreEqual(expectedNormalized, progressArgs.NormalisedProgress);
            Assert.AreEqual(expectedPercentage, progressArgs.PercentageComplete);
        }
    }
}
