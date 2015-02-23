using System;
using System.Collections.Generic;

namespace Barista.Foundation.Commanding
{
    /// <summary>
    /// Provides functionality to wrap a handler with a number of decorators.
    /// </summary>
    public class DecoratorPipeline
    {
        private readonly Stack<Func<Func<IHandleCommandOn>, Func<IHandleCommandOn>>> decorators =
            new Stack<Func<Func<IHandleCommandOn>, Func<IHandleCommandOn>>>();

        public void AddDecorator(Func<Func<IHandleCommandOn>, Func<IHandleCommandOn>> decoratorFactory)
        {
            decorators.Push(decoratorFactory);
        }

        public Func<IHandleCommandOn> Wrap(Func<IHandleCommandOn> innerFactory)
        {
            var factory = innerFactory;

            while (decorators.Count > 0)
            {
                factory = decorators.Pop()(factory);
            }

            return factory;
        }
    }
}
