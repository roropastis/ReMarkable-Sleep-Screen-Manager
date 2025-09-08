# Internationalization Guide

This document explains how to use and extend the internationalization (i18n) system in the ReMarkable SleepScreen Manager.

## Overview

The application supports multiple languages through a resource-based localization system using .NET's built-in resource management.

## Supported Languages

- **English** (en-US) - Default
- **Français** (fr-FR) - French

## Adding New Languages

### 1. Create Resource Files

Create a new resource file for your language in the `Resources/` folder:

```
Resources/
├── Strings.resx              # Default (English)
├── Strings.fr-FR.resx        # French
└── Strings.de-DE.resx        # German (example)
```

### 2. Add Language to ResourceManager

Update `Localization/ResourceManager.cs`:

```csharp
public static string[] GetAvailableLanguages()
{
    return new[] { "en-US", "fr-FR", "de-DE" }; // Add your language
}
```

### 3. Update Language Selector

Add your language to `Localization/LanguageSelector.xaml`:

```xml
<ComboBoxItem Content="Deutsch" Tag="de-DE"/>
```

## Using Localized Strings

### In Code-Behind

```csharp
using RemarkableSleepScreenManager.Localization;

// Get a localized string
string title = ResourceManager.GetString("AppTitle");

// Using LocalizedString for automatic updates
var localizedTitle = new LocalizedString("AppTitle");
```

### In XAML

```xml
<Window xmlns:local="clr-namespace:RemarkableSleepScreenManager.Localization"
        Title="{Binding Source={x:Static local:ResourceManager}, Path=GetString('AppTitle')}">
```

### In ViewModels

```csharp
public class MyViewModel : INotifyPropertyChanged
{
    private readonly LocalizedString _title;
    
    public MyViewModel()
    {
        _title = new LocalizedString("AppTitle");
        ResourceManager.CultureChanged += OnCultureChanged;
    }
    
    public string Title => _title.Value;
    
    private void OnCultureChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(Title));
    }
}
```

## Adding New Strings

### 1. Add to Resource Files

Add the new key-value pair to all language resource files:

**Strings.resx (English):**
```xml
<data name="NewFeature" xml:space="preserve">
  <value>New Feature</value>
</data>
```

**Strings.fr-FR.resx (French):**
```xml
<data name="NewFeature" xml:space="preserve">
  <value>Nouvelle Fonctionnalité</value>
</data>
```

### 2. Use in Code

```csharp
string newFeatureText = ResourceManager.GetString("NewFeature");
```

## Best Practices

### 1. String Keys
- Use descriptive, hierarchical names: `Connection_TestButton`, `Gallery_DownloadButton`
- Use PascalCase for consistency
- Avoid abbreviations when possible

### 2. String Values
- Keep strings concise but clear
- Consider text expansion in different languages (German text is typically 30% longer)
- Use placeholders for dynamic content: `"Welcome {0}!"`

### 3. Pluralization
For complex pluralization rules, consider using libraries like `Humanizer` or implement custom logic:

```csharp
public static string GetPluralizedString(string key, int count, params object[] args)
{
    var culture = ResourceManager.CurrentCulture;
    var pluralKey = count == 1 ? $"{key}_Singular" : $"{key}_Plural";
    return ResourceManager.GetString(pluralKey, args);
}
```

## Testing Localization

### 1. Manual Testing
- Switch languages using the language selector
- Verify all UI elements update correctly
- Check for text overflow or layout issues

### 2. Automated Testing
```csharp
[Test]
public void AllLanguagesHaveAllKeys()
{
    var baseKeys = GetResourceKeys("en-US");
    var languages = ResourceManager.GetAvailableLanguages();
    
    foreach (var language in languages)
    {
        var languageKeys = GetResourceKeys(language);
        Assert.AreEqual(baseKeys.Count, languageKeys.Count, 
            $"Language {language} is missing keys");
    }
}
```

## Troubleshooting

### Common Issues

1. **Strings not updating**: Ensure you're using `LocalizedString` or handling `CultureChanged` events
2. **Missing translations**: Check that all resource files have the same keys
3. **Build errors**: Verify resource files are properly included in the project

### Debug Tips

```csharp
// Enable resource debugging
ResourceManager.Initialize();
var allKeys = ResourceManager.GetString("NonExistentKey"); // Returns "NonExistentKey"
```

## Future Enhancements

- **RTL Support**: Add support for right-to-left languages
- **Date/Time Formatting**: Localize date and time formats
- **Number Formatting**: Localize number and currency formats
- **Pluralization**: Implement proper pluralization rules
- **Context-Aware Translation**: Support for context-specific translations
