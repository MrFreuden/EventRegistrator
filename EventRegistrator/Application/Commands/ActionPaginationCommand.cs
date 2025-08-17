using EventRegistrator.Application.Interfaces;
using EventRegistrator.Application.Objects.DTOs;
using EventRegistrator.Domain;
using EventRegistrator.Domain.Models;

namespace EventRegistrator.Application.Commands
{
    public class ActionPaginationCommand : ICommand
    {
        private readonly IPagiable _items;
        public Task<List<Response>> Execute(MessageDTO message, UserAdmin user)
        {
            if (_items is TargetChat)
            {
            }
            throw new NotImplementedException();
        }
    }
}
