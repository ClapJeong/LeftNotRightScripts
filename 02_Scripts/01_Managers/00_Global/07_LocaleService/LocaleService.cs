
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using Zenject;

public class LocaleService : IDisposable
{
  private const string LocaleKey = "Locale";
  private readonly LocalizeFonts fonts;

  private readonly List<LocalizeFontEvent> fontEvents = new();

  private Locale currentLocale;
  private TMP_FontAsset currentFont;

  [Inject]
  public LocaleService(TableContainer tableContainer)
  {
    this.fonts = tableContainer.LocalizationSO.LocalizeFonts;

    LocalizationSettings.SelectedLocaleChanged += UpdateFont;

    LoadLocale();

    currentLocale = LocalizationSettings.SelectedLocale;
    currentFont = GetLocaleFont(currentLocale);
  }

  public void Register(LocalizeFontEvent localizeFontEvent)
  {
    fontEvents.Add(localizeFontEvent);
    localizeFontEvent.UpdateFont(currentFont);
  }

  public void Unregister(LocalizeFontEvent localizeFontEvent)
    => fontEvents.Remove(localizeFontEvent);

  public void SetLocale(Locale locale)
    => LocalizationSettings.SelectedLocale = locale;

  public void SaveLocale()
  {
    var locale = LocalizationSettings.SelectedLocale;
    if (locale == null) return;

    PlayerPrefs.SetString(LocaleKey, locale.Identifier.Code);
    PlayerPrefs.Save();
  }

  public void LoadLocale()
  {    
    if (!PlayerPrefs.HasKey(LocaleKey))
      return;

    var code = PlayerPrefs.GetString(LocaleKey);
    var locale = LocalizationSettings.AvailableLocales.GetLocale(code);

    if (locale != null)
      LocalizationSettings.SelectedLocale = locale;
  }

  private void UpdateFont(Locale locale)
  {
    if (currentLocale == locale)
      return;

    currentLocale = locale;
    currentFont = GetLocaleFont(locale);
    foreach (var fontEvent in fontEvents)
      fontEvent.UpdateFont(currentFont);
  }

  private TMP_FontAsset GetLocaleFont(Locale locale)
    => fonts
    .FontSets
    .FirstOrDefault(set => set.Locale.Identifier == locale.Identifier)
    .FontAsset;

  public void Dispose()
  {
    LocalizationSettings.SelectedLocaleChanged -= UpdateFont;
  }
}
