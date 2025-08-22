using EventRegistrator.Application.DTOs;
using EventRegistrator.Domain.Models;
using EventRegistrator.Infrastructure.Utils;

namespace EventRegistrator.Application.Services
{
    public class ResponseManager
    {
        public List<Response> PrepareNotificationMessages(UserAdmin user, Event lastEvent)
        {
            var eventDataPrivateMessage = CreatePrivateEventSummaryMessage(user.PrivateChatId, lastEvent);

            var firstCommentUpdateMessage = new Response
            {
                ChatId = lastEvent.TargetChatId,
                Text = lastEvent.TemplateText,
                MessageToEditId = lastEvent.CommentMessageId,
                ButtonData = new(Constants.Cancel, Constants.Cancel),
            };

            return [eventDataPrivateMessage, firstCommentUpdateMessage];
        }

        public Response PrepareCommentUpdateMessage(Event lastEvent)
        {
            var firstCommentUpdateMessage = new Response
            {
                ChatId = lastEvent.TargetChatId,
                Text = lastEvent.TemplateText,
                MessageToEditId = lastEvent.CommentMessageId,
                ButtonData = new(Constants.Cancel, Constants.Cancel),
            };
            return firstCommentUpdateMessage;
        }

        public Response CreatePrivateEventSummaryMessage(long chatId, Event lastEvent)
        {
            if (lastEvent.PrivateMessageId == default)
            {
                var eventDataMessage = new Response
                {
                    ChatId = chatId,
                    Text = TextFormatter.FormatRegistrationsInfo(lastEvent),
                    SaveMessageIdCallback = id => { lastEvent.PrivateMessageId = id; }
                };
                return eventDataMessage;
            }
            else
            {
                var eventDataPrivateUpdateMessage = new Response
                {
                    ChatId = chatId,
                    Text = TextFormatter.FormatRegistrationsInfo(lastEvent),
                    MessageToEditId = lastEvent.PrivateMessageId,
                };
                return eventDataPrivateUpdateMessage;
            }
        }

        public Response CreateLikeMessage(long chatId, int messageId)
        {
            return new Response
            {
                ChatId = chatId,
                MessageToEditId = messageId,
                Like = true,
            };
        }

        public Response CreateUnlikeMessage(long chatId, int messageId)
        {
            return new Response
            {
                ChatId = chatId,
                MessageToEditId = messageId,
                UnLike = true,
            };
        }
    }
}
