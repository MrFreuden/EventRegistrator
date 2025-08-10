using EventRegistrator.Application;
using Telegram.Bot.Types;

namespace EventRegistrator.Infrastructure
{
    public class CallbackQueryHandler
    {
        UserRepository _userRepository;
        MessageSender _messageSender;

        public CallbackQueryHandler(UserRepository userRepository, MessageSender messageSender)
        {
            _userRepository = userRepository;
            _messageSender = messageSender;
        }

        public async Task ProcessCallbackQuery(CallbackQuery callbackQuery)
        {
            if (callbackQuery.Data.StartsWith("EditTemplateText"))
            {
                _userRepository.GetUser(callbackQuery.From.Id).IsAsked = true;
                var askForTextMessage = new MessageDTO { ChatId = callbackQuery.From.Id, Text = Constants.AskForNewTemplate };
                await _messageSender.SendMessage(askForTextMessage);
            }
            else if (callbackQuery.Data.StartsWith("Cancel"))
            {
                var messages = new List<MessageDTO>();
                var user = _userRepository.GetUserByTargetChat(callbackQuery.Message.Chat.Id);
                var lastEvent = user.GetLastEvent();
                var messageIds = lastEvent.RemoveRegistrations(callbackQuery.From.Id);
                foreach (var messageId in messageIds)
                {
                    messages.Add(new MessageDTO { ChatId = user.TargetChatId, MessageToEditId = messageId, UnLike = true });
                }

                var eventDataPrivateUpdateMessage = new MessageDTO
                {
                    ChatId = user.PrivateChatId,
                    Text = EventFormatter.FormatRegistrationsInfo(lastEvent),
                    MessageToEditId = lastEvent.PrivateMessageId,
                };

                messages.Add(eventDataPrivateUpdateMessage);

                var text = TimeSlotParser.UpdateTemplateText(user.TempleText, lastEvent.GetSlots());
                lastEvent.TemplateText = text;

                var firstCommentUpdateMessage = new MessageDTO
                {
                    ChatId = user.TargetChatId,
                    Text = lastEvent.TemplateText,
                    MessageToEditId = lastEvent.CommentMessageId,
                };
                messages.Add(firstCommentUpdateMessage);
                await ProcessMessagesAsync(messages);
            }
        }

        private async Task ProcessMessagesAsync(List<MessageDTO> messages)
        {
            foreach (var message in messages)
            {
                var sentMessage = await _messageSender.SendMessage(message);

                message.SaveMessageIdCallback?.Invoke(sentMessage.MessageId);
            }
        }
    }
}