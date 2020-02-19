using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FloatIntegration
{
    public static class Extensions
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
    (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static DateTime StartOfWeek(this DateTime date, DayOfWeek dayOfWeek)
        {
            // Sunday = 0, ... , Saturday = 6
            int diff = dayOfWeek - date.DayOfWeek;
            if (diff > 0) diff -= 7;
            return date.AddDays(diff).Date;
        }
    }
}
