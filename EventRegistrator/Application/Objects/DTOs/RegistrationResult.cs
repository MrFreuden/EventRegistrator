using EventRegistrator.Domain.Models;

namespace EventRegistrator.Application.Objects.DTOs
{
    public class RegistrationResult
    {
        public bool Success { get; set; }
        public Event Event { get; set; }
        public List<int> MessageIds { get; set; }
    }
}
