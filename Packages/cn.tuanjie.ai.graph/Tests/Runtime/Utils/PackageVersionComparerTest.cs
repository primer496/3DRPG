using NUnit.Framework;

namespace UnityEngine.AIGraph.Tests
{
    [TestFixture]
    public class PackageVersionComparerTest
    {
        [TestFixture]
        public class CompareVersionsTests
        {
            [Test]
            public void CompareVersions_StandardVersions_ReturnsCorrectComparison()
            {
                // Arrange & Act & Assert
                Assert.AreEqual(-1, PackageVersionComparer.CompareVersions("1.2.3", "1.2.4"));
                Assert.AreEqual(0, PackageVersionComparer.CompareVersions("1.2.3", "1.2.3"));
                Assert.AreEqual(1, PackageVersionComparer.CompareVersions("1.2.4", "1.2.3"));
            }

            [Test]
            public void CompareVersions_DifferentLengthVersions_ReturnsCorrectComparison()
            {
                Assert.AreEqual(-1, PackageVersionComparer.CompareVersions("1.2", "1.2.1"));
                Assert.AreEqual(0, PackageVersionComparer.CompareVersions("1.2", "1.2.0"));
                Assert.AreEqual(1, PackageVersionComparer.CompareVersions("1.2.1", "1.2"));
            }

            [Test]
            public void CompareVersions_WithVPrefix_ReturnsCorrectComparison()
            {
                Assert.AreEqual(-1, PackageVersionComparer.CompareVersions("v1.2.3", "v1.2.4"));
                Assert.AreEqual(0, PackageVersionComparer.CompareVersions("v1.2.3", "1.2.3"));
                Assert.AreEqual(1, PackageVersionComparer.CompareVersions("v1.2.4", "v1.2.3"));
            }

            [Test]
            public void CompareVersions_WithPreRelease_ReturnsCorrectComparison()
            {
                Assert.AreEqual(-1, PackageVersionComparer.CompareVersions("1.2.3-alpha", "1.2.3"));
                Assert.AreEqual(-1, PackageVersionComparer.CompareVersions("1.2.3-alpha", "1.2.3-beta"));
                Assert.AreEqual(1, PackageVersionComparer.CompareVersions("1.2.3", "1.2.3-alpha"));
            }
            
            [Test]
            public void CompareVersions_WithExp_ReturnsCorrectComparison()
            {
                Assert.AreEqual(-1, PackageVersionComparer.CompareVersions("1.2.3-exp", "1.2.3"));
                Assert.AreEqual(0, PackageVersionComparer.CompareVersions("1.2.3-exp", "1.2.3-exp"));
                Assert.AreEqual(1, PackageVersionComparer.CompareVersions("1.2.4-exp", "1.2.3-exp"));
            }

            [Test]
            public void CompareVersions_WithBuildMetadata_ReturnsCorrectComparison()
            {
                Assert.AreEqual(0, PackageVersionComparer.CompareVersions("1.2.3+build.123", "1.2.3"));
                Assert.AreEqual(0, PackageVersionComparer.CompareVersions("1.2.3+build.123", "1.2.3+build.456"));
            }

            [Test]
            public void CompareVersions_NullOrEmptyVersions_ReturnsCorrectComparison()
            {
                Assert.AreEqual(0, PackageVersionComparer.CompareVersions("", ""));
                Assert.AreEqual(0, PackageVersionComparer.CompareVersions(null, null));
                Assert.AreEqual(-1, PackageVersionComparer.CompareVersions("", "1.0.0"));
                Assert.AreEqual(1, PackageVersionComparer.CompareVersions("1.0.0", ""));
                Assert.AreEqual(-1, PackageVersionComparer.CompareVersions(null, "1.0.0"));
                Assert.AreEqual(1, PackageVersionComparer.CompareVersions("1.0.0", null));
            }

            [Test]
            public void CompareVersions_MajorVersionDifferences_ReturnsCorrectComparison()
            {
                Assert.AreEqual(-1, PackageVersionComparer.CompareVersions("1.9.9", "2.0.0"));
                Assert.AreEqual(1, PackageVersionComparer.CompareVersions("2.0.0", "1.9.9"));
            }

            [Test]
            public void CompareVersions_MinorVersionDifferences_ReturnsCorrectComparison()
            {
                Assert.AreEqual(-1, PackageVersionComparer.CompareVersions("1.2.9", "1.3.0"));
                Assert.AreEqual(1, PackageVersionComparer.CompareVersions("1.3.0", "1.2.9"));
            }

