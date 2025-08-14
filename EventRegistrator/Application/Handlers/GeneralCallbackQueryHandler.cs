using EventRegistrator.Application.DTOs;
using EventRegistrator.Application.Interfaces;
using EventRegistrator.Domain;
using EventRegistrator.Domain.Models;
using Microsoft.Extensions.Logging;

namespace EventRegistrator.Application.Handlers
{
    public class GeneralCallbackQueryHandler : IHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly CommandStateFactory _commandStateFactory;
        private readonly ILogger<GeneralCallbackQueryHandler> _logger;

        public GeneralCallbackQueryHandler(
            IUserRepository userRepository,
            CommandStateFactory commandStateFactory,
            ILogger<GeneralCallbackQueryHandler> logger)
        {
            _userRepository = userRepository;
            _commandStateFactory = commandStateFactory;
            _logger = logger;
        }

        public async Task<List<Response>> HandleAsync(MessageDTO message)
        {
            var user = _userRepository.GetUserByTargetChat(message.ChatId);
            if (user == null)
            {
                _logger.LogWarning("User not found for chat {ChatId}", message.ChatId);
                return new List<Response>();
            }

            var stateType = CommandTypeResolver.DetermineStateType(message, user);
            if (!stateType.HasValue)
            {
                _logger.LogWarning("Cannot determine state type for user {UserId}", user.Id);
                return new List<Response>();
            }

            var state = _commandStateFactory.CreateState(stateType.Value);
            if (state == null)
            {
                _logger.LogError("Failed to create state for type {StateType}", stateType.Value);
                return new List<Response>();
            }

            user.State = state;
            var response = await state.Handle(message, user);
            return new List<Response> { response };
        }

        public bool CanHandle(MessageDTO message)
        {
            return (message.ChatId > 0 || message.Text.StartsWith("Cancel"));
        }
    }
}
