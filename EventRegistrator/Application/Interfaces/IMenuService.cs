using EventRegistrator.Application.Enums;

namespace EventRegistrator.Application.Interfaces
{
    public interface IMenuService
    {
        MenuDescriptor Get(MenuKey key, MenuContext ctx);
    }

}
