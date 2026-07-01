using System.IO;
using NUnit.Framework;

namespace UnityEngine.AIGraph.Tests
{
    [TestFixture]
    public class ExportUtilsTests
    {
        [TestFixture]
        public class GetAssetPathTests
        {
            [Test]
            public void GetAssetPath_WhenNullOrEmpty_ReturnsOriginal()
            {
                // Arrange
                string nullPath = null;
                string emptyPath = "";
                string whitespacePath = "   ";

                // Act & Assert
                Assert.IsNull(ExportUtils.GetAssetPath(nullPath));
                Assert.AreEqual("", ExportUtils.GetAssetPath(emptyPath));
                Assert.AreEqual(string.Empty, ExportUtils.GetAssetPath(whitespacePath));
            }

            [Test]
            public void GetAssetPath_WhenAlreadyStartsWithAssets_ReturnsNormalized()
            {
                // Arrange
                string path1 = "Assets/Textures/image.png";
                string path2 = "Assets\\Textures\\image.png"; // 反斜杠

                // Act
                string result1 = ExportUtils.GetAssetPath(path1);
                string result2 = ExportUtils.GetAssetPath(path2);

                // Assert
                Assert.AreEqual("Assets/Textures/image.png", result1);
                Assert.AreEqual("Assets/Textures/image.png", result2); // 应该被标准化为正斜杠
            }

            [Test]
            public void GetAssetPath_WhenNotStartingWithAssets_PrefixesAssets()
            {
                // Arrange
                string path1 = "Textures/image.png";
                string path2 = "Textures\\image.png";

                // Act
                string result1 = ExportUtils.GetAssetPath(path1);
                string result2 = ExportUtils.GetAssetPath(path2);

                // Assert
                Assert.AreEqual("Assets/Textures/image.png", result1);
                Assert.AreEqual("Assets/Textures/image.png", result2);
            }

            [Test]
            public void GetAssetPath_WhenStartsWithDataPath_ConvertsToAssetsRelative()
            {
                string fullPath = $"{Application.dataPath}/Textures/image.png";
                string fullPathWithBackslashes = $"{Application.dataPath}\\Textures\\image.png";

                // Act
                string result1 = ExportUtils.GetAssetPath(fullPath);
                string result2 = ExportUtils.GetAssetPath(fullPathWithBackslashes);

                // Assert
                Assert.AreEqual("Assets/Textures/image.png", result1);
                Assert.AreEqual("Assets/Textures/image.png", result2);
            }

            [Test]
            public void GetAssetPath_WhenStartsWithDataPathButDifferentCase_StillConverts()
            {
                string fullPath = $"{Application.dataPath.ToUpper()}/Textures/image.png";

                // Act
                string result = ExportUtils.GetAssetPath(fullPath);

                // Assert
                Assert.AreEqual("Assets/Textures/image.png", result);
            }

            [Test]
            public void GetAssetPath_WhenPathIsDataPathExactly_ReturnsAssets()
            {
                // Act
                string result = ExportUtils.GetAssetPath(Application.dataPath);

                // Assert
                Assert.AreEqual("Assets", result);
            }

            [Test]
            public void GetAssetPath_WhenPathOutsideDataPathButStartsWithAssets_ReturnsNormalized()
            {
                // Arrange
                string path = "Assets/SomeFolder/file.asset";

                // Act
                string result = ExportUtils.GetAssetPath(path);

                // Assert
                Assert.AreEqual("Assets/SomeFolder/file.asset", result);
            }

            [Test]
            public void GetAssetPath_WhenPathOutsideDataPathAndNotStartingWithAssets_PrefixesAssets()
            {
                // Arrange
                string path = "SomeFolder/file.asset";

                // Act
                string result = ExportUtils.GetAssetPath(path);

                // Assert
                Assert.AreEqual("Assets/SomeFolder/file.asset", result);
            }

            [Test]
            public void GetAssetPath_WithMixedSlashes_NormalizesToForwardSlashes()
            {
                // Arrange
                string mixedPath = "Assets\\Textures/image.png";
                string allBackslashes = "Assets\\Textures\\image.png";

                // Act
                string result1 = ExportUtils.GetAssetPath(mixedPath);
                string result2 = ExportUtils.GetAssetPath(allBackslashes);

                // Assert
                Assert.AreEqual("Assets/Textures/image.png", result1);
                Assert.AreEqual("Assets/Textures/image.png", result2);
            }

