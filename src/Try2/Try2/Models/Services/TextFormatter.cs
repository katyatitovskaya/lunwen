

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

            // strikethrough (зачеркнутый)
            text = Regex.Replace(text, @"~~(.*?)~~", "<del>$1</del>");

            // размер шрифта: [large]текст[/large] или [small]текст[/small] или [medium]текст[/medium]
            text = Regex.Replace(text, @"\[large\](.*?)\[/large\]", "<span style='font-size: 1.5em;'>$1</span>");
            text = Regex.Replace(text, @"\[medium\](.*?)\[/medium\]", "<span style='font-size: 1.2em;'>$1</span>");
            text = Regex.Replace(text, @"\[small\](.*?)\[/small\]", "<span style='font-size: 0.9em;'>$1</span>");

            // цвет шрифта: [red]текст[/red], [green]текст[/green], [black]текст[/black], [orange]текст[/orange], [pink]текст[/pink]
            text = Regex.Replace(text, @"\[red\](.*?)\[/red\]", "<span style='color: #8B0000;'>$1</span>");
            text = Regex.Replace(text, @"\[green\](.*?)\[/green\]", "<span style='color: #1B5E20;'>$1</span>");
            text = Regex.Replace(text, @"\[blue\](.*?)\[/blue\]", "<span style='color: #00008B;'>$1</span>");
            text = Regex.Replace(text, @"\[orange\](.*?)\[/orange\]", "<span style='color: #FF4500;'>$1</span>");
            text = Regex.Replace(text, @"\[pink\](.*?)\[/pink\]", "<span style='color: #FF1493;'>$1</span>");

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
