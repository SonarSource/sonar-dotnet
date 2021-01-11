// Noncompliant (S1451)

namespace Framework48.Models
{
    public class Article
    {
        public string Title { get; }
        public string Text { get; }

        public Article(string title, string text)
        {
            Title = title;
            Text = text;
        }
    }
}
