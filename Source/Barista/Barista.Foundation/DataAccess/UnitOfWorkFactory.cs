using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Web;

namespace Barista.Foundation.DataAccess
{
    public abstract class UnitOfWorkFactory<TUnitOfWork> : IUnitOfWorkFactory<TUnitOfWork> 
        where TUnitOfWork : UnitOfWork
    {
        private readonly string ContextKey;

        protected UnitOfWorkFactory()
        {
            ContextKey = GetType().FullName;
        }

        public TUnitOfWork Create()
        {
            return Create(UnitOfWorkOption.ExistingOrNew);
        }

        public TUnitOfWork Create(UnitOfWorkOption option)
        {
            TUnitOfWork uow = GetTop();
            if ((option == UnitOfWorkOption.ExistingOrNew) && (uow != null))
            {
                uow.IncreaseReferenceCount();
            }
            else
            {
                uow = CreateNew();
                uow.Disposing += OnUowDisposing;

                UowStack.Push(uow);
            }

            return uow;
        }

        private void OnUowDisposing(object sender, EventArgs e)
        {
            TUnitOfWork top = GetTop();

            if (ReferenceEquals(top, sender))
            {
                top.Disposing -= OnUowDisposing;
                UowStack.Pop();
            }
        }

        private TUnitOfWork GetTop()
        {
            TUnitOfWork uow = (UowStack.Count > 0) ? UowStack.Peek() : null;
            if ((uow != null) && uow.IsDisposed)
            {
                UowStack.Pop();
            }

            return uow;
        }

        /// <summary>
        /// Starts a new unit-of-work that tracks changes made the entities obtained through the associated repositories.
        /// </summary>
        /// <returns>
        /// Always creates a new unit-of-work, regardless of an existing one that is associated with the current thread.
        /// </returns>
        protected abstract TUnitOfWork CreateNew();

        private Stack<TUnitOfWork> UowStack
        {
            get
            {
                Stack<TUnitOfWork> stack = State;
                if (stack == null)
                {
                    State = stack = new Stack<TUnitOfWork>();
                }

                return stack;
            }
        }

        private Stack<TUnitOfWork> State
        {
            get
            {
                // See http://piers7.blogspot.com/2005/11/threadstatic-callcontext-and_02.html
                if (HttpContext.Current != null)
                {
                    return HttpContext.Current.Items[ContextKey] as Stack<TUnitOfWork>;
                }

                return CallContext.GetData(ContextKey) as Stack<TUnitOfWork>;
            }
            set
            {
                if (HttpContext.Current != null)
                {
                    HttpContext.Current.Items[ContextKey] = value;
                }
                else
                {
                    CallContext.SetData(ContextKey, value);
                }
            }
        }
    }
}
