using System;
using System.Collections.Generic;
using System.Text;

using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using Cysharp.Threading.Tasks;

public static class LocaleAutoSetter
{
  public static async UniTask SetLocaleBySystemLanguageAsync()
  {
    // Localization 초기화 대기 (중요)
    await LocalizationSettings.InitializationOperation;

    var systemLanguage = Application.systemLanguage;
    var locales = LocalizationSettings.AvailableLocales.Locales;

    Locale targetLocale = null;

    foreach (var locale in locales)
    {
      if (IsMatch(locale, systemLanguage))
      {
        targetLocale = locale;
        break;
      }
    }

    // fallback
    if (targetLocale == null)
    {
      Debug.LogWarning($"No locale matched for {systemLanguage}, fallback to English.");
      targetLocale = LocalizationSettings.AvailableLocales.GetLocale("en");
    }

    LocalizationSettings.SelectedLocale = targetLocale;
  }

  private static bool IsMatch(Locale locale, SystemLanguage systemLanguage)
  {
    switch (systemLanguage)
    {
      case SystemLanguage.Korean:
        return locale.Identifier.Code.StartsWith("ko");

      case SystemLanguage.Japanese:
        return locale.Identifier.Code.StartsWith("ja");

      case SystemLanguage.ChineseSimplified:
        return locale.Identifier.Code.StartsWith("zh-Hans");

      case SystemLanguage.ChineseTraditional:
        return locale.Identifier.Code.StartsWith("zh-Hant");

      case SystemLanguage.English:
        return locale.Identifier.Code.StartsWith("en");

      case SystemLanguage.French:
        return locale.Identifier.Code.StartsWith("fr");

      case SystemLanguage.German:
        return locale.Identifier.Code.StartsWith("de");

      case SystemLanguage.Spanish:
        return locale.Identifier.Code.StartsWith("es");

      default:
        return false;
    }
  }
}
