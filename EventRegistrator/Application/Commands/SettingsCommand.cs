using EventRegistrator.Application.Interfaces;
using EventRegistrator.Application.Objects.DTOs;
using EventRegistrator.Application.Objects.Enums;
using EventRegistrator.Domain;
using EventRegistrator.Domain.Models;

namespace EventRegistrator.Application.Commands
{
    public class SettingsCommand : ICommand
    {
        private readonly IUserRepository _userRepository;

        public SettingsCommand(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<List<Response>> Execute(MessageDTO message, UserAdmin user)
        {
            var state = new StartMenuCommand(_userRepository, MenuKey.TargetChats);
            return await state.Execute(message, user);
        }
    }
}
