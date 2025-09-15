using EventRegistrator.Application.Commands.Attributes;
using EventRegistrator.Application.DTOs;
using EventRegistrator.Application.Interfaces;
using EventRegistrator.Domain.DTO;
using EventRegistrator.Domain.Interfaces;
using EventRegistrator.Domain.Models;
using EventRegistrator.Infrastructure.Config;
using EventRegistrator.Infrastructure.Persistence;

namespace EventRegistrator.Application.Commands
{
    [Command("/save", "Сохранение")]
    public class AdminSaveCommand : ICommand
    {  
        private readonly IUserRepository _userRepository;
        private readonly RepositoryLoader _repositoryLoader;

        public AdminSaveCommand(IUserRepository userRepository, RepositoryLoader repositoryLoader)
        {
            _userRepository = userRepository;
            _repositoryLoader = repositoryLoader;
        }

        public async Task<List<Response>> Execute(MessageDTO message, UserAdmin user)
        {
            if (EnvLoader.GetAdminId() == message.UserId)
            {
                await _repositoryLoader.SaveDataAsync(_userRepository as UserRepository);
                var r = new Response { ChatId = message.ChatId, Text = "Сохранение" };
                return [r];
            }
            return [];
        }
    }
}
