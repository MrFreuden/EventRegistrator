using EventRegistrator.Application.DTOs;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace EventRegistrator.Infrastructure
{
    public class MessageSender
    {
        private readonly ITelegramBotClient _bot;
        private readonly ILogger<MessageSender> _logger;

        public MessageSender(ITelegramBotClient bot, ILogger<MessageSender> logger)
        {
            _bot = bot;
            _logger = logger;
        }

        public async Task<Message> SendMessage(Response message)
        {
            try
            {
                if (message.Like || message.UnLike)
                {
                    await SendReaction(message);
                    return new Message();
                }
                else if (message.ButtonData.HasValue)
                {
                    return await SendMessageWithButton(message, new InlineKeyboardButton(message.ButtonData.Value.Item1, message.ButtonData.Value.Item2));
                }
                else if (message.MessageToEditId.HasValue)
                {
                    return await EditMessageText(message, message.MessageToEditId.Value);
                }
                else if (message.MessageToReplyId.HasValue)
                {
                    return await ReplyToMessage(message, message.MessageToReplyId.Value);
                }
                else
                {
                    return await _bot.SendMessage(message.ChatId, message.Text);
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to send message to chat {ChatId}", message.ChatId);
                throw;
            }
        }

        private async Task<Message> SendMessageWithButton(Response message, ReplyMarkup markup)
        {
            if (message.MessageToReplyId.HasValue)
            {
                return await ReplyToMessageWithButton(message, markup, message.MessageToReplyId.Value);
            }
            if (message.MessageToEditId.HasValue)
            {
                return await EditMessageText(message, message.MessageToEditId.Value, new InlineKeyboardButton(message.ButtonData.Value.Item1, message.ButtonData.Value.Item2));
            }
            return await _bot.SendMessage(message.ChatId, message.Text, replyMarkup: markup);
        }

        private async Task<Message> EditMessageText(Response message, int messageToEditId)
        {
            return await _bot.EditMessageText(message.ChatId, messageToEditId, message.Text);
        }

        private async Task<Message> EditMessageText(Response message, int messageToEditId, InlineKeyboardMarkup markup)
        {
            return await _bot.EditMessageText(message.ChatId, messageToEditId, message.Text, replyMarkup: markup);
        }

        private async Task<Message> ReplyToMessage(Response message, int messageToReplyId)
        {
            var replyParams = new ReplyParameters() { MessageId = messageToReplyId };
            return await _bot.SendMessage(message.ChatId, message.Text, replyParameters: replyParams);
        }

        private async Task<Message> ReplyToMessageWithButton(Response message, ReplyMarkup markup, int messageToReplyId)
        {
            var replyParams = new ReplyParameters() { MessageId = messageToReplyId };
            return await _bot.SendMessage(message.ChatId, message.Text, replyParameters: replyParams, replyMarkup: markup);
        }

        private async Task SendReaction(Response message)
        {
            if (message.Like)
            {
                await LikeMessage(message.ChatId, message.MessageToEditId.Value);
            }
            else if (true)
            {
                await UnLikeMessage(message.ChatId, message.MessageToEditId.Value);
            }
        }

        private async Task LikeMessage(long targetChatId, int messageId)
        {
            await _bot.SetMessageReaction(targetChatId, messageId, new[] { new ReactionTypeEmoji() { Emoji = "👍" } });
        }

        private async Task UnLikeMessage(long targetChatId, int messageId)
        {
            await _bot.SetMessageReaction(targetChatId, messageId, []);
        }
    }
}
