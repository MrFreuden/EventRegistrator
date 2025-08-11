using EventRegistrator.Domain;
using EventRegistrator.Domain.Models;
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

        public AntwortDTO Handle(MessageDTO message)
        {
            if (IsCommand(message))
            {
                return ProcessPrivateMessageCommand(message);
            }
            else if (IsUserAsked(message))
            {
                return ProcessEditTemplateText(message);
            }
            return new AntwortDTO { ChatId = message.ChatId, Text = Constants.Error };
        }

        private AntwortDTO ProcessPrivateMessageCommand(MessageDTO message)
        {
            switch (message.Text)
            {
                case "/start":
                    _userRepository.AddUser(message.ChatId);
                    return new AntwortDTO { ChatId = message.ChatId, Text = Constants.Greetings };
                case "/settings":
                    var text = _userRepository.GetUser(message.ChatId).GetTargetChat().GetHashtagByName("sws").TemplateText;
                    return new AntwortDTO { ChatId = message.ChatId, Text = text };
                case "/admin":
                    var text2 = TextFormatter.GetAllUsersInfo(_userRepository as UserRepository);
                    return new AntwortDTO { ChatId = message.ChatId, Text = text2 };
                default:
                    return new AntwortDTO { ChatId = message.ChatId, Text = Constants.UnknownCommand };
            }
        }

        private AntwortDTO ProcessEditTemplateText(MessageDTO message)
        {
            var user = _userRepository.GetUser(message.ChatId);
            user.IsAsked = false;
            var hashtag = user.GetTargetChat().GetHashtagByName("sws");
            hashtag.EditTemplateText(message.Text);
            return new AntwortDTO { ChatId = message.ChatId, Text = hashtag.TemplateText };
        }

        private bool IsUserAsked(MessageDTO message)
        {
            var user = _userRepository.GetUser(message.ChatId);
            return user.IsAsked;
        }

        private bool IsCommand(MessageDTO message)
        {
            return message.Text.StartsWith('/');
        }
    }
}
