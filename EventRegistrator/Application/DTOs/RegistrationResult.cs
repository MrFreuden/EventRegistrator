using EventRegistrator.Domain.Models;

namespace EventRegistrator.Application.DTOs
{
    public class RegistrationResult
    {
        public bool Success { get; set; }
        public Event Event { get; set; }
        public int MessageId { get; set; }

    }
}
