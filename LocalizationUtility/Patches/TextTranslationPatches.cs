﻿using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

namespace LocalizationUtility
{
    [HarmonyPatch]
    public static class TextTranslationPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(TextTranslation), nameof(TextTranslation.SetLanguage))]
        public static bool TextTranslation_SetLanguage(TextTranslation.Language lang, TextTranslation __instance)
        {
            if (lang > TextTranslation.Language.TURKISH) lang = TextTranslation.Language.ENGLISH;

            if (lang != TextTranslation.Language.ENGLISH) return true;

            __instance.m_language = TextTranslation.Language.ENGLISH;

            var language = LocalizationUtility.Instance.GetLanguage();

            var path = language.TranslationPath;

            LocalizationUtility.WriteLine($"Loading translation from {path}");

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(ReadAndRemoveByteOrderMarkFromPath(path));

                var translationTableNode = xmlDoc.SelectSingleNode("TranslationTable_XML");
                var translationTable_XML = new TextTranslation.TranslationTable_XML();

                // Add regular text to the table
                foreach (XmlNode node in translationTableNode.SelectNodes("entry"))
                {
                    var key = node.SelectSingleNode("key").InnerText;
                    var value = node.SelectSingleNode("value").InnerText;

                    if (language.Fixer != null) value = language.Fixer(value);

                    translationTable_XML.table.Add(new TextTranslation.TranslationTableEntry(key, value));
                }

                // Add ship log entries
                foreach (XmlNode node in translationTableNode.SelectSingleNode("table_shipLog").SelectNodes("TranslationTableEntry"))
                {
                    var key = node.SelectSingleNode("key").InnerText;
                    var value = node.SelectSingleNode("value").InnerText;

                    if (language.Fixer != null) value = language.Fixer(value);

                    translationTable_XML.table_shipLog.Add(new TextTranslation.TranslationTableEntry(key, value));
                }

                // Add UI
                foreach (XmlNode node in translationTableNode.SelectSingleNode("table_ui").SelectNodes("TranslationTableEntryUI"))
                {
                    // Keys for UI are all ints
                    var key = int.Parse(node.SelectSingleNode("key").InnerText);
                    var value = node.SelectSingleNode("value").InnerText;

                    if (language.Fixer != null) value = language.Fixer(value);

                    translationTable_XML.table_ui.Add(new TextTranslation.TranslationTableEntryUI(key, value));
                }

                __instance.m_table = new TextTranslation.TranslationTable(translationTable_XML);

                // Goofy stuff to envoke event
                var onLanguageChanged = (MulticastDelegate)__instance.GetType().GetField("OnLanguageChanged", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
                if (onLanguageChanged != null)
                {
                    onLanguageChanged.DynamicInvoke();
                }
            }
            catch (Exception e)
            {
                LocalizationUtility.WriteError($"Couldn't load translation: {e.Message}{e.StackTrace}");
                return true;
            }

            return false;
        }

        public static string ReadAndRemoveByteOrderMarkFromPath(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            byte[] preamble1 = Encoding.UTF8.GetPreamble();
            byte[] preamble2 = Encoding.Unicode.GetPreamble();
            byte[] preamble3 = Encoding.BigEndianUnicode.GetPreamble();
            if (bytes.StartsWith(preamble1))
                return Encoding.UTF8.GetString(bytes, preamble1.Length, bytes.Length - preamble1.Length);
            if (bytes.StartsWith(preamble2))
                return Encoding.Unicode.GetString(bytes, preamble2.Length, bytes.Length - preamble2.Length);
            return bytes.StartsWith(preamble3) ? Encoding.BigEndianUnicode.GetString(bytes, preamble3.Length, bytes.Length - preamble3.Length) : Encoding.UTF8.GetString(bytes);
        }
    }
}
