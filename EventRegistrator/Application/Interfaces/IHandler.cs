using EventRegistrator.Application.Objects.DTOs;
using EventRegistrator.Domain.Models;

namespace EventRegistrator.Application.Interfaces
{
    public interface IHandler
    {
        Task<List<Response>> HandleAsync(MessageDTO message);
        bool CanHandle(MessageDTO message);
    }
}
