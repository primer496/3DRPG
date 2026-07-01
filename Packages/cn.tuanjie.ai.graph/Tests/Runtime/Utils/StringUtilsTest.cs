using NUnit.Framework;

namespace UnityEngine.AIGraph.Tests
{
    [TestFixture]
    public class StringUtilsTest
    {
        [Test]
        public void CleanFileName_WithBasicCases_ReturnsExpectedResult()
        {
            var testCases = new[]
            {
                // 空值和边界情况
                (Input: null, Expected: "model"),
                (Input: "", Expected: "model"),
                (Input: "   ", Expected: "model"),

                // 合法文件名
                (Input: "file.txt", Expected: "file.txt"),
                (Input: "document.pdf", Expected: "document.pdf"),
                (Input: "image.png", Expected: "image.png"),
                (Input: "data.json", Expected: "data.json"),

                // 无扩展名情况
                (Input: "file", Expected: "file"),
                (Input: ".gitignore", Expected: "model.gitignore"),
                (Input: "no_extension", Expected: "no_extension"),

                // 大小写敏感性
                (Input: "FILE.TXT", Expected: "FILE.TXT"),
                (Input: "Document.PDF", Expected: "Document.PDF"),
                (Input: "mixedCase.File", Expected: "mixedCase.File")
            };

            foreach (var (input, expected) in testCases)
            {
                var result = StringUtils.CleanFileName(input);
                Assert.That(result, Is.EqualTo(expected), $"Failed for input: '{input}'");
            }
        }

        [Test]
        public void CleanFileName_WithInvalidCharacters_RemovesInvalidChars()
        {
            var testCases = new[]
            {
                (Input: "file<>.txt", Expected: "file.txt"),
                (Input: "doc:ument.pdf", Expected: "document.pdf"),
                (Input: "image\\|.png", Expected: "image.png"),
                (Input: "data*.json", Expected: "data.json"),
                (Input: "test?.xml", Expected: "test.xml"),
                (Input: "file\"\".csv", Expected: "file.csv")
            };

            foreach (var (input, expected) in testCases)
            {
                var result = StringUtils.CleanFileName(input);
                Assert.That(result, Is.EqualTo(expected), $"Failed for input: '{input}'");
            }
        }

        [Test]
        public void CleanFileName_WithChineseCharacters_RemovesChinese()
        {
            var testCases = new[]
            {
                (Input: "中文文件.txt", Expected: "model.txt"),
                (Input: "测试文档.pdf", Expected: "model.pdf"),
                (Input: "file中文name.png", Expected: "filename.png"),
                (Input: "文档.docx", Expected: "model.docx")
            };

            foreach (var (input, expected) in testCases)
            {
                var result = StringUtils.CleanFileName(input);
                Assert.That(result, Is.EqualTo(expected), $"Failed for input: '{input}'");
            }
        }

        [Test]
        public void CleanFileName_WithDotHandling_HandlesCorrectly()
        {
            var testCases = new[]
            {
                (Input: "file.name.with.dots.txt", Expected: "file.name.with.dots.txt"),
                (Input: "..hidden..", Expected: "hidden"),
                (Input: "file..txt", Expected: "file.txt"),
                (Input: ".config", Expected: "model.config"),
                (Input: "file.tar.gz", Expected: "file.tar.gz"),
                (Input: "archive.zip.001", Expected: "archive.zip.001"),
                (Input: "backup.2023.12.01.zip", Expected: "backup.2023.12.01.zip")
            };

            foreach (var (input, expected) in testCases)
            {
                var result = StringUtils.CleanFileName(input);
                Assert.That(result, Is.EqualTo(expected), $"Failed for input: '{input}'");
            }
        }

        [Test]
        public void CleanFileName_WithReservedNames_UsesDefault()
        {
            var testCases = new[]
            {
                (Input: "con.txt", Expected: "con.txt"),
                (Input: "prn.jpg", Expected: "prn.jpg"),
                (Input: "aux.png", Expected: "aux.png"),
                (Input: "nul.pdf", Expected: "nul.pdf"),
                (Input: "com1.docx", Expected: "com1.docx")
            };

            foreach (var (input, expected) in testCases)
            {
                var result = StringUtils.CleanFileName(input);
                Assert.That(result, Is.EqualTo(expected), $"Failed for input: '{input}'");
            }
        }

        [Test]
        public void CleanFileName_WithLongNames_TruncatesAppropriately()
        {
            var testCases = new[]
            {
                (Input: "a".PadRight(300) + ".txt", Expected: "a.txt"),
                (Input: "test." + new string('x', 20), Expected: "test.xxxxxxxxxx")
            };

            foreach (var (input, expected) in testCases)
            {
                var result = StringUtils.CleanFileName(input);
                Assert.That(result, Is.EqualTo(expected), $"Failed for input: '{input}'");
            }
        }

