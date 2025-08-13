using EventRegistrator.Application.DTOs;
using EventRegistrator.Application.Interfaces;
using EventRegistrator.Domain;
using EventRegistrator.Domain.Models;

namespace EventRegistrator.Application.Handlers
{
    public class GeneralCallbackQueryHandler : IHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly CommandStateFactory _commandStateFactory;

        public GeneralCallbackQueryHandler(IUserRepository userRepository, CommandStateFactory commandStateFactory)
        {
            _userRepository = userRepository;
            _commandStateFactory = commandStateFactory;
        }

        public async Task<List<Response>> HandleAsync(MessageDTO message)
        {
            var user = _userRepository.GetUserByTargetChat(message.ChatId);
            var stateType = CommandTypeResolver.DetermineStateType(message, user);
            var state = _commandStateFactory.CreateState(stateType.Value);
            user.State = state;
            return [await state.Handle(message, user)];
        }

        public bool CanHandle(MessageDTO message)
        {
            return (message.ChatId > 0 || message.Text.StartsWith("Cancel"));
        }
    }
}
