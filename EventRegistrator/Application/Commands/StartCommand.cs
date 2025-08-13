using EventRegistrator.Application.DTOs;
using EventRegistrator.Application.Interfaces;
using EventRegistrator.Domain;
using EventRegistrator.Domain.Models;

namespace EventRegistrator.Application.Commands
{
    public class StartCommand : ICommand
    {
        private readonly IUserRepository _userRepository;

        public StartCommand(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<List<Response>> Execute(MessageDTO message, UserAdmin user = null)
        {
            _userRepository.AddUser(message.ChatId);
            return [new Response { ChatId = message.ChatId, Text = Constants.Greetings }];
        }
    }
}
