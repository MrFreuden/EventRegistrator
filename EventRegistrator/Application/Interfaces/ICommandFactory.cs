using EventRegistrator.Application.Enums;

namespace EventRegistrator.Application.Interfaces
{
    public interface ICommandFactory
    {
        ICommand CreateCommand(CommandType commandType);
        ICommand CreateSlashCommand(string? text);
    }
}
