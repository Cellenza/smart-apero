using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartApero.Finders
{
    public class ThemeFinder : Finder
    {
        private string[] _dinatoire = new[] { "dinatoire", "dînatoire" };
        private string[] _classique = new[] { "classique" };

        public override string Resolve(string text)
        {
            var words = text.Split(' ');

            foreach (var word in words)
            {
                if (_dinatoire.Contains(word))
                    return "0";
                if (_classique.Contains(word))
                    return "1";
            }

            return null;
        }
    }
}
