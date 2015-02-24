using System;

using Barista.Foundation.Commanding;

namespace Barista.Commands
{
    public class CreateOrderCommand : ICommand
    {
        public Guid Identity { get; set; }
        public string BaristaName { get; set; }
    }
}
