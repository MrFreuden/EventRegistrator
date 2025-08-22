using EventRegistrator.Application.Objects;
using EventRegistrator.Application.Objects.Enums;
using EventRegistrator.Application.States;

namespace EventRegistrator.Application.Interfaces
{
    public interface IMenuStateFactory
    {
        MenuState Create(MenuKey key, MenuContext ctx, int startPage = 0);
    }
}