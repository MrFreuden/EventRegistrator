using EventRegistrator.Application.DTOs;
using EventRegistrator.Domain.Models;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace EventRegistrator.Infrastructure
{
    public class MessageSender
    {
        private readonly ITelegramBotClient _bot;

        public MessageSender(ITelegramBotClient bot)
        {
            _bot = bot;
        }

        public async Task<Message> SendMessage(Response message)
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

        public async Task SendReaction(Response message)
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
