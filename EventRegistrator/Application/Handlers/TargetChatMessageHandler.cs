using EventRegistrator.Application.DTOs;
using EventRegistrator.Application.Interfaces;
using EventRegistrator.Application.Services;
using EventRegistrator.Domain;
using EventRegistrator.Domain.Models;

namespace EventRegistrator.Application.Handlers
{
    public class TargetChatMessageHandler : IHandler
    {

        private readonly IUserRepository _userRepository;
        private readonly ICommandFactory _commandFactory;

        public TargetChatMessageHandler(IUserRepository userRepository, ICommandFactory commandFactory)
        {
            _userRepository = userRepository;
            _commandFactory = commandFactory;
        }

        public async Task<List<Response>> HandleEditAsync(MessageDTO message)
        {
            var user = _userRepository.GetUserByTargetChat(message.ChatId);
            var commandType = CommandTypeResolver.DetermineCommandType(message, user);
            var command = _commandFactory.CreateCommand(commandType.Value);
            var result = await command.Execute(message, user);


            var commandType2 = CommandTypeResolver.DetermineCommandType(message, user);
            var command2 = _commandFactory.CreateCommand(commandType2.Value);
            result.AddRange(await command2.Execute(message, user));

            return result;
        }

        public async Task<List<Response>> HandleAsync(MessageDTO message)
        {
            if (message.IsEdit)
            {
                return await HandleEditAsync(message);
            }
            var user = _userRepository.GetUserByTargetChat(message.ChatId);
            var commandType = CommandTypeResolver.DetermineCommandType(message, user);
            var command = _commandFactory.CreateCommand(commandType.Value);
            return await command.Execute(message, user);
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
