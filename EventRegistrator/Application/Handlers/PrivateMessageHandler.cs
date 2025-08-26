using EventRegistrator.Application.Commands;
using EventRegistrator.Application.DTOs;
using EventRegistrator.Application.Interfaces;
using EventRegistrator.Application.Services;
using EventRegistrator.Application.States;
using EventRegistrator.Domain.DTO;
using EventRegistrator.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EventRegistrator.Application.Handlers
{
    public class PrivateMessageHandler : IHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<PrivateMessageHandler> _logger;
        private readonly ICommandFactory _commandFactory;

        public PrivateMessageHandler(IUserRepository userRepository, ICommandFactory commandFactory, ILogger<PrivateMessageHandler> logger)
        {
            _userRepository = userRepository;
            _commandFactory = commandFactory;
            _logger = logger;
        }

        public async Task<List<Response>> HandleAsync(MessageDTO message)
        {
            var user = _userRepository.GetUser(message.ChatId);
            if (user == null)
            {
                _logger.LogWarning("User not found for chat {ChatId}", message.ChatId);
                return new List<Response>();
            }
            
            if (IsSlashCommand(message))
            {
                var command = _commandFactory.CreateSlashCommand(message.Text);
                return await command.Execute(message, user);
            }

            if (user.State != null && user.State as DefaultState == null)
            {
                var response = await user.State.Execute(message, user);
                if (response != null && response.Count > 0)
                {
                    await _userRepository.Save(user);
                    return response;
                }

                _logger.LogError("Failed to execute state. State: {State}", user.State);
                return new List<Response>();
            }

            _logger.LogError("Failed to handle private message. Message: {@Message}", message);
            return new List<Response>();
        }

        public bool CanHandle(MessageDTO message)
        {
            return IsPrivateMessage(message);
        }

        private bool IsSlashCommand(MessageDTO message)
        {
            return message.Text.StartsWith('/');
        }

        private bool IsPrivateMessage(MessageDTO message)
        {
            return message.ChatId > 0;
        }
    }
}
