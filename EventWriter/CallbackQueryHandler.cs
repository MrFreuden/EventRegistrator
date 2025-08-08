using Telegram.Bot.Types;

namespace EventWriter
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
        }
    }
}