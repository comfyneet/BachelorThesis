using System;

namespace RiceDoctor.Shared
{
    public static class DoubleExtensions
    {
        public static double Tolerance = 0.001;

        public static bool Equals3DigitPrecision(this double left, double right)
        {
            return Math.Abs(left - right) < Tolerance;
        }
    }
}