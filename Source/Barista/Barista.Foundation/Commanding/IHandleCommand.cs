namespace Barista.Foundation.Commanding
{
    public interface IHandleCommand<in TCommand>
    {
        void Handle(TCommand command);
    }
}
