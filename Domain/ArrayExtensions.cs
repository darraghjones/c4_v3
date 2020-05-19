using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Domain
{
    static class ArrayExtensions
    {
        public static T[][] DeepClone<T>(this T[][] array)
        {
            return array.Select(x => x.ToArray()).ToArray();
        }
    }
}
