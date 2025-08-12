using EventRegistrator.Application.DTOs;
using EventRegistrator.Domain;
using EventRegistrator.Domain.Models;
using EventRegistrator.Infrastructure;

namespace EventRegistrator.Application
{
    public class MessageRouter
    {
        private readonly IUserRepository _userRepository;
        private readonly PrivateMessageHandler _privateMessageHandler;
        private readonly TargetChatMessageHandler _targetChatMessageHandler;

        public MessageRouter(IUserRepository userRepository, PrivateMessageHandler privateMessageHandler, TargetChatMessageHandler targetChatMessageHandler)
        {
            _userRepository = userRepository;
            _privateMessageHandler = privateMessageHandler;
            _targetChatMessageHandler = targetChatMessageHandler;
        }

        public async Task<List<Response>> Route(MessageDTO message)
        {
            if (IsPrivateMessage(message))
            {
                return await _privateMessageHandler.Handle(message);
            }
            else if (IsMessageFromTargetChat(message))
            {
                return await _targetChatMessageHandler.Handle(message);
            }

            return [new Response { ChatId = message.ChatId, Text = Constants.Error }];
        }

        private bool IsPrivateMessage(MessageDTO message)
        {
            return message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Private;
        }

        private bool IsMessageFromTargetChat(MessageDTO message)
        {
            var user = _userRepository.GetUserByTargetChat(message.Chat.Id);
            return user != null;
        }
    }

    public interface ICommand
    {
        Task<Response> Execute(MessageDTO message, UserAdmin user = null);
    }

    public class StartCommand : ICommand
    {
        private readonly IUserRepository _userRepository;

        public StartCommand(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Response> Execute(MessageDTO message, UserAdmin user = null)
        {
            _userRepository.AddUser(message.ChatId);
            return new Response { ChatId = message.ChatId, Text = Constants.Greetings };
        }
    }

    public class SettingsCommand : ICommand
    {
        public async Task<Response> Execute(MessageDTO message, UserAdmin user)
        {
            var text = user.GetTargetChat().GetHashtagByName("sws").TemplateText;
            return new Response { ChatId = message.ChatId, Text = text };
        }
    }
    public class AdminCommand : ICommand
    {
        private readonly IUserRepository _userRepository;

        public AdminCommand(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<Response> Execute(MessageDTO message, UserAdmin user = null)
        {
            var text2 = TextFormatter.GetAllUsersInfo(_userRepository as UserRepository);
            return new Response { ChatId = message.ChatId, Text = text2 };
        }
    }
    public class EditTemplateTextCommand : ICommand
    {
        public async Task<Response> Execute(MessageDTO message, UserAdmin user)
        {
            user.IsAsked = false;
            var hashtag = user.GetTargetChat().GetHashtagByName("sws");
            hashtag.EditTemplateText(message.Text);
            return new Response { ChatId = message.ChatId, Text = hashtag.TemplateText };
        }
    }
    public class CreateEventCommand : ICommand
    {
        public async Task<Response> Execute(MessageDTO message, UserAdmin user)
        {
            throw new NotImplementedException();
        }
    }
    public class RegisterCommand : ICommand
    {
        public async Task<Response> Execute(MessageDTO message, UserAdmin user)
        {
            throw new NotImplementedException();
        }
    }
    public interface IState
    {
        Task<Response> Handle(MessageDTO message);
    }

    public class DefaultState : IState
    {
        private Dictionary<string, Func<ICommand>> _commands;

        public DefaultState(Dictionary<string, Func<ICommand>> commands)
        {
            _commands = commands;
        }

        public async Task<Response> Handle(MessageDTO message)
        {
            throw new NotImplementedException();
        }
    }
    public class EditTemplateTextState : IState
    {
        private readonly IUserRepository _userRepository;

        public EditTemplateTextState(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Response> Handle(MessageDTO message)
        {
            throw new NotImplementedException();
        }
    }
}
