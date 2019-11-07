using System;
using System.Collections.Generic;

namespace Vertica.Integration.Infrastructure.Extensions
{
    internal static class QueueExtensions
    {
        public static T SafePeek<T>(this Queue<T> queue, T valueIfEmpty = default(T))
        {
            if (queue == null) throw new ArgumentNullException(nameof(queue));

            return queue.Count > 0 ? queue.Peek() : valueIfEmpty;
        }
    }
}