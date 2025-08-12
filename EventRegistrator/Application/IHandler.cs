using EventRegistrator.Application.DTOs;
using EventRegistrator.Domain.Models;

namespace EventRegistrator.Application
{
    public interface IHandler
    {
        Task<List<Response>> HandleAsync(MessageDTO message);
        bool CanHandle(MessageDTO message);
    }
}