        [Test]
        public void UnderScoreToPascalCase_WithBasicCases_ReturnsExpectedResult()
        {
            var testCases = new[]
            {
                // 空值和边界情况
                (Input: null, Expected: null),
                (Input: "", Expected: ""),
                (Input: "___", Expected: ""),
                (Input: " _ __ _ ", Expected: ""),

                // 单单词转换
                (Input: "single", Expected: "Single"),
                (Input: "WORD", Expected: "Word"),
                (Input: "word", Expected: "Word"),
                (Input: "w", Expected: "W"),

                // 多单词转换
                (Input: "user_name", Expected: "UserName"),
                (Input: "first_name", Expected: "FirstName"),
                (Input: "last_name", Expected: "LastName"),
                (Input: "email_address", Expected: "EmailAddress"),
                (Input: "phone_number", Expected: "PhoneNumber"),
                (Input: "created_at", Expected: "CreatedAt"),
                (Input: "updated_at", Expected: "UpdatedAt"),
                (Input: "is_active", Expected: "IsActive"),
                (Input: "has_permission", Expected: "HasPermission"),
                (Input: "api_key", Expected: "ApiKey"),

                // 常见技术模式
                (Input: "xml_parser", Expected: "XmlParser"),
                (Input: "html_content", Expected: "HtmlContent"),
                (Input: "json_data", Expected: "JsonData"),
                (Input: "url_path", Expected: "UrlPath"),
                (Input: "ip_address", Expected: "IpAddress")
            };

            foreach (var (input, expected) in testCases)
            {
                var result = StringUtils.UnderScoreToPascalCase(input);
                Assert.That(result, Is.EqualTo(expected), $"Failed for input: '{input}'");
            }
        }

        [Test]
        public void UnderScoreToPascalCase_WithCaseVariations_ConvertsCorrectly()
        {
            var testCases = new[]
            {
                // 全大写输入
                (Input: "USER_NAME", Expected: "UserName"),
                (Input: "FIRST_NAME", Expected: "FirstName"),
                (Input: "LAST_NAME", Expected: "LastName"),
                (Input: "EMAIL_ADDRESS", Expected: "EmailAddress"),
                (Input: "API_KEY", Expected: "ApiKey"),

                // 混合大小写输入
                (Input: "User_Name", Expected: "UserName"),
                (Input: "First_Name", Expected: "FirstName"),
                (Input: "Last_Name", Expected: "LastName"),
                (Input: "Email_Address", Expected: "EmailAddress"),
                (Input: "Api_Key", Expected: "ApiKey")
            };

            foreach (var (input, expected) in testCases)
            {
                var result = StringUtils.UnderScoreToPascalCase(input);
                Assert.That(result, Is.EqualTo(expected), $"Failed for input: '{input}'");
            }
        }

        [Test]
        public void UnderScoreToPascalCase_WithUnderscoreVariations_HandlesCorrectly()
        {
            var testCases = new[]
            {
                // 多个连续下划线
                (Input: "user__name", Expected: "UserName"),
                (Input: "first___name", Expected: "FirstName"),

                // 前后下划线
                (Input: "__last_name__", Expected: "LastName"),
                (Input: "_email_address_", Expected: "EmailAddress"),
                (Input: "phone_number_", Expected: "PhoneNumber"),
                (Input: "_created_at", Expected: "CreatedAt"),

                // 边缘情况
                (Input: "only_underscores", Expected: "OnlyUnderscores"),
                (Input: "multiple__underscores", Expected: "MultipleUnderscores"),
                (Input: "trailing_underscore_", Expected: "TrailingUnderscore"),
                (Input: "_leading_underscore", Expected: "LeadingUnderscore")
            };

            foreach (var (input, expected) in testCases)
            {
                var result = StringUtils.UnderScoreToPascalCase(input);
                Assert.That(result, Is.EqualTo(expected), $"Failed for input: '{input}'");
            }
        }

        [Test]
        public void UnderScoreToPascalCase_WithSpecialCharacters_HandlesCorrectly()
        {
            var testCases = new[]
            {
                // 包含数字
                (Input: "user_id_123", Expected: "UserId123"),
                (Input: "item_2", Expected: "Item2"),
                (Input: "test_123_abc", Expected: "Test123Abc"),
                (Input: "version_2_0", Expected: "Version20"),

                // 单字母部分
                (Input: "a_b_c", Expected: "ABC"),
                (Input: "x_y_z", Expected: "XYZ"),
                (Input: "a_b", Expected: "AB")
            };

            foreach (var (input, expected) in testCases)
            {
                var result = StringUtils.UnderScoreToPascalCase(input);
                Assert.That(result, Is.EqualTo(expected), $"Failed for input: '{input}'");
            }
        }
    }
}