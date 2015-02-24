namespace Barista.Domain
{
    public class OrderItem
    {
        public string ProductName { get; private set; }

        public int Quantity { get; private set; }

        public OrderItem(string productName, int quantity)
        {
            ProductName = productName;
            Quantity = quantity;
        }
    }
}
