using EPiServer.Core;
using EPiServer.Labs.LanguageManager.Business;
using EPiServer.Labs.LanguageManager.Service;
using EPiServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.ServiceLocation;
using EPiServer.Labs.LanguageManager.Business.Providers;
using EPiServer.Labs.LanguageManager.Models;
using EPiServer.Labs.LanguageManager.ViewModels;
using System.Globalization;
using Microsoft.Extensions.Options;

namespace MP.LanguageManager.DeepLTranslate
{
    public class CustomLanguageBranchManager : ILanguageBranchManager
    {
        private Injected<IContentLoader> _contentLoader;
        private readonly Injected<IOptions<DeepLOptions>> _options;

        private ILanguageBranchManager _defaultLanguageBranchManager;

        public CustomLanguageBranchManager(ILanguageBranchManager defaultLanguageBranchManager)
        {
            _defaultLanguageBranchManager = defaultLanguageBranchManager;
        }

        public async Task<TranslationAsyncResult> CopyDataFromMasterBranch(ContentReference contentReference,
            string fromLanguageID,
            string toLanguageID,
            Func<object, Task<object>> transformOnCopyingValue,
            bool autoPublish = false)
        {

            var options = _options.Service.Value;
            string ignoreExistingPages = (options.ignoreExistingPages!= null) ? options.ignoreExistingPages : "0";

            //Get the content being translated
            var masterContent = _contentLoader.Service.Get<ContentData>(contentReference.ToReferenceWithoutVersion(), new LanguageSelector(fromLanguageID));
            var toLanguage = new CultureInfo(toLanguageID);
            //Check whether the content already has a version in the language we're looking to translate to
            if (ignoreExistingPages == "1" && (masterContent is ILocalizable localizableContent && localizableContent.ExistingLanguages.Any(x => x.Equals(toLanguage))))
            {
                //The content already exists in the target language so don't recreate it
                //Pretend the creation's been a success so as to continue the process for other content
                return new TranslationAsyncResult { ContentLink = contentReference, Result = true };
            }

            return await _defaultLanguageBranchManager.CopyDataFromMasterBranch(contentReference, fromLanguageID, toLanguageID, transformOnCopyingValue, autoPublish);
        }

        //Everything below here just calls the matching method on the default implementation...
        public async Task<TranslationAsyncResult> TranslateAndCopyDataFromMasterBranch(
            ContentReference contentReference,
            string fromLanguageID,
            string fromTwoLetterLanguageName,
            string toLanguageID,
            string toTwoLetterLanguageName,
            bool autoPublish = false)
        {
            return await _defaultLanguageBranchManager.TranslateAndCopyDataFromMasterBranch(contentReference, fromLanguageID, fromTwoLetterLanguageName, toLanguageID, toTwoLetterLanguageName, autoPublish);
        }

        bool ILanguageBranchManager.CreateLanguageBranch(ContentReference contentLink, string languageID, out ContentReference createdContentLink)
        {
            return _defaultLanguageBranchManager.CreateLanguageBranch(contentLink, languageID, out createdContentLink);
        }

        bool ILanguageBranchManager.DeleteLanguageBranch(ContentReference contentLink, string languageID)
        {
            return _defaultLanguageBranchManager.DeleteLanguageBranch(contentLink, languageID);
        }

        bool ILanguageBranchManager.GetMasterLanguageName(ContentReference contentReference, out string languageID, out string twoLetterLanguageName)
        {
            return _defaultLanguageBranchManager.GetMasterLanguageName(contentReference, out languageID, out twoLetterLanguageName);
        }

        IContent ILanguageBranchManager.GetPublishedOrCommonDraftVersion(ContentReference contentReference, string languageID)
        {
            return _defaultLanguageBranchManager.GetPublishedOrCommonDraftVersion(contentReference, languageID);
        }

        IEnumerable<LanguageInfo> ILanguageBranchManager.GetSystemEnabledLanguages(ContentReference contentReference)
        {
            return _defaultLanguageBranchManager.GetSystemEnabledLanguages(contentReference);
        }

        bool ILanguageBranchManager.ToggleLanguageBranchActivation(ContentReference contentLink, string languageID)
        {
            return _defaultLanguageBranchManager.ToggleLanguageBranchActivation(contentLink, languageID);
        }

        Task<object> ILanguageBranchManager.TranslateStringList(object inputStringList, IMachineTranslatorProvider translator, string fromLanguageID, string toLanguageID)
        {
            return _defaultLanguageBranchManager.TranslateStringList(inputStringList, translator, fromLanguageID, toLanguageID);
        }
    }
}
