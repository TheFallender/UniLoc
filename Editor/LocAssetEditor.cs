/*************************************
 * ©   ©   ©   ©   ©   ©   ©   ©   © *
 * LocAssetEditor.cs                 *
 * Created by: TheFallender          *
 * Created on: 27/02/2021 (dd/mm/yy) *
 * ©   ©   ©   ©   ©   ©   ©   ©   © *
 *************************************/

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;


namespace UniLoc {
    [CustomEditor(typeof(LocAsset))]
    public class LocAssetEditor : Editor {
        //Asset
        private LocAsset locAsset;

        //Serialized object
        private SerializedObject locSerialized;

        //Properties
        private SerializedProperty avlLangs;
        private SerializedProperty locKeys;

        //Variables
        bool openKeyLocsSettings = false;
        int settingsAddKeysNumber = 1;
        int settingsAddKeyAtIndex = 1;
        int settingsAddKeysAtIndex = 1;
        int settingsAddKeysAtIndexNumber = 1;

        List<bool> shownLocKeys;

        //When it gets enabled
        private void OnEnable () {
            locAsset = (LocAsset) target;                      //Set the target
            locSerialized = new SerializedObject(locAsset);             //Serialize the object for modification
            avlLangs = locSerialized.FindProperty("availableLangs");    //Set lang property
            locKeys = locSerialized.FindProperty("localizationKeys");   //Set localization keys property
            shownLocKeys = new List<bool>(new bool[locAsset.localizationKeys.Count]);
        }

        //Whenever the GUI is being used
        public override void OnInspectorGUI () {
            //Update the status of the list
            locSerialized.Update();

            //Show the Languages and the settings
            LanguageShow();

            //Padding
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            //Show the Localization Keys and the settings
            LocalizationKeysShow();

            //Apply modifications
            locSerialized.ApplyModifiedProperties();
        }

