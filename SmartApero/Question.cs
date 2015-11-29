using SmartApero.Finders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartApero
{
    public class Question
    {
        public string Key { get; set; }
        public string AssociatedMark { get; set; }

        private object _value;
        public object Value
        {
            get
            { return _value == null ? DefaultValue : _value; }
            set { _value = value; }
        }

        public object DefaultValue { get; set; }

        public bool HasBeenAsked { get; set; }

        private Finder _finder = new GenericFinder();
        public Finder Finder { get { return _finder; } set { _finder = value; } }
    }
}
