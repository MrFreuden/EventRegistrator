using EventRegistrator.Application.Commands;
using EventRegistrator.Application.Interfaces;
using EventRegistrator.Application.Services;

namespace EventRegistrator.Application
{
    public class CommandFactory : ICommandFactory
    {
        private readonly EventService _eventService;
        private readonly RegistrationService _registrationService;
        private readonly ResponseManager _responseManager;

        public CommandFactory(
            EventService eventService,
            RegistrationService registrationService,
            ResponseManager responseManager)
        {
            _eventService = eventService;
            _registrationService = registrationService;
            _responseManager = responseManager;
        }

        public ICommand CreateCommand(CommandType commandType)
        {
            return commandType switch
            {
                CommandType.CreateEvent => new CreateEventCommand(_eventService),
                CommandType.Register => new RegisterCommand(_registrationService, _responseManager),
                CommandType.CancelRegistration => new DeleteRegistrationsCommand(_registrationService, _responseManager),
                _ => throw new ArgumentException($"Неизвестный тип команды: {commandType}")
            };
        }
    }
}
