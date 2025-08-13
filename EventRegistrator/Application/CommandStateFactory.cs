using EventRegistrator.Application.Commands;
using EventRegistrator.Application.Enums;
using EventRegistrator.Application.Interfaces;
using EventRegistrator.Application.Services;
using EventRegistrator.Application.States;
using CommandType = EventRegistrator.Application.Enums.CommandType;

namespace EventRegistrator.Application
{
    public class CommandStateFactory : ICommandFactory, IStateFactory
    {
        private readonly EventService _eventService;
        private readonly RegistrationService _registrationService;
        private readonly ResponseManager _responseManager;

        public CommandStateFactory(
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

        public IState CreateState(StateType stateType)
        {
            return stateType switch
            {
                StateType.EditTemplateText => new EditTemplateTextState(),
                _ => throw new ArgumentException($"Неизвестный тип состояния: {stateType}")
            };
        }
    }
}
