using EventRegistrator.Application.DTOs;
using EventRegistrator.Application.Interfaces;
using EventRegistrator.Domain;
using EventRegistrator.Domain.Models;

namespace EventRegistrator.Application.States
{
    public class EditTemplateTextState : IState
    {
        private readonly IUserRepository _userRepository;

        public EditTemplateTextState(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Response> Handle(MessageDTO message)
        {
            throw new NotImplementedException();
        }
    }
}
