using EventRegistrator.Domain.Models;
using Telegram.Bot.Types;

namespace EventRegistrator.Infrastructure
{
    public static class UpdateMapper
    {
        public static MessageDTO Map(Message message)
        {
            var messageDto = new MessageDTO
            {
                ChatId = message.Chat.Id,
                Id = message.MessageId,
                Text = message.Text,
                UserId = message.From?.Id,
                ReplyToMessageId = message.ReplyToMessage?.Id,
                Created = message.Date,
            };

            if (messageDto.ReplyToMessageId != null) 
                messageDto.IsReply = true;

            if (message.ForwardFromChat != null)
            {
                messageDto.ForwardFromChat = new ChatDTO
                {
                    Id = message.ForwardFromChat.Id,
                    Title = message.ForwardFromChat.Title,
                    Type = message.ForwardFromChat.Type.ToString()
                };
            }

            if (message.ReplyToMessage != null)
            {
                messageDto.ReplyToMessage = Map(message.ReplyToMessage);
            }

            return messageDto;
        }

        public static List<MessageDTO> Map(List<Message> messages)
        {
            var result = new List<MessageDTO>();
            foreach (var message in messages)
            {
                result.Add(Map(message));
            }
            return result;
        }

        public static MessageDTO Map(CallbackQuery callbackQuery)
        {
            var message = Map(callbackQuery.Message);
            var messageDto = new MessageDTO
            {
                ChatId = message.ChatId,
                Id = message.Id,
                Text = callbackQuery.Data,
                UserId = message.UserId,
                ReplyToMessageId = message.ReplyToMessageId,
                Created = message.Created
            };

            return messageDto;
        }
    }
}
