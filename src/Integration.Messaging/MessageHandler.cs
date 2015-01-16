using System;

namespace Integration.Messaging
{
	public abstract class MessageHandler<TMessage>
	{
		public virtual bool CanHandle(TMessage message)
		{
			return true;
		}

		public void Handle(TMessage message)
		{
			if (!CanHandle(message))
				throw new InvalidOperationException("Cannot handle the message at it's current state.");

			HandleMessage(message);
		}

		protected abstract void HandleMessage(TMessage message);
	}
}