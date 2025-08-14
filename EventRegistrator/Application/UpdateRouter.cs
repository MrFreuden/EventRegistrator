using EventRegistrator.Application.DTOs;
using EventRegistrator.Application.Handlers;
using EventRegistrator.Application.Interfaces;
using EventRegistrator.Domain.Models;
using Microsoft.Extensions.Logging;

namespace EventRegistrator.Application
{
    public class UpdateRouter
    {
        private readonly List<IHandler> _handlers;
        private readonly ILogger<UpdateRouter> _logger;
        public UpdateRouter(IEnumerable<IHandler> handlers, ILogger<UpdateRouter> logger)
        {
            _handlers = handlers.ToList();
            _logger = logger;   
        }

        public async Task<List<Response>> RouteMessage(MessageDTO message)
        {
            var handler = _handlers.FirstOrDefault(h => h.CanHandle(message));
            if (handler != null)
            {
                return await handler.HandleAsync(message);
            }

            _logger.LogError("Cant find handler for {Message}", message);

            return new List<Response>();
        }

        public async Task<List<Response>> RouteCallback(MessageDTO message)
        {
            var handler = _handlers.FirstOrDefault(h => h.CanHandle(message));
            if (handler != null)
            {
                return await handler.HandleAsync(message);
            }

            _logger.LogError("Cant find handler for {Callback}", message);

            return new List<Response>();
        }
    }
}
