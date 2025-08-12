using EventRegistrator.Application.DTOs;
using EventRegistrator.Domain;
using EventRegistrator.Domain.Models;

namespace EventRegistrator.Application
{
    public class PrivateMessageHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly Dictionary<string, Func<ICommand>> _commands;

        public PrivateMessageHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
            _commands = new Dictionary<string, Func<ICommand>>
            {
                { "/start", () => new StartCommand(_userRepository) },
                { "/settings", () => new SettingsCommand() },
                { "/admin", () => new AdminCommand(_userRepository) }
            };
        }

        public async Task<List<Response>> Handle(MessageDTO message)
        {
            if (IsUserAsked(message))
            {
                var editTemplateTextState = new EditTemplateTextState(_userRepository);
                return [await editTemplateTextState.Handle(message)];
            }
            if (IsCommand(message))
            {
                var defaultState = new DefaultState(_commands);
                return [await defaultState.Handle(message)];
            }

            return [new Response { ChatId = message.ChatId, Text = Constants.Error }];
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
