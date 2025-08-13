using EventRegistrator.Application.DTOs;
using EventRegistrator.Application.Interfaces;
using EventRegistrator.Domain.Models;

namespace EventRegistrator.Application
{
    public class UpdateRouter
    {
        private readonly List<IHandler> _handlers;
        public UpdateRouter(IEnumerable<IHandler> handlers)
        {
            _handlers = handlers.ToList();
        }

        public async Task<List<Response>> RouteMessage(MessageDTO message)
        {
            var handler = _handlers.FirstOrDefault(h => h.CanHandle(message));
            if (handler != null)
            {
                return await handler.HandleAsync(message);
            }
            return new List<Response> { new Response { ChatId = message.ChatId, Text = Constants.Error } };
        }

        public async Task<List<Response>> RouteCallback(MessageDTO message)
        {
            var handler = _handlers.FirstOrDefault(h => h.CanHandle(message));
            if (handler != null)
            {
                return await handler.HandleAsync(message);
            }
            return new List<Response> { new Response { ChatId = message.ChatId, Text = Constants.Error } };
        }
    }
}
