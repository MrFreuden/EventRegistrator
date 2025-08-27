using EventRegistrator.Application.Commands.Attributes;
using EventRegistrator.Application.DTOs;
using EventRegistrator.Application.Interfaces;
using EventRegistrator.Domain.DTO;
using EventRegistrator.Domain.Interfaces;
using EventRegistrator.Domain.Models;
using EventRegistrator.Infrastructure.Persistence;
using EventRegistrator.Infrastructure.Utils;

namespace EventRegistrator.Application.Commands
{
    [Command("/admin", "Администрирование")]
    public class AdminCommand : ICommand
    {
        private readonly IUserRepository _userRepository;

        public AdminCommand(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<List<Response>> Execute(MessageDTO message, UserAdmin user = null)
        {
            var text2 = TextFormatter.GetAllUsersInfo(_userRepository as UserRepository);
            return [new Response { ChatId = message.ChatId, Text = text2 }];
        }
    }
}
