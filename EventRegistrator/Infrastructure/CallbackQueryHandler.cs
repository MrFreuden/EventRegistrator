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
                await _messageSender.SendAskForText(callbackQuery.From.Id);
            }
            else if (callbackQuery.Data.StartsWith("Cancel"))
            {
                var user = _userRepository.GetUserByTargetChat(callbackQuery.Message.Chat.Id);
                var lastEvent = user.GetLastEvent();
                var messageIds = lastEvent.RemoveRegistrations(callbackQuery.From.Id);
                foreach (var messageId in messageIds)
                {
                    await _messageSender.UnLikeMessage(user.TargetChatId, messageId);
                }
                
                await _messageSender.EditEventData(user.PrivateChatId, lastEvent.PrivateMessageId, lastEvent);

                var text = TimeSlotParser.UpdateTemplateText(user.TempleText, lastEvent.GetSlots());
                lastEvent.TemplateText = text;
                await _messageSender.EditFirstComment(user.TargetChatId, lastEvent.CommentMessageId, lastEvent.TemplateText);
            }
        }
    }
}