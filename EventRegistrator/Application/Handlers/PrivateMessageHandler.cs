using EventRegistrator.Application.Commands;
using EventRegistrator.Application.DTOs;
using EventRegistrator.Application.Interfaces;
using EventRegistrator.Application.States;
using EventRegistrator.Domain.DTO;
using EventRegistrator.Domain.Interfaces;
using EventRegistrator.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace EventRegistrator.Application.Handlers
{
    public class PrivateMessageHandler : IHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IMenuStateFactory _menuStateFactory;
        private readonly ILogger<PrivateMessageHandler> _logger;
        private readonly RepositoryLoader _loader;
        private readonly Dictionary<string, Func<ICommand>> _commands;

        public PrivateMessageHandler(IUserRepository userRepository, IMenuStateFactory menuStateFactory, ILogger<PrivateMessageHandler> logger, RepositoryLoader loader)
        {
            _userRepository = userRepository;
            _menuStateFactory = menuStateFactory;
            _logger = logger;
            _loader = loader;
            _commands = new Dictionary<string, Func<ICommand>>
            {
                { "/start", () => new StartCommand(_userRepository) },
                { "/settings", () => new SettingsCommand(_menuStateFactory) },
                { "/admin", () => new AdminInfoCommand(_userRepository) },
                { "/save", () => new AdminSaveCommand(_userRepository, _loader) }
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
            
            if (IsCommand(message))
            {
                var defaultState = new DefaultState(_commands);
                return [await defaultState.Handle(message, user)];
            }

            if (user.State != null || user.State as DefaultState == null)
            {
                var response = await user.State.Execute(message, user);
                if (response == null || response.Count == 0)
                {
                    _logger.LogError("Failed to execute state. State: {State}", user.State);
                    return new List<Response>();
                }
                await _userRepository.Save(user);
                return response;
            }

            _logger.LogError("Failed to handle private message. Message: {@Message}", message);
            return new List<Response>();
        }

        public bool CanHandle(MessageDTO message)
        {
            return IsPrivateMessage(message);
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
