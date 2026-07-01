using System.IO;
using NUnit.Framework;

namespace UnityEngine.AIGraph.Tests
{
    [TestFixture]
    public class PathUtilsTest
    {
        [Test]
        public void IsSamePath_WithVariousPathComparisons_ReturnsExpectedResult()
        {
            var testCases = new[]
            {
                // 相同路径
                (Path1: @"C:\Users\YourUsername\Documents\example.txt", 
                 Path2: @"C:\Users\YourUsername\Documents\example.txt", 
                 Expected: true,
                 Description: "Identical paths"),

                // 不同路径
                (Path1: @"C:\Users\YourUsername\Documents\example.txt", 
                 Path2: @"C:\Users\YourUsername\Documents\example2.txt", 
                 Expected: false,
                 Description: "Different filenames"),

                // 相对路径指向相同位置
                (Path1: Path.Combine(Directory.GetCurrentDirectory(), "example.txt"), 
                 Path2: Path.Combine(Directory.GetCurrentDirectory(), ".", "example.txt"), 
                 Expected: true,
                 Description: "Relative paths to same location"),

                // 大小写不同
                (Path1: @"C:\Users\YourUsername\Documents\example.txt", 
                 Path2: @"C:\USERS\YOURUSERNAME\DOCUMENTS\EXAMPLE.TXT", 
                 Expected: true,
                 Description: "Different case"),

                // 斜杠不同
                (Path1: @"C:\Users\YourUsername\Documents\example.txt", 
                 Path2: @"C:/Users/YourUsername/Documents/example.txt", 
                 Expected: true,
                 Description: "Different slashes")
            };

            foreach (var (path1, path2, expected, description) in testCases)
            {
                var result = PathUtils.IsSamePath(path1, path2);
                Assert.That(result, Is.EqualTo(expected), 
                    $"{description}: Expected {expected} for paths '{path1}' and '{path2}'");
            }
        }

        [Test]
        public void GetUrlExtension_WithVariousUrls_ReturnsCorrectExtension()
        {
            var testCases = new[]
            {
                // 包含点号的扩展名
                (Url: "https://example.com/image.jpg", IncludeDot: true, Expected: ".jpg"),
                (Url: "https://example.com/document.pdf?query=string", IncludeDot: true, Expected: ".pdf"),
                (Url: "https://example.com/page.html#section", IncludeDot: true, Expected: ".html"),
                (Url: "https://example.com/file.with.multiple.dots.png", IncludeDot: true, Expected: ".png"),
                (Url: "https://example.com/path/to/file.js", IncludeDot: true, Expected: ".js"),
                (Url: "https://example.com/archive.tar.gz", IncludeDot: true, Expected: ".gz"),
                (Url: "relative/path/image.png", IncludeDot: true, Expected: ".png"),
                (Url: "https://example.com/document.PDF", IncludeDot: true, Expected: ".pdf"),
                (Url: "https://example.com/files/my%20document.txt", IncludeDot: true, Expected: ".txt"),
                (Url: "https://example.com/file.MP4", IncludeDot: true, Expected: ".mp4"),
                (Url: "https://example.com/image.JPEG", IncludeDot: true, Expected: ".jpeg"),
                (Url: "https://example.com/data.JSON", IncludeDot: true, Expected: ".json"),
                (Url: "https://example.com/config.XML", IncludeDot: true, Expected: ".xml"),

                // 不包含点号的扩展名
                (Url: "https://example.com/image.jpg", IncludeDot: false, Expected: "jpg"),
                (Url: "https://example.com/document.pdf?query=string", IncludeDot: false, Expected: "pdf"),
                (Url: "https://example.com/page.html#section", IncludeDot: false, Expected: "html"),
                (Url: "https://example.com/file.with.multiple.dots.png", IncludeDot: false, Expected: "png")
            };

            foreach (var (url, includeDot, expected) in testCases)
            {
                var result = PathUtils.GetUrlExtension(url, includeDot);
                Assert.That(result, Is.EqualTo(expected), 
                    $"Failed for URL: '{url}' with includeDot={includeDot}");
            }
        }

        [Test]
        public void GetUrlExtension_WithUrlsWithoutExtension_ReturnsNull()
        {
            var testCases = new[]
            {
                "https://example.com/no-extension",
                "https://example.com/.hiddenfile",
                "https://example.com/",
                "https://example.com/path/only/",
                "",
                null
            };

            foreach (var url in testCases)
            {
                var result = PathUtils.GetUrlExtension(url);
                Assert.That(result, Is.Null, $"Expected null for URL: '{url}'");
            }
        }
    }
}