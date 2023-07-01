using System.Text;

namespace RV.Chess.PGN
{
    public static class PGNFileParser
    {
        const int MAX_BUFFER = 1024 * 1024;

        public static IEnumerable<PgnGame> Parse(string path)
        {
            var previousPart = string.Empty;
            var buffer = new byte[MAX_BUFFER];
            int bytesRead;

            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"PGN file not found at {path}");
            }

            using var fs = File.Open(path, FileMode.Open, FileAccess.Read);

            while ((bytesRead = fs.Read(buffer, 0, MAX_BUFFER)) != 0)
            {
                var sb = new StringBuilder(previousPart);
                sb.Append(Encoding.Default.GetString(buffer, 0, bytesRead));
                var chunk = sb.ToString();
                // split the chunk into game fragments
                var (games, remainder) = GetCompleteGameChunks(chunk);

                foreach (var gameText in games)
                {
                    yield return PgnGame.FromString(gameText);
                }

                previousPart = remainder;
            }
        }

        public static IEnumerable<PgnGame> ParseString(string data)
        {
            var previousPart = string.Empty;

            var start = 0;
            while (start < data.Length)
            {
                var sb = new StringBuilder(previousPart);
                var end = Math.Min(MAX_BUFFER, data.Length - start);
                sb.Append(data.AsSpan(start, end - start));
                var chunk = sb.ToString();
                // split the chunk into game fragments
                var (games, remainder) = GetCompleteGameChunks(chunk);

                foreach (var gameText in games)
                {
                    yield return PgnGame.FromString(gameText);
                }

                previousPart = remainder;
                start += MAX_BUFFER;
            }
        }

        private static (List<string>, string) GetCompleteGameChunks(string text)
        {
            var games = new List<string>();
            var sb = new StringBuilder();
            var position = 0;
            var insideTag = false;
            var insideComment = false;

            while (position < text.Length)
            {
                if (text[position] == '[' && !insideComment)
                {
                    insideTag = true;
                    sb.Append(text[position]);
                    position++;
                }
                else if (text[position] == ']' && insideTag)
                {
                    insideTag = false;
                    sb.Append(text[position]);
                    position++;
                }
                else if (text[position] == '{')
                {
                    insideComment = true;
                    sb.Append(text[position]);
                    position++;
                }
                else if (text[position] == '}' && insideComment)
                {
                    insideComment = false;
                    sb.Append(text[position]);
                    position++;
                }
                else if (text[position] == '*' && !insideComment && !insideTag)
                {
                    sb.Append(text[position]);
                    games.Add(sb.ToString());
                    sb.Clear();
                    insideComment = false;
                    insideTag = false;
                    position++;
                }
                else if (text[position] == '1' && !insideComment && !insideTag)
                {
                    // 1-0
                    if (position + 2 < text.Length
                        && text[position + 1] == '-'
                        && text[position + 2] == '0')
                    {
                        sb.Append("1-0");
                        games.Add(sb.ToString());
                        sb.Clear();
                        insideComment = false;
                        insideTag = false;
                        position += 3;
                    }
                    // 1/2-1/2
                    else if (position + 6 < text.Length
                        && text[position + 1] == '/'
                        && text[position + 2] == '2'
                        && text[position + 3] == '-'
                        && text[position + 4] == '1'
                        && text[position + 5] == '/'
                        && text[position + 6] == '2')
                    {
                        sb.Append("1/2-1/2");
                        games.Add(sb.ToString());
                        sb.Clear();
                        insideComment = false;
                        insideTag = false;
                        position += 7;
                    }
                    else
                    {
                        sb.Append(text[position]);
                        position++;
                    }
                }
                else if (text[position] == '0' && !insideComment && !insideTag)
                {
                    // 0-1
                    if (position + 2 < text.Length
                        && text[position + 1] == '-'
                        && text[position + 2] == '1')
                    {
                        sb.Append("0-1");
                        games.Add(sb.ToString());
                        sb.Clear();
                        insideComment = false;
                        insideTag = false;
                        position += 3;
                    }
                    else
                    {
                        sb.Append(text[position]);
                        position++;
                    }
                }
                else
                {
                    sb.Append(text[position]);
                    position++;
                }
            }

            var remainder = sb.ToString();
            return (games, remainder);
        }
    }
}
