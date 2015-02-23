namespace Barista.Foundation.Commanding
{
    public interface IAggregateFactory<in TCommand, out TAggregateRoot>
    {
        TAggregateRoot Create(TCommand command);
    }
}
