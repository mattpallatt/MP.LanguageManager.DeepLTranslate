# MP.LanguageManager.DeepLTranslate

## Description

This package extends the Optimizely CMS Language Manager to allow translations through the DeepL API.

## Configuration

To use the DeepL Translator to perform automation translations ensure it's selected as your selected option in the settings of the Language Manager.

![image](./img/languagemanager.png)

You will need a Authentication Key for the DeepL API - you can create one at https://www.deepl.com/pro-api

## AppSettings

![image](./img/appsettings.png)

A &lt;DeepL&gt;&lt;Formality&gt; Configuration element in appSettings can be used to control the formality of translations, with options Less, More, PreferLess, PreferMore, Default - a pro license is required, otherwise any setting other than Default will make the translation hang.

A &lt;DeepL&gt;&lt;English&gt; Configuration element in appSettings controls whether to translate to en-GB or en-US where "En" is your target language in Optimizely.

## Usage

- Install the Languages Gadget (https://nuget.optimizely.com/package/?id=EPiServer.Labs.LanguageManager)
- Set DeepL Web Translator as your Language Manager Translator
- Set your Authentication/Subscription key
- Use the Langauges Gadget to auto-translate all your content through DeepL!

![image](./img/gadget.png)

