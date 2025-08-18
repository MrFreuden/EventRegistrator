using EventRegistrator.Application.Objects.Enums;

namespace EventRegistrator.Application.Interfaces
{
    public interface ICommandFactory
    {
        ICommand CreateCommand(CommandType commandType);
    }
}