        //Language show
        private void LanguageShow () {
            //Language Variables
            int langsCount = locAsset.availableLangs.Count; //Number of languages
            bool langsAdded = false;                        //Wether there has been any lang added
            bool langsDeleted = false;                      //Wether there has been any lang deleted

            //Label for the Langs
            EditorGUILayout.LabelField("Languages:", EditorStyles.boldLabel);

            //Text to show if there are no languages
            if (langsCount == 0)
                EditorGUILayout.LabelField("\tNo languages found. Try to add some.");
            else {
                //Show all the languages and allow the modification
                for (int i = 0; i < langsCount; i++) {
                    //Serialized Properties
                    SerializedProperty lang = avlLangs.GetArrayElementAtIndex(i);    //Get the element serialized

                    EditorGUILayout.BeginHorizontal();

                    //PopUp Selector
                    lang.enumValueIndex = (int) (UniLocLangs) EditorGUILayout.EnumPopup(         //Enum PopUp, needs casting
                        string.Format("\t{0}. {1}: ", i + 1, (UniLocLangs) lang.intValue),       //Label the language
                        (UniLocLangs) lang.enumValueIndex                                        //Selected value
                    );

                    //Button to Delete the language
                    if (GUILayout.Button("x", GUILayout.ExpandWidth(false))) {
                        avlLangs.DeleteArrayElementAtIndex(i);  //Delete the element
                        langsCount--;                           //Reduce the number of availableLangs
                        i--;                                    //Reduce i to iterate through the next lang
                        langsDeleted = true;
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }

            //Padding
            EditorGUILayout.Space();

            //Option to add the language
            if (GUILayout.Button("Add New Language", GUILayout.ExpandWidth(false))) {
                locAsset.availableLangs.Add(UniLocLangs.Unknown);
                langsAdded = true;
            }

            //Apply modifications
            if (langsAdded || langsDeleted) {
                locSerialized.ApplyModifiedProperties();
                //Resize Localization Keys and if there was a change, apply the modifications
                ResizeLocKeys(langsAdded, langsDeleted);
            }
        }

        //Resize the Localization Keys
        private void ResizeLocKeys (bool langsAdded, bool langsDeleted) {
            //Go through each key resizing it
            foreach (LocKey locKey in locAsset.localizationKeys) {
                //Add missing langs
                if (langsAdded) {
                    //Loop for each of the languages available on the system
                    foreach (UniLocLangs lang in locAsset.availableLangs) {
                        //If the key does not contain the language
                        if (locKey.value.Find(value => value.key == lang) == null)
                            locKey.value.Add(new LangText(lang, ""));
                    }
                }

                //Remove extra langs
                if (langsDeleted) {
                    //Loop for on each of the languages in the key and remove them if they are not found in the available langs
                    for (int i = 0, langsCountInKey = locKey.value.Count; i < langsCountInKey; i++) {
                        //If the available langs do not contain the language, delete it
                        if (!locAsset.availableLangs.Contains(locKey.value[i].key)) {
                            locKey.value.Remove(locKey.value[i]);   //Remove the language in the key
                            langsCountInKey--;                      //Decrease to adjust to the real size of the array
                            i--;                                    //Decrease for the next iteration
                        }
                    }
                }
            }

            //Apply modifications
            locSerialized.ApplyModifiedProperties();
        }

        //Localization Keys
        private void LocalizationKeysShow () {
            //Variables to use
            int locKeysCount = locAsset.localizationKeys.Count;

            //Label for the Localization Keys
            EditorGUILayout.LabelField("Key Localizations:", EditorStyles.boldLabel);

            //Show the Localization Key Settings
            LocalizationKeysSettings(locKeysCount);

            //Text to show if there are no keys
            if (locKeysCount == 0)
                EditorGUILayout.LabelField("\tNo keys found.");
            else {
                //Show all the languages and allow the modification
                for (int i = 0; i < locKeysCount; i++) {
                    //Serialized Properties
                    SerializedProperty locKey = locKeys.GetArrayElementAtIndex(i);      //Get the element serialized
                    SerializedProperty key = locKey.FindPropertyRelative("key");        //Get the key of the localization key
                    SerializedProperty value = locKey.FindPropertyRelative("value");    //Get the key of the localization value

                    //GUI Elements
                    EditorGUILayout.BeginHorizontal();


                    //Detect how many keys are missing
                    string foldoutText = "";
                    for (int j = 0, missingKeys = 0; j < value.arraySize; j++) {
                        //Presets
                        SerializedProperty langText = value.GetArrayElementAtIndex(j);
                        SerializedProperty text = langText.FindPropertyRelative("value");

                        //If it's empty, add it to the missing keys
                        if (string.IsNullOrEmpty(text.stringValue))
                            missingKeys++;      //Missing Keys add 

                        //Last iteration
                        if (j + 1 == value.arraySize) {
                            if (missingKeys == 0) {                         //If there are no missing keys, set the default text
                                foldoutText = string.Format("\t{0}. Key:", i + 1);
                            } else if (missingKeys < value.arraySize) {     //If some keys are missing, set the missing text
                                foldoutText = string.Format("\t{0}. Key: (Missing)", i + 1);
                                GUI.contentColor = Color.yellow;
                            } else {                                        //If all of the keys are missings, set undefined text
                                foldoutText = string.Format("\t{0}. Key: (Undefined)", i + 1);
                                GUI.contentColor = Color.red;
                            }
                        }
                    }

                    //Fold Out for the langs on each key
                    shownLocKeys[i] = EditorGUILayout.Foldout(shownLocKeys[i], foldoutText, true);
                    GUI.contentColor = Color.white; //Reset the color

                    //Property field for the key
                    EditorGUILayout.PropertyField(
                        key,
                        new GUIContent(""),
                        GUILayout.ExpandWidth(true)
                    );

                    //Button to Delete the Localization Key
                    if (GUILayout.Button("x", GUILayout.ExpandWidth(false))) {
                        locKeys.DeleteArrayElementAtIndex(i);   //Delete the element
                        shownLocKeys.RemoveAt(i);               //Delete shown val
                        locKeysCount--;                         //Reduce the number of localization keys
                        i--;                                    //Reduce i to iterate through the next key
                        EditorGUILayout.EndHorizontal();
                        //Apply modifications
                        locSerialized.ApplyModifiedProperties();
                        continue;
                    }

                    EditorGUILayout.EndHorizontal();

                    //Show the langs and each of the values
                    if (shownLocKeys[i]) {
                        for (int j = 0; j < value.arraySize; j++) {
                            SerializedProperty langText = value.GetArrayElementAtIndex(j);
                            SerializedProperty lang = langText.FindPropertyRelative("key");
                            SerializedProperty text = langText.FindPropertyRelative("value");

                            EditorGUILayout.BeginHorizontal();

                            //Property field for the language
                            EditorGUILayout.LabelField(
                                "\t    " + ((UniLocLangs) lang.enumValueIndex).ToString(),
                                GUILayout.Width(160)
                            );
                            //Property field for the text
                            EditorGUILayout.PropertyField(
                                text,
                                new GUIContent(""),
                                GUILayout.ExpandWidth(true)
                            );

                            EditorGUILayout.EndHorizontal();
                        }
                    }

                    locSerialized.ApplyModifiedProperties();
                }
            }
        }

        //Settings available for the localization key settings
        private void LocalizationKeysSettings (int locKeysCount) {
            //Fold Out for the langs on each key
            openKeyLocsSettings = EditorGUILayout.Foldout(openKeyLocsSettings, "    Settings:", true);
            if (openKeyLocsSettings) {
                //Add Localization Key
                {
                    //Buttons to add key
                    bool settingsNewLocKey = GUILayout.Button("Add New Key", GUILayout.ExpandWidth(true));

                    //Add a new localization key
                    if (settingsNewLocKey) {
                        locAsset.localizationKeys.Add(new LocKey("", locAsset.availableLangs));
                        shownLocKeys.Add(false);
                        locSerialized.ApplyModifiedProperties();
                    }
                }

                //Add Multiple Keys
                {
                    //Buttons to add keys with the button and a field 
                    EditorGUILayout.BeginHorizontal();
                    bool settingsNewLocKeys = GUILayout.Button("Add New Keys. Nº of keys to add", GUILayout.ExpandWidth(true));
                    settingsAddKeysNumber = EditorGUILayout.IntField(settingsAddKeysNumber, GUILayout.Width(60));
                    EditorGUILayout.EndHorizontal();

                    //Localization Keys Add
                    if (settingsNewLocKeys) {
                        //Add the localization keys empty
                        for (int i = 0; i < settingsAddKeysNumber; i++) {
                            locAsset.localizationKeys.Add(new LocKey("", locAsset.availableLangs));
                            shownLocKeys.Add(false);
                        }
                        locSerialized.ApplyModifiedProperties();
                    }
                }

                //Add a key at an index
                {
                    //Buttons to add key
                    EditorGUILayout.BeginHorizontal();
                    bool settingsNewLocKeyAtIndex = GUILayout.Button("Add New Key at index", GUILayout.ExpandWidth(true));
                    settingsAddKeyAtIndex = EditorGUILayout.IntField(settingsAddKeyAtIndex, GUILayout.Width(60));
                    EditorGUILayout.EndHorizontal();

                    //Add the localization key at an specific index
                    if (settingsNewLocKeyAtIndex) {
                        if (settingsAddKeyAtIndex > 0 && settingsAddKeyAtIndex < locKeysCount) {
                            locAsset.localizationKeys.Insert(settingsAddKeyAtIndex - 1, new LocKey("", locAsset.availableLangs));
                            shownLocKeys.Insert(settingsAddKeyAtIndex - 1, false);
                            locSerialized.ApplyModifiedProperties();
                        } else
                            Debug.LogError("ERROR - Index must be positive and less than the size of the keys.");
                    }
                }


                {
                    //Buttons to add key
                    bool settingsNewLocKeysAtIndex = GUILayout.Button("Add New Key at index. Nº of keys to add", GUILayout.ExpandWidth(true));
                    settingsAddKeysAtIndex = EditorGUILayout.IntField("\tIndex:", settingsAddKeysAtIndex);
                    settingsAddKeysAtIndexNumber = EditorGUILayout.IntField("\tNumber:", settingsAddKeysAtIndexNumber);

                    //Add multiple localization keys at an specific index
                    if (settingsNewLocKeysAtIndex) {
                        if (settingsAddKeysAtIndex > 0 && settingsAddKeysAtIndex < locKeysCount) {
                            for (int i = 0; i < settingsAddKeysAtIndexNumber; i++) {
                                locAsset.localizationKeys.Insert(settingsAddKeysAtIndex - 1, new LocKey("", locAsset.availableLangs));
                                shownLocKeys.Insert(settingsAddKeysAtIndex - 1, false);
                            }
                            locSerialized.ApplyModifiedProperties();
                        } else
                            Debug.LogError("ERROR - Index must be positive and less than the size of the keys.");
                    }
                }

                {
                    //Purge WhiteSpaces
                    bool settingsPurgeWhitespaces = GUILayout.Button("Remove Spaces from Keys", GUILayout.ExpandWidth(true));

                    //Clean all the whitespaces in the keys, as they should be clean
                    if (settingsPurgeWhitespaces) {
                        //Regex to use with same pattern
                        System.Text.RegularExpressions.Regex rgxCleaner = new System.Text.RegularExpressions.Regex(@"\s+");

                        //Loop through each of the localization keys
                        for (int i = 0; i < locKeysCount; i++) {
                            //Serialized Properties
                            SerializedProperty locKey = locKeys.GetArrayElementAtIndex(i);      //Get the element serialized
                            SerializedProperty key = locKey.FindPropertyRelative("key");        //Get the key of the localization key

                            //Clean the whitespaces
                            key.stringValue = rgxCleaner.Replace(key.stringValue, "");
                        }

                        locSerialized.ApplyModifiedProperties();
                    }
                }


                {
                    //Purge end WhiteSpaces from definition
                    bool settingsPurgeLocalizationSpaces = GUILayout.Button("Remove spaces at the end of localization strings", GUILayout.ExpandWidth(true));

                    //Clean all the whitespaces in the localization strings, as they should be clean
                    if (settingsPurgeLocalizationSpaces) {
                        //Loop through each of the localization keys
                        for (int i = 0; i < locKeysCount; i++) {
                            //Serialized Properties
                            SerializedProperty locKey = locKeys.GetArrayElementAtIndex(i);
                            SerializedProperty value = locKey.FindPropertyRelative("value");

                            //Loop through each of the strings
                            for (int j = 0; j < value.arraySize; j++) {
                                SerializedProperty langText = value.GetArrayElementAtIndex(j);
                                SerializedProperty text = langText.FindPropertyRelative("value");

                                //Trim both start and end
                                text.stringValue = text.stringValue.Trim();
                            }
                        }

                        locSerialized.ApplyModifiedProperties();
                    }
                }
            }

            //Padding
            EditorGUILayout.Space();
        }
    }
}