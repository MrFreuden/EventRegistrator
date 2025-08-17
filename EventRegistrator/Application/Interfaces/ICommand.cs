using EventRegistrator.Application.Objects.DTOs;
using EventRegistrator.Domain.Models;

namespace EventRegistrator.Application.Interfaces
{
    public interface ICommand
    {
        Task<List<Response>> Execute(MessageDTO message, UserAdmin user = null);
    }
}
