using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UnityEngine.AIGraph
{
    /// <summary>
    /// utils for text-to-speech whisper pipeline
    /// </summary>
    public static class STTUtils
    {
        //Special tokens see added tokens file for details
        public const int END_OF_TEXT = 50257;
        public const int START_OF_TRANSCRIPT = 50258;
        public const int ENGLISH = 50259;
        public const int GERMAN = 50261;
        public const int FRENCH = 50265;
        public const int TRANSCRIBE = 50359; //for speech-to-text in specified language
        public const int TRANSLATE = 50358;  //for speech-to-text then translate to English
        public const int NO_TIME_STAMPS = 50363;
        public const int START_TIME = 50364;

        public static string[] LoadVocabs(string vocabPath)
        {
            var jsonText = File.ReadAllText(vocabPath);
            var vocab = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, int>>(jsonText);
            string[] tokenToVocab = new string[vocab.Count];
            foreach (var item in vocab)
            {
                tokenToVocab[item.Value] = item.Key;
            }
            return tokenToVocab;
        }

        public static string GetUnicodeText(string text, int[] whiteSpaceCharacters)
        {
            var bytes = Encoding.GetEncoding("ISO-8859-1").GetBytes(ShiftCharacterDown(text, whiteSpaceCharacters));
            return Encoding.UTF8.GetString(bytes);
        }

        public static string ShiftCharacterDown(string text, int[] whiteSpaceCharacters)
        {
            string outText = "";
            foreach (char letter in text)
            {
                outText += ((int)letter <= 256) ? letter :
                    (char)whiteSpaceCharacters[(int)(letter - 256)];
            }
            return outText;
        }

        /// <summary>
        /// TOCHECK: 拎出unicode中的特殊字符，如果是中文要怎么做？
        /// </summary>
        public static int[] SetupWhiteSpaceShifts()
        {
            int[] whiteSpaceCharacters = new int[256];
            for (int i = 0, n = 0; i < 256; i++)
            {
                if (IsWhiteSpace((char)i)) whiteSpaceCharacters[n++] = i;
            }
            return whiteSpaceCharacters;
        }

        public static bool IsWhiteSpace(char c)
        {
            return !(('!' <= c && c <= '~') || ('¡' <= c && c <= '¬') || ('®' <= c && c <= 'ÿ'));
        }
    }
}