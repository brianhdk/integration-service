using System.Collections.Generic;

namespace Experiments.Website
{
    public interface IEnqueueTask
    {
        void Run(string taskName, params KeyValuePair<string, string>[] arguments);
    }
}