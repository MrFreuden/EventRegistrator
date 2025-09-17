using EventRegistrator.Application.Commands.Attributes;
using EventRegistrator.Application.DTOs;
using EventRegistrator.Application.Enums;
using EventRegistrator.Application.Interfaces;
using EventRegistrator.Domain;
using EventRegistrator.Domain.DTO;
using EventRegistrator.Domain.Models;

namespace EventRegistrator.Application.Commands
{
    [Command("/settings", "Настройки")]
    public class SettingsCommand : ICommand
    {
        private readonly IMenuStateFactory _menuStateFactory;

        public SettingsCommand(IMenuStateFactory menuStateFactory)
        {
            _menuStateFactory = menuStateFactory;
        }

        public async Task<List<Response>> Execute(MessageDTO message, UserAdmin user)
        {
            var state = new StartMenuCommand(_menuStateFactory, MenuKey.Hashtags);
            return await state.Execute(message, user);
        }
    }
}
