using Newtonsoft.Json;

namespace EventRegistrator.Domain.Models
{
    public class Hashtag : IPagiable
    {
        public string HashtagName { get; }
        public string TemplateText { get; private set; }

        public string Name => HashtagName;

        public string Callback => HashtagName;

        private const string defaultTemplate = "10:00 - 10 вільних місць\r\n10:15 - 10 вільних місць\r\n10:30 - 10 вільних місць";

        public Hashtag(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Имя хэштега не может быть пустым", nameof(name));

            HashtagName = name;
            TemplateText = defaultTemplate;
        }

        [JsonConstructor]
        private Hashtag(string hashtagName, string templateText)
        {
            HashtagName = hashtagName;
            TemplateText = templateText;
        }

        public void EditTemplateText(string text)
        {
            if (IsTemplateValid(text))
            {
                TemplateText = text;
            }
        }

        private bool IsTemplateValid(string text)
        {
            return true;
        }
    }
}
