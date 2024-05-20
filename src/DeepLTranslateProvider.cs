using System;
using EPiServer.Labs.LanguageManager;
using EPiServer.Labs.LanguageManager.Business.Providers;
using EPiServer.Labs.LanguageManager.Configuration;
using EPiServer.Labs.LanguageManager.Models;
using System.Threading.Tasks;
using DeepL.Model;
using System.Globalization;
using DeepL;
using Microsoft.Extensions.Configuration;

namespace MP.Episerver.Labs.LanguageManager.DeepLTranslate
{
    public class DeepLTranslateProvider : IMachineTranslatorProvider
    {

        public string DisplayName => "DeepL Web Translator";

        public DeepL.Formality DLFormality = DeepL.Formality.Default;
        public string EnglishType = "";
        public string authkey = "";

        public bool Initialize(ITranslatorProviderConfig config)
        {
            
            var languageManagerOptions = new LanguageManagerOptions();
            var languageManagerConfig = new LanguageManagerConfig(languageManagerOptions);
            authkey = languageManagerConfig.ActiveTranslatorProvider.SubscriptionKey;

            var MyConfig = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var dlfv = MyConfig.GetValue<string>("DeepL:Formality");    
            EnglishType = MyConfig.GetValue<string>("DeepL:English");
            if (EnglishType == "") { EnglishType = "en-GB"; }

            switch (dlfv)
            {
                case "Less": DLFormality = DeepL.Formality.Less; break;
                case "More": DLFormality = DeepL.Formality.More; break;
                case "PreferLess": DLFormality = DeepL.Formality.PreferLess; break;
                case "PreferMore": DLFormality = DeepL.Formality.PreferMore; break;
                default:
                    DLFormality = DeepL.Formality.Default; break;

            }

            return true;
        }

        public TranslateTextResult Translate(string inputText, string sourceLanguage, string targetLanguage)
        {
            var translateTextResult = new TranslateTextResult
            {
                IsSuccess = true,
                Text = ""
            };

            if (string.IsNullOrWhiteSpace(inputText))
            {
                return translateTextResult;
            }

            try
            {

                var n = DoTranslate(inputText, sourceLanguage, targetLanguage);
                n.Wait();

                translateTextResult.Text = n.Result.ToString();
                translateTextResult.IsSuccess = true;

            }
            catch (Exception ex)
            {
                translateTextResult.IsSuccess = false;
                translateTextResult.Text = "An unexpected error occurred: " + ex.Message + ex.InnerException;
            }

            return translateTextResult;
        }

        public async Task<TextResult> DoTranslate(string inputText, string sourceLanguage, string targetLanguage)
        {

            var slci = new CultureInfo(sourceLanguage);
            var tlci = new CultureInfo(targetLanguage);
            string tl = tlci.TwoLetterISOLanguageName.ToString();
            
            // dealing with deprecated "en" language code
            if (tlci.TwoLetterISOLanguageName.Contains("en") == true)
            {
                tl = EnglishType;
            }
            else { tl = tlci.TwoLetterISOLanguageName; }

            var translator = new DeepL.Translator(authkey);
            var translatedText = await translator.TranslateTextAsync(
                inputText,
                slci.TwoLetterISOLanguageName,
                tl,
                new TextTranslateOptions { Formality = DLFormality, PreserveFormatting = true, SentenceSplittingMode = SentenceSplittingMode.Off }
                );

            return translatedText;

        }

    }

}