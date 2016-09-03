using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Framework.Collections
{
    public static class Extensions
    {
        public static T[] Populate<T>(this T[] Arr, T Value)
        {
            for (int i = 0; i < Arr.Length; i++)
            {
                Arr[i] = Value;
            }
            return Arr;
        }
    }
}
