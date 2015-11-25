using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartApero.Finders
{
    public abstract class Finder
    {
        public abstract string Resolve(string text);
    }
}
