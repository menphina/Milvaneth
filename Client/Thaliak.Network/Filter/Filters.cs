using System.Collections.Generic;
using System.Linq;
using Thaliak.Network.Analyzer;

namespace Thaliak.Network.Filter
{
    public class Filters<T>
    {
        public FilterOperator FilterOperator { get; set; }
        public IList<PropertyFilter<T>> PropertyFilters { get; set; }

        public Filters(FilterOperator filterFilterOperator)
        {
            this.FilterOperator = filterFilterOperator;
            this.PropertyFilters = new List<PropertyFilter<T>>();
        }

        public bool IsMatch(T obj)
        {
            return this.FilterOperator == FilterOperator.AND
                ? this.PropertyFilters.All(x => x.IsMatch(obj))
                : this.PropertyFilters.Any(x => x.IsMatch(obj));
        }

        public List<MessageAttribute> WhichMatch(T obj)
        {
            var tmp = new List<MessageAttribute>(this.PropertyFilters.Count);

            if (this.FilterOperator == FilterOperator.AND)
            {
                if (!this.PropertyFilters.All(x => x.IsMatch(obj))) return tmp;
                tmp.AddRange(this.PropertyFilters.Select(x => x.Label));
            }
            else
            {
                var item = this.PropertyFilters.FirstOrDefault(x => x.IsMatch(obj));

                if(item != null)
                    tmp.Add(item.Label);
            }

            return tmp;
        }

        public MessageAttribute FirstMatch(T obj)
        {
            return this.PropertyFilters.FirstOrDefault(x => x.IsMatch(obj))?.Label ?? MessageAttribute.NoMatch;
        }
    }
}