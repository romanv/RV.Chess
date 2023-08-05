namespace RV.Chess.CBReader.Entities
{
    public record GuidingTextSection
    {
        public GuidingTextSection(TextLanguage language, string title, string text)
        {
            Language = language;
            Title = title;
            Text = text;
        }

        public TextLanguage Language { get; private set; }

        public string Title { get; private set; }

        public string Text { get; private set; }
    }
}
