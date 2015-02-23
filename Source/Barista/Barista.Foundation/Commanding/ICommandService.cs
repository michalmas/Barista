using System.Collections.Generic;

namespace Barista.Foundation.Commanding
{
    public interface ICommandService
    {
        /// <summary>
        /// Executes a single command.
        /// </summary>
        void Execute(ICommand command);

        /// <summary>
        /// Executes a batch of commands as one atomic operation.
        /// </summary>
        void Execute(IEnumerable<ICommand> commands);
    }
}
