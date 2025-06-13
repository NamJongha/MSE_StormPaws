using System.Collections.Generic;

public static class LanguageTranslate
{
    public enum Language { Korean, English }

    public static Language CurrentLanguage = Language.English;

    private static readonly Dictionary<string, (string Korean, string English)> translations = new()
    {
        { "Frog", ("개구리", "Frog") },
        { "Ostrich", ("타조", "Ostrich") },
        { "Elephant", ("아프리카 코끼리", "Elephant") },
        { "Hamster", ("햄스터", "Hamster") },
        { "Geko", ("목도리 도마뱀", "Geko") },
        { "Lion", ("사자", "Lion") },
        { "Whale", ("흰수염 고래", "Whale") },
        { "Monkey", ("원숭이", "Monkey") },
        { "Horse", ("말", "Horse") },
        { "Tiger", ("호랑이", "Tiger") },
        { "Polarbear", ("북극곰", "Polarbear") },
        { "Octopus", ("문어", "Octopus") },
        { "Sloth", ("나무늘보", "Sloth") },
        { "Hippo", ("하마", "Hippo") },
        { "Giraffe", ("기린", "Giraffe") }
    };

    private static readonly Dictionary<string, string> typeMap = new()
    {
        { "클리어", "Clear" },
        { "클라우드", "Clouds" },
        { "레인", "Rain" },
        { "스노우", "Snow" },
        { "포그", "Fog" },
        { "미스트", "Mist" },
        { "썬더스톰", "Thunderstorm" },
        { "황사", "Sand" },
        { "토네이도", "Tornado" },
        { "돌풍", "Gust" }
    };

    public static string GetDisplayName(string inputName)
    {
        foreach (var pair in translations)
        {
            if (CurrentLanguage == Language.Korean && inputName == pair.Key)
            {
                return pair.Value.Korean;
            }

            if (CurrentLanguage == Language.English && inputName == pair.Value.Korean)
            {
                return pair.Value.English;
            }
        }

        return inputName; // fallback
    }

    public static string GetResourceKey(string inputName)
    {
        foreach (var pair in translations)
        {
            if (inputName == pair.Value.Korean || inputName == pair.Key)
            {
                return pair.Key;
            }
        }

        return inputName; // fallback
    }

    public static string GetCardType(string koreanType)
    {
        if (CurrentLanguage == Language.Korean)
        {
            return koreanType;
        }

        return typeMap.TryGetValue(koreanType, out var eng) ? eng : koreanType;
    }
}