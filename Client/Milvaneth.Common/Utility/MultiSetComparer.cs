using System;
using System.Collections.Generic;
using System.Linq;

namespace Milvaneth.Common
{
    // https://stackoverflow.com/questions/50098/comparing-two-collections-for-equality-irrespective-of-the-order-of-items-in-the
    public class MultiSetComparer<T> : IEqualityComparer<IEnumerable<T>>
    {
        private readonly IEqualityComparer<T> _comparer;
        public MultiSetComparer(IEqualityComparer<T> comparer = null)
        {
            _comparer = comparer ?? EqualityComparer<T>.Default;
        }

        public bool Equals(IEnumerable<T> first, IEnumerable<T> second)
        {
            if (first == null)
                return second == null;

            if (second == null)
                return false;

            if (ReferenceEquals(first, second))
                return true;

            if (first is ICollection<T> firstCollection && second is ICollection<T> secondCollection)
            {
                if (firstCollection.Count != secondCollection.Count)
                    return false;

                if (firstCollection.Count == 0)
                    return true;
            }

            return !HaveMismatchedElement(first, second);
        }

        private bool HaveMismatchedElement(IEnumerable<T> first, IEnumerable<T> second)
        {
            var firstElementCounts = GetElementCounts(first, out var firstNullCount);
            var secondElementCounts = GetElementCounts(second, out var secondNullCount);

            if (firstNullCount != secondNullCount || firstElementCounts.Count != secondElementCounts.Count)
                return true;

            foreach (var kvp in firstElementCounts)
            {
                var firstElementCount = kvp.Value;
                secondElementCounts.TryGetValue(kvp.Key, out var secondElementCount);

                if (firstElementCount != secondElementCount)
                    return true;
            }

            return false;
        }

        private Dictionary<T, int> GetElementCounts(IEnumerable<T> enumerable, out int nullCount)
        {
            var dictionary = new Dictionary<T, int>(_comparer);
            nullCount = 0;

            foreach (var element in enumerable)
            {
                if (element == null)
                {
                    nullCount++;
                }
                else
                {
                    dictionary.TryGetValue(element, out var num);
                    num++;
                    dictionary[element] = num;
                }
            }

            return dictionary;
        }

        public int GetHashCode(IEnumerable<T> enumerable)
        {
            if (enumerable == null) throw new ArgumentNullException(nameof(enumerable));

            return enumerable.OrderBy(x => x).Aggregate(17, (current, val) => current * 23 + (val?.GetHashCode() ?? 42));
        }
    }
}
