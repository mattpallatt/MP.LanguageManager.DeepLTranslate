using DeepL;
using DeepL.Model;
using EPiServer.Labs.LanguageManager;
using EPiServer.Labs.LanguageManager.Business.Providers;
using EPiServer.Labs.LanguageManager.Configuration;
using EPiServer.Labs.LanguageManager.Models;
using EPiServer.ServiceLocation;
using Microsoft.Extensions.Options;
using MP.LanguageManager.DeepLTranslate;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace MP.Episerver.Labs.LanguageManager.DeepLTranslate
{
    public class DeepLTranslateProvider : IMachineTranslatorProvider
    {
        private readonly Injected<IOptions<DeepLOptions>> _options;

        public string DisplayName => "DeepL Web Translator";

        public DeepL.Formality DLFormality = DeepL.Formality.Default;
        public string EnglishType = "";
        public string authkey = "";

        public bool Initialize(ITranslatorProviderConfig config)
        {

            var languageManagerOptions = new LanguageManagerOptions();
            var languageManagerConfig = new LanguageManagerConfig(languageManagerOptions);
            authkey = languageManagerConfig.ActiveTranslatorProvider.SubscriptionKey;

            var options = _options.Service.Value;

            var dlfv = options.Formality;
            EnglishType = options.English;

            dlfv ??= "Default";
            EnglishType ??= "en-GB";

            // free API authorisation keys end in :fx...
            if (authkey.EndsWith(":fx"))
            {
                // ...set the formality to Default, as this is a Pro feature
                DLFormality = DeepL.Formality.Default;
            }
            else
            {
                DLFormality = dlfv.ToLower() switch
                {
                    "less" => DeepL.Formality.Less,
                    "more" => DeepL.Formality.More,
                    "preferless" => DeepL.Formality.PreferLess,
                    "prefermore" => DeepL.Formality.PreferMore,
                    _ => DeepL.Formality.Default,
                };
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

            // dealing with deprecated "en" target language code
            if (tlci.TwoLetterISOLanguageName.Contains("en") == true)
            {
                tl = EnglishType;
            }
            else { tl = tlci.TwoLetterISOLanguageName; }

            var translator = new DeepL.Translator(authkey);

            var translatedText = await translator.TranslateTextAsync(
                inputText,
                slci.TwoLetterISOLanguageName.ToUpper(),
                tl.ToUpper(),
                new TextTranslateOptions { Formality = DLFormality, TagHandling = "html"}
                );

            return translatedText;

        }

    }

}