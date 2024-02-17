using System.Linq;

namespace RV.Chess.PGN.Utils
{
    internal static class CommentMerger
    {
        internal static List<string> MergeInto(List<string> comments, string newComment)
        {
            var merged = new List<string>(comments);
            var useComment = true;

            for (var i = 0; i < comments.Count; i++)
            {
                if (string.Equals(newComment.ToLowerInvariant(), comments[i].ToLowerInvariant(), StringComparison.Ordinal))
                {
                    useComment = false;
                    break;
                }

                var wordsNew = HashWords(newComment);
                var wordsExisting = HashWords(comments[i]);
                var matchingCount = 0;

                // check if shorter comment is a part of the longer one
                if (wordsNew.Count < wordsExisting.Count)
                {
                    matchingCount += wordsNew.Count(wordHash => wordsExisting.Contains(wordHash));

                    if (matchingCount >= wordsNew.Count * 0.8)
                    {
                        // at least 90% of words in the new comment are the same as in the old one,
                        // but the old one is longer, so we just keep it
                        useComment = false;
                        break;
                    }
                }
                else
                {
                    matchingCount += wordsExisting.Count(wordHash => wordsNew.Contains(wordHash));

                    if (matchingCount >= wordsExisting.Count * 0.8)
                    {
                        // at least 90% of words in the existing comment are the same as in the new one,
                        // but the new one is longer, so we replace it
                        useComment = false;
                        merged[i] = newComment;
                        break;
                    }
                }
            }

            if (useComment)
            {
                merged.Add(newComment);
            }

            return merged;
        }

        private static HashSet<int> HashWords(ReadOnlySpan<char> text)
        {
            var result = new HashSet<int>();
            var wordHash = 0;
            var shift = 0;

            for (var i = 0; i < text.Length; i++)
            {
                if (char.IsWhiteSpace(text[i]))
                {
                    result.Add(wordHash);
                    wordHash = 0;
                    shift = 0;
                }
                else
                {
                    wordHash += text[i] << shift;
                    shift = (shift + 8) % 24;
                }
            }

            result.Add(wordHash);

            return result;
        }
    }
}
