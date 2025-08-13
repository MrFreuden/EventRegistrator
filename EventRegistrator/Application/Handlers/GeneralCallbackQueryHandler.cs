using EventRegistrator.Application.DTOs;
using EventRegistrator.Application.Interfaces;
using EventRegistrator.Domain.Models;

namespace EventRegistrator.Application.Handlers
{
    public class GeneralCallbackQueryHandler : IHandler
    {
        public Task<List<Response>> HandleAsync(MessageDTO message)
        {
            if (message.Text.StartsWith("Cancel"))
            {
                var messages = new List<Response>();
                var user = _userRepository.GetUserByTargetChat(callbackQuery.Message.Chat.Id);
                var lastEvent = user.GetLastEvent();
                var messageIds = lastEvent.RemoveRegistrations(callbackQuery.From.Id);
                foreach (var messageId in messageIds)
                {
                    messages.Add(new Response { ChatId = lastEvent.TargetChatId, MessageToEditId = messageId, UnLike = true });
                }

                var eventDataPrivateUpdateMessage = new Response
                {
                    ChatId = user.PrivateChatId,
                    Text = TextFormatter.FormatRegistrationsInfo(lastEvent),
                    MessageToEditId = lastEvent.PrivateMessageId,
                };

                messages.Add(eventDataPrivateUpdateMessage);

                var text = TimeSlotParser.UpdateTemplateText(lastEvent.TemplateText, lastEvent.GetSlots());
                lastEvent.TemplateText = text;

                var firstCommentUpdateMessage = new Response
                {
                    ChatId = lastEvent.TargetChatId,
                    Text = lastEvent.TemplateText,
                    MessageToEditId = lastEvent.CommentMessageId,
                    ButtonData = (Constants.Cancel, Constants.Cancel)
                };
                messages.Add(firstCommentUpdateMessage);
                await ProcessMessagesAsync(messages);
            }


            if (callbackQuery.Data.StartsWith("EditTemplateText"))
            {
                _userRepository.GetUser(callbackQuery.From.Id).IsAsked = true;
                var askForTextMessage = new Response { ChatId = callbackQuery.From.Id, Text = Constants.AskForNewTemplate };
                await _messageSender.SendMessage(askForTextMessage);
            }
        }

        public bool CanHandle(MessageDTO message)
        {
            return (message.ChatId > 0 || message.Text.StartsWith("Cancel"));
        }
    }
}
