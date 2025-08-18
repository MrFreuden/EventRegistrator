using EventRegistrator.Application.Interfaces;
using EventRegistrator.Application.Objects.DTOs;
using EventRegistrator.Domain;
using EventRegistrator.Domain.Models;
using Microsoft.Extensions.Logging;

namespace EventRegistrator.Application.Handlers
{
    public class TargetChatMessageHandler : IHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly ICommandFactory _commandFactory;
        private readonly ILogger<TargetChatMessageHandler> _logger;

        public TargetChatMessageHandler(IUserRepository userRepository, ICommandFactory commandFactory, ILogger<TargetChatMessageHandler> logger)
        {
            _userRepository = userRepository;
            _commandFactory = commandFactory;
            _logger = logger;
        }

        public async Task<List<Response>> HandleEditAsync(MessageDTO message)
        {
            var user = _userRepository.GetUserByTargetChat(message.ChatId);
            if (user == null)
            {
                _logger.LogWarning("HandleEditAsync: user not found for chat {ChatId}", message.ChatId);
                return new List<Response>();
            }

            var commandType = CommandTypeResolver.DetermineCommandType(message, user);
            if (commandType == null)
            {
                _logger.LogWarning("HandleEditAsync: command type not determined for user {UserId}", user.Id);
                return new List<Response>();
            }

            var command = _commandFactory.CreateCommand(commandType.Value);
            if (command == null)
            {
                _logger.LogError("HandleEditAsync: failed to create command for type {CommandType}", commandType.Value);
                return new List<Response>();
            }

            var result = await command.Execute(message, user);

            var commandType2 = CommandTypeResolver.DetermineCommandType(message, user);
            var command2 = _commandFactory.CreateCommand(commandType2.Value);
            if (command2 != null)
            {
                result.AddRange(await command2.Execute(message, user));
            }
            else
            {
                _logger.LogError("HandleEditAsync: failed to create second command for type {CommandType}", commandType2);
            }
            await _userRepository.Save(user);
            return result;
        }

        public async Task<List<Response>> HandleAsync(MessageDTO message)
        {
            if (message.IsEdit)
            {
                return await HandleEditAsync(message);
            }

            var user = _userRepository.GetUserByTargetChat(message.ChatId);
            if (user == null)
            {
                _logger.LogWarning("HandleAsync: user not found for chat {ChatId}", message.ChatId);
                return new List<Response>();
            }

            var commandType = CommandTypeResolver.DetermineCommandType(message, user);
            if (commandType == null)
            {
                _logger.LogWarning("HandleAsync: command type not determined for user {UserId}", user.Id);
                return new List<Response>();
            }

            var command = _commandFactory.CreateCommand(commandType.Value);
            if (command == null)
            {
                _logger.LogError("HandleAsync: failed to create command for type {CommandType}", commandType.Value);
                return new List<Response>();
            }
            user.CurrentContext = new Objects.MenuContext(user.PrivateChatId, message.ChatId);
            var response = await command.Execute(message, user);
            if (response == null || response.Count == 0)
            {
                _logger.LogError("Failed to execute command. Command: {Command}", command);
                return new List<Response>();
            }
            await _userRepository.Save(user);
            return response;
        }

        public bool CanHandle(MessageDTO message)
        {
            var user = _userRepository.GetUserByTargetChat(message.ChatId);
            return IsMessageFromTargetChat(message, user);
        }

        private static bool IsMessageFromTargetChat(MessageDTO message, UserAdmin user)
        {
            return user != null;
        }
    }
}