            [Test]
            public void GetAssetPath_WithComplexNestedPaths_HandlesCorrectly()
            {
                string complexPath = $"{Application.dataPath}/SubFolder1/SubFolder2/file.asset";

                // Act
                string result = ExportUtils.GetAssetPath(complexPath);

                // Assert
                Assert.AreEqual("Assets/SubFolder1/SubFolder2/file.asset", result);
            }
        }

        [TestFixture]
        public class GetPathTests
        {
            private string testDirectory;

            [SetUp]
            public void SetUp()
            {
                testDirectory = Path.Combine(Application.dataPath, "GetPathTests").Replace("\\", "/");
                Directory.CreateDirectory(testDirectory);
            }

            [TearDown]
            public void TearDown()
            {
                if (Directory.Exists(testDirectory))
                {
                    Directory.Delete(testDirectory, true);
                }
            }

            [Test]
            public void GetPath_WithValidParameters_ReturnsCorrectPath()
            {
                // Act
                string result = TestHelper.InvokePrivateStaticMethod<string>(
                    typeof(ExportUtils), "GetPath", testDirectory, "test", "txt", 259);

                // Assert
                Assert.AreEqual(Path.Combine(testDirectory, "test.txt"), result);
            }

            [Test]
            public void GetPath_WithEmptyParameters_ReturnsEmpty()
            {
                // Act & Assert
                Assert.AreEqual(string.Empty, TestHelper.InvokePrivateStaticMethod<string>(
                    typeof(ExportUtils), "GetPath", "", "test", "txt", 259));
                Assert.AreEqual(string.Empty, TestHelper.InvokePrivateStaticMethod<string>(
                    typeof(ExportUtils), "GetPath", testDirectory, "", "txt", 259));
                Assert.AreEqual(string.Empty, TestHelper.InvokePrivateStaticMethod<string>(
                    typeof(ExportUtils), "GetPath", testDirectory, "test", "", 259));
            }

            [Test]
            public void GetPath_WithLongFileName_TruncatesToMaxLength()
            {
                // Arrange
                string longFileName = new string('a', 300);

                // Act
                string result = TestHelper.InvokePrivateStaticMethod<string>(
                    typeof(ExportUtils), "GetPath", testDirectory, longFileName, "txt", 100);

                // Assert
                Assert.LessOrEqual(result.Length, 100);
                Assert.IsTrue(result.EndsWith(".txt"));
            }
        }

        [TestFixture]
        public class GetAssetExtensionTests
        {
            [Test]
            public void GetAssetExtension_Texture2D_ReturnsPng()
            {
                // Act
                string result = TestHelper.InvokePrivateStaticMethod<string>(
                    typeof(ExportUtils), "GetAssetExtension", typeof(Texture2D));

                // Assert
                Assert.AreEqual(".png", result);
            }

            [Test]
            public void GetAssetExtension_Mesh_ReturnsMesh()
            {
                // Act
                string result = TestHelper.InvokePrivateStaticMethod<string>(
                    typeof(ExportUtils), "GetAssetExtension", typeof(Mesh));

                // Assert
                Assert.AreEqual(".mesh", result);
            }

            [Test]
            public void GetAssetExtension_Material_ReturnsMat()
            {
                // Act
                string result = TestHelper.InvokePrivateStaticMethod<string>(
                    typeof(ExportUtils), "GetAssetExtension", typeof(Material));

                // Assert
                Assert.AreEqual(".mat", result);
            }

            [Test]
            public void GetAssetExtension_AnimationClip_ReturnsAnim()
            {
                // Act
                string result = TestHelper.InvokePrivateStaticMethod<string>(
                    typeof(ExportUtils), "GetAssetExtension", typeof(AnimationClip));

                // Assert
                Assert.AreEqual(".anim", result);
            }

            [Test]
            public void GetAssetExtension_AudioClip_ReturnsWav()
            {
                // Act
                string result = TestHelper.InvokePrivateStaticMethod<string>(
                    typeof(ExportUtils), "GetAssetExtension", typeof(AudioClip));

                // Assert
                Assert.AreEqual(".wav", result);
            }

            [Test]
            public void GetAssetExtension_GameObject_ReturnsPrefab()
            {
                // Act
                string result = TestHelper.InvokePrivateStaticMethod<string>(
                    typeof(ExportUtils), "GetAssetExtension", typeof(GameObject));

                // Assert
                Assert.AreEqual(".prefab", result);
            }

            [Test]
            public void GetAssetExtension_UnknownType_ReturnsAsset()
            {
                // Act
                string result = TestHelper.InvokePrivateStaticMethod<string>(
                    typeof(ExportUtils), "GetAssetExtension", typeof(ScriptableObject));

                // Assert
                Assert.AreEqual(".asset", result);
            }
        }

