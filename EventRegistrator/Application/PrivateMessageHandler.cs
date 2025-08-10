using EventRegistrator.Domain;
using EventRegistrator.Infrastructure;
using Telegram.Bot.Types;

namespace EventRegistrator.Application
{
    public class PrivateMessageHandler
    {
        private readonly IUserRepository _userRepository;

        public PrivateMessageHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<MessageDTO> Handle(Message message)
        {
            if (IsCommand(message))
            {
                return await ProcessPrivateMessageCommand(message);
            }
            else if (IsUserAsked(message))
            {
                return ProcessEditTemplateText(message);
            }
            return new MessageDTO { ChatId = message.Chat.Id, Text = Constants.Error };
        }

        private async Task<MessageDTO> ProcessPrivateMessageCommand(Message message)
        {
            switch (message.Text)
            {
                case "/start":
                    _userRepository.AddUser(message.Chat.Id);
                    return new MessageDTO { ChatId = message.Chat.Id, Text = Constants.Greetings };
                case "/settings":
                    var text = _userRepository.GetUser(message.Chat.Id).TempleText;
                    return new MessageDTO { ChatId = message.Chat.Id, Text = text };
                case "/admin":
                    var text2 = EventFormatter.GetAllUsersInfo(_userRepository as UserRepository);
                    return new MessageDTO { ChatId = message.Chat.Id, Text = text2 };
                default:
                    return new MessageDTO { ChatId = message.Chat.Id, Text = Constants.UnknownCommand };
            }
        }

        private MessageDTO ProcessEditTemplateText(Message message)
        {
            var user = _userRepository.GetUser(message.Chat.Id);
            user.IsAsked = false;
            user.TempleText = message.Text;
            return new MessageDTO { ChatId = message.Chat.Id, Text = user.TempleText };
        }

        private bool IsUserAsked(Message message)
        {
            var user = _userRepository.GetUser(message.Chat.Id);
            return user.IsAsked;
        }

        private bool IsCommand(Message message)
        {
            return message.Text.StartsWith('/');
        }
    }
}
