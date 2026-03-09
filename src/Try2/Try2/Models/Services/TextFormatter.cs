

using System.Text.RegularExpressions;

namespace Try2.Models.Services
{
    public static class TextFormatter
    {
        public static string FormatPostText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            // HTML encode для безопасности
            text = System.Net.WebUtility.HtmlEncode(text);

            // bold
            text = Regex.Replace(text, @"\*\*(.*?)\*\*", "<b>$1</b>");

            // italics 
            text = Regex.Replace(text, @"\*(.*?)\*", "<i>$1</i>");

            // UL список
            text = Regex.Replace(
                text,
                @"(?:^- .+(?:\n|$))+",
                match =>
                {
                    var items = match.Value
                        .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                        .Select(i => "<li>" + i.Substring(2) + "</li>");

                    return "<ul>" + string.Join("", items) + "</ul>";
                },
                RegexOptions.Multiline
            );

            // OL список
            text = Regex.Replace(
                text,
                @"(?:^\d+\. .+(?:\n|$))+",
                match =>
                {
                    var items = match.Value
                        .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                        .Select(i =>
                        {
                            var index = i.IndexOf(". ");
                            return "<li>" + i.Substring(index + 2) + "</li>";
                        });

                    return "<ol>" + string.Join("", items) + "</ol>";
                },
                RegexOptions.Multiline
            );

            // сохраняем переносы строк
            text = text.Replace("\n", "<br>");

            // сохраняем отступы
            text = Regex.Replace(text, @"\s{2,}", m =>
                string.Concat(Enumerable.Repeat("&nbsp;", m.Value.Length)));

            return text;
        }
    }
}
