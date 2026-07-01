using System;
using System.Collections.Generic;
using System.IO;

namespace UnityEngine.AIGraph
{
    /// <summary>
    /// utils for text-to-speech whisper pipeline
    /// </summary>
    public static class TTSUtils
    {
        public static readonly string[] whisperPhonemes = new string[] {
        "<blank>", "<unk>", "AH0", "N", "T", "D", "S", "R", "L", "DH", "K", "Z", "IH1",
        "IH0", "M", "EH1", "W", "P", "AE1", "AH1", "V", "ER0", "F", ",", "AA1", "B",
        "HH", "IY1", "UW1", "IY0", "AO1", "EY1", "AY1", ".", "OW1", "SH", "NG", "G",
        "ER1", "CH", "JH", "Y", "AW1", "TH", "UH1", "EH2", "OW0", "EY2", "AO0", "IH2",
        "AE2", "AY2", "AA2", "UW0", "EH0", "OY1", "EY0", "AO2", "ZH", "OW2", "AE0", "UW2",
        "AH2", "AY0", "IY2", "AW2", "AA0", "\"", "ER2", "UH2", "?", "OY2", "!", "AW0",
        "UH0", "OY0", "..", "<sos/eos>" };

        /// <summary>
        /// 查询CMU的标准音素字典
        /// </summary>
        public static Dictionary<string, string> ReadPhonemeDictionary(string dictPath, string dictUrl = null)
        {
            Dictionary<string, string> phonemeDict = new();
            string[] words = null;
            if (File.Exists(dictPath))
            {
                // step 1-1: try to get phoneme dict from local path
                words = File.ReadAllLines(dictPath);
            }
            else
            {
                // TODO: step 1-2: try to download from url
            }
            if (words == null)
                throw new ArgumentNullException($"No phoneme dict found at {dictPath} or {dictUrl}!");
            // step 2: parse phoneme_dict.txt
            for (int i = 0; i < words.Length; i++)
            {
                string s = words[i];
                string[] parts = s.Split();
                if (parts[0] != ";;;") //ignore comments in file
                {
                    string key = parts[0];
                    phonemeDict.Add(key, s[(key.Length + 2)..]);
                }
            }
            // Add codes for punctuation to the dictionary
            phonemeDict.Add(",", ",");
            phonemeDict.Add(".", ".");
            phonemeDict.Add("!", "!");
            phonemeDict.Add("?", "?");
            phonemeDict.Add("\"", "\"");
            // You could add extra word pronounciations here e.g.
            //dict.Add("somenewword","[phonemes]");
            return phonemeDict;
        }

        /// <summary>
        /// number translation
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string ExpandNumbers(string text)
        {
            return text
                .Replace("0", " ZERO ")
                .Replace("1", " ONE ")
                .Replace("2", " TWO ")
                .Replace("3", " THREE ")
                .Replace("4", " FOUR ")
                .Replace("5", " FIVE ")
                .Replace("6", " SIX ")
                .Replace("7", " SEVEN ")
                .Replace("8", " EIGHT ")
                .Replace("9", " NINE ");
        }

        /// <summary>
        /// Decode the word into phenomes by looking for the longest word in the dictionary that matches
        /// the first part of the word and so on. 
        /// This works fairly well but could be improved. The original paper had a model that
        /// dealt with guessing the phonemes of words
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public static string WordToPhonemes(string word, Dictionary<string, string> phonemeDict)
        {
            string output = "";
            int start = 0;
            for (int end = word.Length; end >= 0 && start < word.Length; end--)
            {
                if (end <= start) //no matches
                {
                    start++;
                    end = word.Length + 1;
                    continue;
                }
                string subword = word.Substring(start, end - start);
                if (phonemeDict.TryGetValue(subword, out string value))
                {
                    output += value + " ";
                    start = end;
                    end = word.Length + 1;
                }
            }
            return output;
        }

        public static int[] GetTokens(string ptext, string[] phonemes)
        {
            string[] p = ptext.Split();
            var tokens = new int[p.Length];
            for (int i = 0; i < tokens.Length; i++)
            {
                tokens[i] = Mathf.Max(0, System.Array.IndexOf(phonemes, p[i]));
            }
            return tokens;
        }
    }
}