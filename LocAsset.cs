/*************************************
 * ©   ©   ©   ©   ©   ©   ©   ©   © *
 * LocAsset.cs                       *
 * Created by: TheFallender          *
 * Created on: 27/02/2021 (dd/mm/yy) *
 * ©   ©   ©   ©   ©   ©   ©   ©   © *
 *************************************/

using UnityEngine;
using System;
using System.Collections.Generic;

namespace UniLoc {
    //Asset to use
    [CreateAssetMenu(fileName = "LocalizationData", menuName = "UniLoc/Localization ")]
    public class LocAsset : ScriptableObject {
        public List<UniLocLangs> availableLangs = new List<UniLocLangs>();
        public List<LocKey> localizationKeys = new List<LocKey>();
    }

    //Localization keys for each of the localized texts
    [Serializable]
    public class LocKey {
        public string key;
        public List<LangText> value;

        public LocKey (string _key, List<LangText> _value) {
            key = _key;
            value = _value;
        }

        public LocKey (string _key, List<UniLocLangs> availableLangs) {
            key = _key;
            List<LangText> langTextsTemp = new List<LangText>();
            foreach (UniLocLangs lang in availableLangs)
                langTextsTemp.Add(new LangText(lang, ""));
            value = langTextsTemp;
        }
    }

    //Each of the texts that belong to each language
    [Serializable]
    public class LangText {
        public UniLocLangs key;
        public string value;

        public LangText (UniLocLangs _key, string _value) {
            key = _key;
            value = _value;
        }
    }

    //Similar to Unity's Languages but making them stable through releases
    //as Unity has some duplicates on specific versions
    public enum UniLocLangs {
        Afrikaans = 0,
        Arabic = 1,
        Basque = 2,
        Belarusian = 3,
        Bulgarian = 4,
        Catalan = 5,
        Chinese = 6,
        Czech = 7,
        Danish = 8,
        Dutch = 9,
        English = 10,
        Estonian = 11,
        Faroese = 12,
        Finnish = 13,
        French = 14,
        German = 15,
        Greek = 16,
        Hebrew = 17,
        Hungarian = 18,
        Icelandic = 19,
        Indonesian = 20,
        Italian = 21,
        Japanese = 22,
        Korean = 23,
        Latvian = 24,
        Lithuanian = 25,
        Norwegian = 26,
        Polish = 27,
        Portuguese = 28,
        Romanian = 29,
        Russian = 30,
        SerboCroatian = 31,
        Slovak = 32,
        Slovenian = 33,
        Spanish = 34,
        Swedish = 35,
        Thai = 36,
        Turkish = 37,
        Ukrainian = 38,
        Vietnamese = 39,
        ChineseSimplified = 40,
        ChineseTraditional = 41,
        Unknown = 42
    }
}