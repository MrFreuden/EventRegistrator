using EventRegistrator.Application.Objects;
using EventRegistrator.Application.Objects.Enums;

namespace EventRegistrator.Application.Interfaces
{
    public interface IMenuService
    {
        MenuDescriptor Get(MenuKey key, MenuContext ctx);
    }

}
