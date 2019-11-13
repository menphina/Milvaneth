using Milvaneth.Common;
using System;
using System.Threading.Tasks;
using Flurl;

namespace Milvaneth.Communication.Foundation
{
    internal class ApiDefinition<T> where T : class
    {
        public static VerbMethod Get;
        public static VerbMethod Delete;
        public static VerbMethod Post;
        public static VerbMethod Put;

        public ApiDefinition(HttpVerb verb, string route, bool requireTarget)
        {
            Verb = verb;
            Route = route;
            NeedTarget = requireTarget;
        }

        public HttpVerb Verb { get; }
        public string Route { get; }
        public bool NeedTarget { get; }

        public T Call(string target, T paramter)
        {
            if(NeedTarget && string.IsNullOrEmpty(target))
                throw new InvalidOperationException("Target not specified");

            var url = NeedTarget ? Route.AppendPathSegment(target).ToString() : Route;
            var input = Serializer<T>.Serialize(paramter);
            byte[] output;

            switch (Verb)
            {
                case HttpVerb.Get:
                    output = Get(url, input);
                    break;

                case HttpVerb.Delete:
                    output = Delete(url, input);
                    break;

                case HttpVerb.Put:
                    output = Put(url, input);
                    break;

                case HttpVerb.Post:
                    output = Post(url, input);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (output == null || output.Length == 0)
            {
                // no return data, treat as null
                // MsgPack null is byte[1]{192}
                return null;
            }

            return Serializer<T>.Deserialize(output);
        }
    }
}
