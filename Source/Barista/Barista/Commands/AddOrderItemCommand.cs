using System;
using Barista.Foundation.Commanding;

namespace Barista.Commands
{
    public class AddOrderItemCommand : ICommand
    {
        public Guid OrderIdentity { get; set; }

        public string ProductName { get; set; }
        public int Quantity { get; set; }
    }
}
