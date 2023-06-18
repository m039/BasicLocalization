using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using SimpleJSON;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace m039.BasicLocalization
{
    public static class BasicLocalizationGoogleTranslator
    {
        public static void Translate(BasicLocalizationProfile profile)
        {
            var defaultLanguage = profile.defaultLanguage;

            if (defaultLanguage == null)
            {
                Debug.LogError("The default language is not set.");
                return;
            }

            var languages = profile.languages;

            var commands = new List<TranslateCommand>();

            foreach (var translation in profile.translations)
            {
                var defaultText = translation.GetTranslation(defaultLanguage);
                if (string.IsNullOrEmpty(defaultText))
                {
                    Debug.LogWarning($"The translation for the default language with key '{translation.key}' is empty.");
                    continue;
                }

                foreach (var language in languages)
                {
                    if (language == defaultLanguage)
                        continue;

                    var text = translation.GetTranslation(language);
                    if (!profile.editorData.translateOnlyEmptyFields || string.IsNullOrEmpty(text))
                    {
                        commands.Add(new TranslateCommand
                        {
                            languageFrom = defaultLanguage,
                            languageTo = language,
                            translation = translation
                        });
                    }
                }
            }

            Undo.RecordObject(profile, "Translate");

            for (int i = 0; i < commands.Count; i++)
            {
                var command = commands[i];
                if (EditorUtility.DisplayCancelableProgressBar("Translating", command.GetInfo(), (float)(i + 1) / commands.Count))
                {
                    break;
                }
                command.Run();
            }

            EditorUtility.ClearProgressBar();

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
        }

        class TranslateCommand
        {
            public BasicLocalizationLanguage languageFrom;
            public BasicLocalizationLanguage languageTo;
            public BasicLocalizationTranslation translation;

            public string GetInfo() {
                return $"{translation.key} from {languageFrom.languageId} to {languageTo.languageId}";
            }

            public void Run()
            {
                var text = Translate(
                    languageFrom.locale.mainTranslationCode,
                    languageTo.locale.mainTranslationCode,
                    translation.GetTranslation(languageFrom)
                    );
                if (!string.IsNullOrEmpty(text))
                {
                    translation.PutTranslation(languageTo, text);
                }
            }

            static string Translate(
                string sourceLanguage,
                string targetLanguage,
                string sourceText)
            {
                var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={sourceLanguage}&tl={targetLanguage}&dt=t&q={UnityWebRequest.EscapeURL(sourceText)}";

                WebRequest request = WebRequest.Create(url);
                request.Credentials = CredentialCache.DefaultCredentials;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Console.WriteLine(response.StatusDescription);
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                Console.WriteLine(responseFromServer);
                reader.Close();
                dataStream.Close();
                response.Close();


                return JSONNode.Parse(responseFromServer)[0][0][0];
            }

        }
    }
}
