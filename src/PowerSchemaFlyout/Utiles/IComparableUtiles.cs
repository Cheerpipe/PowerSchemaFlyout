
using System;

namespace PowerSchemaFlyout.Utiles
{
    public static class IComparableUtiles
    {
        public static T Min<T>(T a, T b) where T : IComparable
        {
            return a.CompareTo(b) <= 0 ? a : b;
        }

        public static T Max<T>(T a, T b) where T : IComparable
        {
            return a.CompareTo(b) >= 0 ? a : b;
        }
    }
}
