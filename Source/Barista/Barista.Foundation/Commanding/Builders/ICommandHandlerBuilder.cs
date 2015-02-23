using System;

namespace Barista.Foundation.Commanding
{
    public interface ICommandHandlerBuilder<in TCommand>
    {
        /// <summary>
        /// Adds a decorator that should be added to the pipeline.
        /// </summary>
        /// <param name="decoratorType">The type of decorator. Should be a generic type with two type parameters. Command & Aggregate</param>
        void AddDecorator(Type decoratorType);

        /// <summary>
        /// Builds a handler pipeline for <typeparamref name="TCommand"/>.
        /// </summary>
        /// <returns>An instance to the handle.</returns>
        IHandleCommand<TCommand> Build();
    }
}
