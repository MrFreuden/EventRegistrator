using EventRegistrator.Application.DTOs;
using EventRegistrator.Application.Interfaces;
using EventRegistrator.Domain;
using EventRegistrator.Domain.Models;
using EventRegistrator.Infrastructure;

namespace EventRegistrator.Application.Commands
{
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
