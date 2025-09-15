using EventRegistrator.Application.DTOs;
using EventRegistrator.Application.Interfaces;
using EventRegistrator.Domain.DTO;
using EventRegistrator.Domain.Models;
using EventRegistrator.Infrastructure.Config;

namespace EventRegistrator.Application.Commands
{
    public abstract class AdminCommandBase : ICommand
    {
        public async Task<List<Response>> Execute(MessageDTO message, UserAdmin user)
        {
            if (EnvLoader.GetAdminId() == message.UserId)
            {
                return await ExecuteAdminCommand(message);
            }
            return [];
        }

        protected abstract Task<List<Response>> ExecuteAdminCommand(MessageDTO message);
    }
}
