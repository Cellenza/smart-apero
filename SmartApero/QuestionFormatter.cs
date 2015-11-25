using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartApero
{
    public class QuestionFormatter : IFormatProvider, ICustomFormatter
    {
        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (formatProvider != null)
            {
                ICustomFormatter fmt = formatProvider.GetFormat(this.GetType()) as ICustomFormatter;
                if (fmt != null) { return fmt.Format(format, this, formatProvider); }
            }

            Question question = null;
            if (arg is Question)
            {
                question = arg as Question;
            }
            else
            {
                var questions = arg as Question[];
                question = questions.First(e => e.Key == format);
            }

            if (question.Value != null)
                return question.Value.ToString();

            return string.Format(format, "{0}", arg);
        }

        public object GetFormat(Type formatType)
        {
            return (formatType == typeof(ICustomFormatter)) ? this : null;
        }
    }
}
