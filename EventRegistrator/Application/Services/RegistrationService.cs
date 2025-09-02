using EventRegistrator.Application.DTOs;
using EventRegistrator.Domain;
using EventRegistrator.Domain.Models;

namespace EventRegistrator.Application.Services
{
    public class RegistrationService
    {
        public RegistrationResult ProcessRegistration(Event @event, List<Registration> registrations)
        {
            foreach (var registration in registrations)
            {
                var result = @event.AddRegistration(registration);
                if (result == true)
                {
                    return new RegistrationResult { Event = @event, Success = true };
                }
            }
            Console.WriteLine("Ошибка добавления во временной слот");
            return new RegistrationResult { Success = false };
        }

        public RegistrationResult CancelRegistration(Event @event, int messageId)
        {
            @event.RemoveRegistrations(messageId);

            return new RegistrationResult { Event = @event, MessageIds = [messageId], Success = true };
        }

        public RegistrationResult CancelAllRegistrations(Event @event, long userId)
        {
            var ids = @event.RemoveRegistrations(userId);

            return new RegistrationResult { Event = @event, Success = true, MessageIds = ids};
        }
    }
}
