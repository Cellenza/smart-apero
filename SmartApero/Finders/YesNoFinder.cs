using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartApero.Finders
{
    public class YesNoFinder : Finder
    {
        private string[] _yes = new[] { "oui", "yes", "bien sûr", "évidemment", "absolument", "certainement" };
        private string[] _no = new[] { "non", "nope", "pas", "négatif" };

        public override string Resolve(string text)
        {
            var words = text.Split(' ');

            foreach (var word in words)
            {
                if (_no.Contains(word))
                    return "0";
                if (_yes.Contains(word))
                    return "1";
            }

            return null;
        }
    }
}
