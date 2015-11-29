using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartApero.Finders
{
    public class PersonsFinder : Finder
    {
        private Dictionary<int, string> Numbers = new Dictionary<int, string>();

        public PersonsFinder()
        {
            Numbers = new Dictionary<int, string>();
            Numbers.Add(1, "un");
            Numbers.Add(2, "deux");
            Numbers.Add(3, "trois");
            Numbers.Add(4, "quatre");
            Numbers.Add(5, "cinq");
            Numbers.Add(6, "six");
            Numbers.Add(7, "sept");
            Numbers.Add(8, "huit");
            Numbers.Add(9, "neuf");
        }

        public override string Resolve(string text)
        {
            return RetrieveNumber(text);
        }

        private string RetrieveNumber(string text)
        {
            var words = text.Split(' ');

            if (words.Contains("entre") && words.Contains("et"))
            {
                var n1 = RetrieveNumber(text.Split(new[] { "entre" }, StringSplitOptions.None)[1]);
                var n2 = RetrieveNumber(text.Split(new[] { "et" }, StringSplitOptions.None)[1]);

                var n = Math.Round((decimal)((int.Parse(n1) + int.Parse(n2)) / 2));

                if (n == 15)
                    return "une quinzaine de";

                return "environ " + n.ToString();
            }

            // Find number
            for (int i = words.Length - 1; i >=0; i--)
            {
                int res = 0;
                if (int.TryParse(words[i], out res))
                {
                    return res.ToString();
                }
            }

            // If not found, find the number in letter
            for (int i = words.Length - 1; i >=0; i--)
            //for (int i = 0; i < words.Length; i++)
            {
                if (Numbers.ContainsValue(words[i]))
                {
                    var number = Numbers.First(e => e.Value == words[i]);
                    return number.Key.ToString();
                }
            }

            return null;
        }
    }
}
