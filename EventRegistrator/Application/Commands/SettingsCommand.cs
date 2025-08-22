using EventRegistrator.Application.Interfaces;
using EventRegistrator.Application.Objects.DTOs;
using EventRegistrator.Application.Objects.Enums;
using EventRegistrator.Domain;
using EventRegistrator.Domain.Models;

namespace EventRegistrator.Application.Commands
{
    public class SettingsCommand : ICommand
    {
        private readonly IMenuStateFactory _menuStateFactory;

        public SettingsCommand(IMenuStateFactory menuStateFactory)
        {
            _menuStateFactory = menuStateFactory;
        }

        public async Task<List<Response>> Execute(MessageDTO message, UserAdmin user)
        {
            var state = new StartMenuCommand(_menuStateFactory, MenuKey.TargetChats);
            return await state.Execute(message, user);
        }
    }
}
