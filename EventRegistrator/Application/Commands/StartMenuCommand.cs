using EventRegistrator.Application.Interfaces;
using EventRegistrator.Application.Objects;
using EventRegistrator.Application.Objects.DTOs;
using EventRegistrator.Application.Objects.Enums;
using EventRegistrator.Application.Services;
using EventRegistrator.Application.States;
using EventRegistrator.Domain;
using EventRegistrator.Domain.Models;

namespace EventRegistrator.Application.Commands
{
    public sealed class StartMenuCommand : ICommand
    {
        private readonly MenuKey _key;
        private readonly IUserRepository _userRepository;
        public StartMenuCommand(IUserRepository userRepository, MenuKey key)
        {
            _key = key;
            _userRepository = userRepository;
        }

        public Task<List<Response>> Execute(MessageDTO message, UserAdmin user)
        {
            user.ClearStateHistory();
            user.SetCurrentState(new MenuState(
                menuService: new MenuService(_userRepository),
                key: _key,
                ctx: new MenuContext(message.ChatId),
                startPage: 0));
            return Task.FromResult(new List<Response> { user.State.Handle(message, user).Result });
        }
    }
}