        [TestFixture]
        public class GetUniquePathTests
        {
            private string testDirectory;

            [SetUp]
            public void SetUp()
            {
                testDirectory = Path.Combine(Path.GetTempPath(), "ExportUtilsTests");
                Directory.CreateDirectory(testDirectory);
            }

            [TearDown]
            public void TearDown()
            {
                if (Directory.Exists(testDirectory))
                {
                    Directory.Delete(testDirectory, true);
                }
            }

            [Test]
            public void GetUniquePath_WhenFileDoesNotExist_ReturnsOriginalPath()
            {
                // Arrange
                string expectedPath = Path.Combine(testDirectory, "test.txt");

                // Act
                string result = ExportUtils.GetUniquePath(testDirectory, "test", "txt");

                // Assert
                Assert.AreEqual(GetNormalizedPath(expectedPath), GetNormalizedPath(result));
            }

            string GetNormalizedPath(string path)
            {
                return path.Replace("\\", "/");
            }

            [Test]
            public void GetUniquePath_WhenFileExists_ReturnsUniquePath()
            {
                // Arrange
                string existingFile = Path.Combine(testDirectory, "test.txt");
                File.WriteAllText(existingFile, "test content");

                // Act
                string result = ExportUtils.GetUniquePath(testDirectory, "test", "txt");

                // Assert
                Assert.AreEqual(Path.Combine(testDirectory, "test 1.txt"), result);
            }

            [Test]
            public void GetUniquePath_WhenMultipleFilesExist_ReturnsCorrectIncrement()
            {
                // Arrange
                File.WriteAllText(Path.Combine(testDirectory, "test.txt"), "content1");
                File.WriteAllText(Path.Combine(testDirectory, "test 1.txt"), "content2");
                File.WriteAllText(Path.Combine(testDirectory, "test 2.txt"), "content3");

                // Act
                string result = ExportUtils.GetUniquePath(testDirectory, "test", "txt");

                // Assert
                Assert.AreEqual(Path.Combine(testDirectory, "test 3.txt"), result);
            }

            [Test]
            public void GetUniquePath_WithEmptyParameters_ReturnsEmpty()
            {
                // Act & Assert
                Assert.AreEqual(string.Empty, ExportUtils.GetUniquePath("", "test", "txt"));
                Assert.AreEqual(string.Empty, ExportUtils.GetUniquePath(testDirectory, "", "txt"));
                Assert.AreEqual(string.Empty, ExportUtils.GetUniquePath(testDirectory, "test", ""));
            }
        }

        [TestFixture]
        public class PathValidationTests
        {
            [Test]
            public void IsInAssets_WhenPathInAssets_ReturnsTrue()
            {
                // Arrange
                string path = $"{Application.dataPath}/Textures/image.png";

                // Act
                bool result = ExportUtils.IsInAssets(path, out string relativePath);

                // Assert
                Assert.IsTrue(result);
                Assert.AreEqual("Assets/Textures/image.png", relativePath);
            }

            [Test]
            public void IsInAssets_WhenPathOutsideAssets_ReturnsFalse()
            {
                // Arrange
                string path = "C:/OtherFolder/file.txt";

                // Act
                bool result = ExportUtils.IsInAssets(path, out string relativePath);

                // Assert
                Assert.IsFalse(result);
                Assert.IsTrue(string.IsNullOrEmpty(relativePath));
            }

            [Test]
            public void IsInAssets_WithRelativeAssetsPath_ReturnsTrue()
            {
                // Arrange
                string path = "Assets/Textures/image.png";

                // Act
                bool result = ExportUtils.IsInAssets(path, out string relativePath);

                // Assert
                Assert.IsTrue(result);
                Assert.AreEqual("Assets/Textures/image.png", relativePath);
            }

            [Test]
            public void GetPathRelativeToRoot_WithAssetsPath_ReturnsRelativePath()
            {
                // Act
                string result = TestHelper.InvokePrivateStaticMethod<string>(
                    typeof(ExportUtils), "GetPathRelativeToRoot", "Assets/Textures/image.png");

                // Assert
                Assert.AreEqual("Assets/Textures/image.png", result);
            }

            [Test]
            public void GetPathRelativeToRoot_WithAbsolutePath_ReturnsRelativePath()
            {
                // Act
                string result = TestHelper.InvokePrivateStaticMethod<string>(
                    typeof(ExportUtils), "GetPathRelativeToRoot", 
                    $"{Application.dataPath}/Textures/image.png");

                // Assert
                Assert.AreEqual("Assets/Textures/image.png", result);
            }
        }
    }
}