            [Test]
            public void CompareVersions_PatchVersionDifferences_ReturnsCorrectComparison()
            {
                Assert.AreEqual(-1, PackageVersionComparer.CompareVersions("1.2.3", "1.2.4"));
                Assert.AreEqual(1, PackageVersionComparer.CompareVersions("1.2.4", "1.2.3"));
            }
        }

        [TestFixture]
        public class ConvenienceMethodsTests
        {
            [Test]
            public void IsNewer_ReturnsTrueWhenNewer()
            {
                Assert.IsTrue(PackageVersionComparer.IsNewer("1.2.4", "1.2.3"));
                Assert.IsFalse(PackageVersionComparer.IsNewer("1.2.3", "1.2.4"));
                Assert.IsFalse(PackageVersionComparer.IsNewer("1.2.3", "1.2.3"));
            }

            [Test]
            public void IsOlder_ReturnsTrueWhenOlder()
            {
                Assert.IsTrue(PackageVersionComparer.IsOlder("1.2.3", "1.2.4"));
                Assert.IsFalse(PackageVersionComparer.IsOlder("1.2.4", "1.2.3"));
                Assert.IsFalse(PackageVersionComparer.IsOlder("1.2.3", "1.2.3"));
            }

            [Test]
            public void IsSame_ReturnsTrueWhenSame()
            {
                Assert.IsTrue(PackageVersionComparer.IsSame("1.2.3", "1.2.3"));
                Assert.IsTrue(PackageVersionComparer.IsSame("v1.2.3", "1.2.3"));
                Assert.IsFalse(PackageVersionComparer.IsSame("1.2.3", "1.2.4"));
            }

            [Test]
            public void IsNewerOrSame_ReturnsCorrectResults()
            {
                Assert.IsTrue(PackageVersionComparer.IsNewerOrSame("1.2.4", "1.2.3"));
                Assert.IsTrue(PackageVersionComparer.IsNewerOrSame("1.2.3", "1.2.3"));
                Assert.IsFalse(PackageVersionComparer.IsNewerOrSame("1.2.3", "1.2.4"));
            }
        }

        [TestFixture]
        public class EdgeCaseTests
        {
            [Test]
            public void CompareVersions_InvalidVersions_FallsBackToManualComparison()
            {
                // 这些版本无法被Version类解析，应该回退到手动比较
                Assert.AreEqual(-1, PackageVersionComparer.CompareVersions("1.2.3.4.5", "1.2.3.4.6"));
                Assert.AreEqual(0, PackageVersionComparer.CompareVersions("1.2.3.4.5", "1.2.3.4.5"));
                Assert.AreEqual(1, PackageVersionComparer.CompareVersions("1.2.3.4.6", "1.2.3.4.5"));
            }

            [Test]
            public void CompareVersions_VeryLongVersions_HandlesCorrectly()
            {
                Assert.AreEqual(-1, PackageVersionComparer.CompareVersions("1.2.3.4.5.6", "1.2.3.4.5.7"));
                Assert.AreEqual(0, PackageVersionComparer.CompareVersions("1.2.3.4.5.6", "1.2.3.4.5.6"));
                Assert.AreEqual(1, PackageVersionComparer.CompareVersions("1.2.3.4.5.7", "1.2.3.4.5.6"));
            }

            [Test]
            public void CompareVersions_VersionWithNonNumericParts_HandlesCorrectly()
            {
                Assert.AreEqual(-1, PackageVersionComparer.CompareVersions("1.2.3-alpha", "1.2.3-beta"));
                Assert.AreEqual(-1, PackageVersionComparer.CompareVersions("1.2.3-rc1", "1.2.3-rc2"));
            }
        }

        [TestFixture]
        public class IntegrationTests
        {
            [Test]
            public void PackageUpdateChecker_DetectsNewVersions()
            {
                // 模拟版本更新检测
                string currentVersion = "1.2.3";
                string newVersion = "1.3.0";

                Assert.IsTrue(PackageVersionComparer.IsNewer(newVersion, currentVersion));
                Assert.IsFalse(PackageVersionComparer.IsNewer(currentVersion, newVersion));
            }

            [Test]
            public void VersionComparison_InRealWorldScenarios()
            {
                // 测试实际应用场景
                string[] versions =
                {
                    "0.9.0",
                    "1.0.0-alpha",
                    "1.0.0-beta",
                    "1.0.0-rc1",
                    "1.0.0",
                    "1.0.1",
                    "1.1.0",
                    "2.0.0"
                };

                // 验证版本顺序正确
                for (int i = 0; i < versions.Length - 1; i++)
                {
                    Assert.IsTrue(
                        PackageVersionComparer.CompareVersions(versions[i], versions[i + 1]) < 0,
                        $"版本 {versions[i]} 应该小于 {versions[i + 1]}"
                    );
                }
            }
        }
    }
}