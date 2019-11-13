// Modifications copyright (C) 2019 Menphina

using System;
using Milvaneth.Common;
using Thaliak.Network.Analyzer;

namespace Thaliak.Network.Filter
{
    public class PropertyFilter<TValue>
    {
        public Func<TValue, dynamic> Property { get; set; }
        public dynamic Value { get; set; }
        public MessageAttribute Label { get; set; }

        public PropertyFilter(Func<TValue, dynamic> property, dynamic value, MessageAttribute label)
        {
            this.Property = property;
            this.Value = value;
            this.Label = label;
        }

        public bool IsMatch(TValue obj)
        {
            return this.Property(obj).Equals(this.Value);
        }
    }
}