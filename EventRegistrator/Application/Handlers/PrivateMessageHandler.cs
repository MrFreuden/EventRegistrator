using EventRegistrator.Application.Commands;
using EventRegistrator.Application.Interfaces;
using EventRegistrator.Application.Objects.DTOs;
using EventRegistrator.Application.Objects.Enums;
using EventRegistrator.Application.States;
using EventRegistrator.Domain;
using EventRegistrator.Domain.Models;
using Microsoft.Extensions.Logging;

namespace EventRegistrator.Application.Handlers
{
    public class PrivateMessageHandler : IHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<PrivateMessageHandler> _logger;
        private readonly Dictionary<string, Func<ICommand>> _commands;

        public PrivateMessageHandler(IUserRepository userRepository, ILogger<PrivateMessageHandler> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
            _commands = new Dictionary<string, Func<ICommand>>
            {
                { "/start", () => new StartCommand(_userRepository) },
                { "/settings", () => new StartMenuCommand(_userRepository, MenuKey.TargetChats) },
                { "/admin", () => new AdminCommand(_userRepository) }
            };
        }

        public async Task<List<Response>> HandleAsync(MessageDTO message)
        {
            var user = _userRepository.GetUser(message.ChatId);
            if (user == null)
            {
                _logger.LogWarning("User not found for chat {ChatId}", message.ChatId);
                return new List<Response>();
            }

            if (IsUserAsked(message))
            {
                return await user.State.Execute(message, user);
            }
            if (IsCommand(message))
            {
                var defaultState = new DefaultState(_commands);
                return [await defaultState.Handle(message, user)];
            }

            _logger.LogError("Failed to handle private message. Message: {@Message}", message);
            return new List<Response>();
        }
        
        public bool CanHandle(MessageDTO message)
        {
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
