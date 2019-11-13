using System;
using System.Collections.Generic;
using System.Linq;

namespace Thaliak.Network.Utilities
{
    public class ConsumerSearcher
    {
        public static IEnumerable<Type> FindConsumers(string @namespace)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(t => t.GetTypes())
                .Where(t => t.IsClass && t.Namespace == @namespace && t.IsSubclassOf(typeof(NetworkMessageProcessor)));
        }
    }
}
