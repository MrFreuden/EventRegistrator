using EventRegistrator.Application.Commands;
using EventRegistrator.Application.DTOs;
using EventRegistrator.Application.Interfaces;
using EventRegistrator.Application.States;
using EventRegistrator.Domain;
using EventRegistrator.Domain.Models;

namespace EventRegistrator.Application.Handlers
{
    public class PrivateMessageHandler : IHandler
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

        public async Task<List<Response>> HandleAsync(MessageDTO message)
        {
            var user = _userRepository.GetUser(message.ChatId); 
            if (IsUserAsked(message))
            {
                return [await user.State.Handle(message)];
            }
            if (IsCommand(message))
            {
                var defaultState = new DefaultState(_commands);
                return [await defaultState.Handle(message)];
            }

            return [new Response { ChatId = message.ChatId, Text = Constants.Error }];
        }

        public bool CanHandle(MessageDTO message)
        {
            //var v = IsCommand(message) == false ? IsUserAsked(message) : true;
            return IsPrivateMessage(message);
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

        private bool IsPrivateMessage(MessageDTO message)
        {
            return message.ChatId > 0;
        }
    }
}
