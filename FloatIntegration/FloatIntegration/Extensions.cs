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


// Grouped Tasks
//var projects = tasksToAggregate
//    .GroupBy(t => t.project_id)
//    .Select(project => 
//    {
//        var peepsForProject = tasksToAggregate
//            .Where(t => t.project_id == project.Key)
//            .GroupBy(t => t.people_id)
//            .Select(person => 
//            {
//                var tasksForPerson = tasksToAggregate
//                    .Where(t => t.project_id == project.Key && t.people_id == person.Key)
//                    .ToList();

//                return new FloatPerson
//                {
//                    person_id = person.Key,
//                    tasks = tasksForPerson
//                };
//            })
//            .ToList();

//        return new FloatProject
//        { 
//            project_id = project.Key, 
//            people = peepsForProject
//        };
//    })
//    .ToList();



// Projects
//IEnumerable<FloatProject> floatProjects = mytasks.Select(x => new FloatProject() { project_id = x.project_id }).DistinctBy(x => x.project_id);