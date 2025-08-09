using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace EventRegistrator
{
    public class MessageSender
    {
        private readonly ITelegramBotClient _bot;

        public MessageSender(ITelegramBotClient bot)
        {
            _bot = bot;
        }

        public async Task<Message> SendTextTemplate(long chatId, string text)
        {
            var keyboard = new InlineKeyboardButton("Edit", "EditTemplateText");
            return await _bot.SendMessage(chatId, text, replyMarkup: keyboard);
        }

        public async Task<Message> SendGreetings(long chatId)
        {
            return await _bot.SendMessage(chatId, "Ласкаво просимо");
        }

        public async Task<Message> SendAskForText(long chatId)
        {
            return await _bot.SendMessage(chatId, "Введите новый текст");
        }

        public async Task<Message> SendFirstCommentAsAntwort(long chatId, int messageId, string templeText)
        {
            var keyboard = new InlineKeyboardButton("Cancel", "Cancel");
            var r = new ReplyParameters() { MessageId = messageId };
            return await _bot.SendMessage(chatId, templeText, replyParameters: r, replyMarkup: keyboard);
        }

        public async Task<Message> EditFirstComment(long chatId, int messageId, string text)
        {
            var keyboard = new InlineKeyboardButton("Cancel", "Cancel");
            return await _bot.EditMessageText(chatId, messageId, text, replyMarkup: keyboard);
        }

        public async Task<Message> EditEventData(long chatId, int messageId, Event lastEvent)
        {
            if (lastEvent == null)
                throw new ArgumentNullException(nameof(lastEvent));

            string registrationInfo = FormatRegistrationsInfo(lastEvent);

            return await _bot.EditMessageText(chatId, messageId, registrationInfo);
        }

        public async Task<Message> SendEventData(long chatId, Event lastEvent)
        {
            if (lastEvent == null)
                throw new ArgumentNullException(nameof(lastEvent));

            string registrationInfo = FormatRegistrationsInfo(lastEvent);

            return await _bot.SendMessage(chatId, registrationInfo);
        }

        private string FormatRegistrationsInfo(Event lastEvent)
        {
            var slots = lastEvent.GetSlots() ?? new List<TimeSlot>();

            if (slots.Count == 0)
                return "Нет доступных временных слотов";

            var sb = new StringBuilder();
            sb.AppendLine($"Запись на: {lastEvent.Title}");
            sb.AppendLine();

            slots = slots.OrderBy(s => s.Time).ToList();

            foreach (var slot in slots)
            {
                sb.AppendLine($"{slot.Time:HH:mm}");

                var registrations = GetRegistrationsFromTimeSlot(slot);

                if (registrations.Any())
                {
                    foreach (var registration in registrations)
                    {
                        sb.AppendLine(registration.Name);
                    }
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        private List<Registration> GetRegistrationsFromTimeSlot(TimeSlot slot)
        {
            var registrationsField = typeof(TimeSlot).GetField("_currentRegistrations",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (registrationsField != null)
            {
                var registrations = registrationsField.GetValue(slot) as List<Registration>;
                return registrations ?? new List<Registration>();
            }

            return new List<Registration>();
        }

        public async Task LikeMessage(long targetChatId, int id)
        {
            await _bot.SetMessageReaction(targetChatId, id, new[] { new ReactionTypeEmoji() { Emoji = "👍" } });
        }

        public async Task UnLikeMessage(long targetChatId, int id)
        {
            await _bot.SetMessageReaction(targetChatId, id, []);
        }
    }
}
