using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Framework
{
    public class HashCalculator
    {
        public static int Calculate(long Value1, long Value2)
        {
            return (int)(Value1.GetHashCode() ^ RotateLeft((uint)Value2.GetHashCode(), 16));
        }
        private static uint RotateLeft(uint value, int count)
        {
            return (value << count) | (value >> (32 - count));
        }
    }
}
