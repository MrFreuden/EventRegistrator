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
            var user = _userRepository.GetUser(message.ChatId);
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

            _logger.LogError("Failed to handle callback {MessageDTO}", message);
            return [];
        }

        public bool CanHandle(MessageDTO message)
        {
            return (message.ChatId > 0 || message.Text.StartsWith("Cancel"));
        }
    }
}
