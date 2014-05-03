using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynoDL8
{
    public static class ArgumentValidation
    {
        public static T ThrowIfNull<T>(this T source, string name) where T : class
        {
            if (source == null)
            {
                throw new ArgumentNullException(name);
            }

            return source;
        }
    }
}
