using UnityEngine;
using UnityEngine.UI;
using m039.BasicLocalization;
using System;
using System.Collections.Generic;

public class BasicLocalizationHelloSample : MonoBehaviour
{
    #region Inspector

    public Dropdown languagesDropdown;

    #endregion

    void Start()
    {
        InitControls();
    }

    private void OnDestroy()
    {
        languagesDropdown?.onValueChanged.RemoveListener(OnDropdownValueChanged);
    }

    void InitControls()
    {
        if (languagesDropdown == null)
            return;

        var currentLanguage = BasicLocalization.GetCurrentLanguage();
        var availableLanguages = BasicLocalization.GetAvailableLanguages();
        var currentLanguageIndex = availableLanguages.IndexOf(currentLanguage);

        var options = new List<Dropdown.OptionData>();
        foreach (var language in availableLanguages)
        {
            options.Add(new Dropdown.OptionData
            {
                text = language.languageId
            });
        }

        languagesDropdown.options = options;
        languagesDropdown.value = currentLanguageIndex;
        languagesDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    void OnDropdownValueChanged(int index)
    {
        BasicLocalization.SelectLanguageAt(index);
    }
}
