using EventRegistrator.Application.DTOs;
using EventRegistrator.Domain.DTO;

namespace EventRegistrator.Application.Interfaces
{
    public interface IHandler
    {
        Task<List<Response>> HandleAsync(MessageDTO message);
        bool CanHandle(MessageDTO message);
    }
}
