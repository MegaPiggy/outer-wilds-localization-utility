﻿using OWML.ModHelper;
using System;
using UnityEngine;

namespace LocalizationUtility
{
    public class CustomLanguage
    {
        public string Name { get; private set; }
        public string TranslationPath { get; private set; }
        public Font Font { get; private set; }
        public Func<string, string> Fixer { get; private set; }

        private string _fontAssetBundlePath;
        private string _fontAssetPath;

        public CustomLanguage(string name, string translationPath, string assetBundlePath, string fontPath, Func<string, string> fixer, ModBehaviour mod)
        {
            Name = name;
            TranslationPath = mod.ModHelper.Manifest.ModFolderPath + translationPath;
            _fontAssetBundlePath = mod.ModHelper.Manifest.ModFolderPath + assetBundlePath;
            _fontAssetPath = fontPath;

            try
            {
                Font = AssetBundle.LoadFromFile(_fontAssetBundlePath).LoadAsset<Font>(_fontAssetPath);
            }
            catch (Exception) { };

            if (Font == null) LocalizationUtility.WriteError($"Couldn't load font at {_fontAssetPath} in bundle {_fontAssetBundlePath}");

            Fixer = fixer;
        }

        public CustomLanguage(string name, string translationPath, ModBehaviour mod)
        {
            Name = name;
            TranslationPath = mod.ModHelper.Manifest.ModFolderPath + translationPath;
        }
    }
}