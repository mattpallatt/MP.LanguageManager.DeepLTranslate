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
using System.Linq;
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
        public string AutoGlossary = "";
        public string GlossaryList = "";

        public bool Initialize(ITranslatorProviderConfig config)
        {

            var languageManagerOptions = new LanguageManagerOptions();
            var languageManagerConfig = new LanguageManagerConfig(languageManagerOptions);
            authkey = languageManagerConfig.ActiveTranslatorProvider.SubscriptionKey;

            var options = _options.Service.Value;
            var dlfv = "default";

            try
            {
                dlfv = (options.Formality.ToLower() == "default" || options.Formality.ToLower() == "more" || options.Formality.ToLower() == "less" || options.Formality.ToLower() == "preferless" || options.Formality.ToLower() == "prefermore") ? options.Formality.ToLower() : dlfv;
                EnglishType = (options.English.ToLower() == "en-gb" || options.English.ToLower() == "en-us") ? options.English.ToLower() : "en-gb";
                AutoGlossary = (options.AutoGlossary == "0" || options.AutoGlossary == "1") ? options.AutoGlossary : AutoGlossary;
                GlossaryList = options.GlossaryList;
            }
            catch { }

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
            var sourceLanguageCode = FetchDeeplSourceLanguageCode(sourceLanguage);
            var targetLanguageCode = FetchDeeplTargetLanguageCode(targetLanguage);

            string? glossaryId = string.Empty;

            var translator = new Translator(authkey);

            if ((AutoGlossary == "1") || (GlossaryList.Contains($"[{sourceLanguageCode}>{targetLanguageCode}]")))
            {
                GlossaryInfo[] glossaryInfos = await translator.ListGlossariesAsync();
                glossaryId = glossaryInfos.FirstOrDefault(item =>
                    item.SourceLanguageCode.Equals(sourceLanguageCode, StringComparison.OrdinalIgnoreCase) &&
                    item.TargetLanguageCode.Equals(targetLanguageCode, StringComparison.OrdinalIgnoreCase))?.GlossaryId;
            }

            try
            {
                var translatedText = await translator.TranslateTextAsync(
                    inputText,
                    sourceLanguageCode,
                    targetLanguageCode,
                    new TextTranslateOptions { Formality = DLFormality, TagHandling = "html", GlossaryId = glossaryId }
                );

                return translatedText;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private string FetchDeeplSourceLanguageCode(string languageCode)
        {
            var culture = new CultureInfo(languageCode);
            string targetCode = culture.TwoLetterISOLanguageName;

            // DeepL uses ISO 639-2 code for Norwegian language
            if (targetCode.Equals("no", StringComparison.OrdinalIgnoreCase))
            {
                targetCode = "NB";
            }

            return targetCode.ToUpper();
        }

        private string FetchDeeplTargetLanguageCode(string languageCode)
        {
            var culture = new CultureInfo(languageCode);
            string targetCode = culture.TwoLetterISOLanguageName;

            // DeepL uses ISO 639-2 code for Norwegian language
            if (targetCode.Equals("no", StringComparison.OrdinalIgnoreCase))
            {
                targetCode = "NB";
            }
            // deal with PT, ZH (xx-xx) language codes
            else if (targetCode.Equals("pt", StringComparison.OrdinalIgnoreCase) ||
                     targetCode.Equals("zh", StringComparison.OrdinalIgnoreCase))
            {
                targetCode = languageCode;
            }
            // dealing with depreciated "en" target language code using default EN code
            else if (targetCode.Equals("en", StringComparison.OrdinalIgnoreCase))
            {
                targetCode = EnglishType;
            }

            return targetCode.ToUpper();
        }
    }

}