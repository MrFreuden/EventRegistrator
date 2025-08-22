using EventRegistrator.Application.Interfaces;
using EventRegistrator.Application.Objects.DTOs;
using EventRegistrator.Domain;
using EventRegistrator.Domain.Models;
using Microsoft.Extensions.Logging;

namespace EventRegistrator.Application.Handlers
{
    public class GeneralCallbackQueryHandler : IHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly ICommandFactory _commandFactory;
        private readonly IStateFactory _stateFactory;
        private readonly ILogger<GeneralCallbackQueryHandler> _logger;

        public GeneralCallbackQueryHandler(IUserRepository userRepository, ICommandFactory commandFactory, IStateFactory stateFactory, ILogger<GeneralCallbackQueryHandler> logger)
        {
            _userRepository = userRepository;
            _commandFactory = commandFactory;
            _stateFactory = stateFactory;
            _logger = logger;
        }

        public async Task<List<Response>> HandleAsync(MessageDTO message)
        {
            var user = _userRepository.GetUser(message.ChatId) ??
                _userRepository.GetUserByTargetChat(message.ChatId);
            if (user == null)
            {
                _logger.LogWarning("User not found for chat {ChatId}", message.ChatId);
                return new List<Response>();
            }
            if (user.State != null)
            {
                var response = await user.State.Execute(message, user);
                return response;
            }
            if (message.Text.StartsWith("Cancel"))
            {
                var cancelCommand = _commandFactory.CreateCommand(Objects.Enums.CommandType.CancelRegistrations);
                return await cancelCommand.Execute(message, user);
            }
            _logger.LogError("Failed to handle callback {MessageDTO}", message.Text);
            return [];
        }

        public bool CanHandle(MessageDTO message)
        {
            return (message.ChatId > 0 || message.Text.StartsWith("Cancel"));
        }
    }
}
