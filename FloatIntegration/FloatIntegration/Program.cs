using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Serializers.NewtonsoftJson;
using static FloatIntegration.Models;

namespace FloatIntegration
{
    class Program
    {
        static void Main(string[] args)
        {
            // Connect to API
            var client = new RestClient(ConfigurationManager.AppSettings["Float.API"]);
            client.AddDefaultHeader("Authorization", string.Format("Bearer {0}", ConfigurationManager.AppSettings["Float.AuthKey"]));
            client.UseNewtonsoftJson();
            var request = new RestRequest(ConfigurationManager.AppSettings["Float.TaskEndpoint"], DataFormat.Json);
            var response = client.Get(request);

            // Current tasks from Float
            var currentTasks = Newtonsoft.Json.JsonConvert.DeserializeObject<List<FloatTask>>(response.Content);

            // Load known tasks if we have any
            var knownTasks = LoadKnownTasks();
            
            // Build a list of any tasks that are new or have changed
            var newAndUpdatedTasks = currentTasks
                .Select(x => new
                {
                    task_id = x.task_id,
                    project_id = x.project_id,
                    people_id = x.people_id,
                    start_date = x.start_date,
                    end_date = x.end_date,
                    hours = x.hours

                })
                .Except(knownTasks
                    .Select(x => new
                    {
                        task_id = x.task_id,
                        project_id = x.project_id,
                        people_id = x.people_id,
                        start_date = x.start_date,
                        end_date = x.end_date,
                        hours = x.hours
                    }));

            // Build list of deleted tasks (or where the person/project has changed) and zero out the hours
            var deletedTaskIDs = knownTasks
                .Select(x => new
                {
                    x.task_id,
                    x.project_id,
                    x.people_id
                })
                .Distinct()
                .Except(currentTasks
                    .Select(x => new
                    {
                        x.task_id,
                        x.project_id,
                        x.people_id
                    })
                    .Distinct());
            var deletedTasks = knownTasks.Where(t => deletedTaskIDs.Select(x => x.task_id).Contains(t.task_id)).ToList();
            deletedTasks.Select(x => { x.hours = 0; return x; }).ToList();

            // Get modified projects (from the new/updated list plus any deleted tasks)
            var modifiedProjectsIDs = newAndUpdatedTasks.Select(x => x.project_id).ToList();
            modifiedProjectsIDs.AddRange(deletedTasks.Select(x => x.project_id).ToList());
            modifiedProjectsIDs = modifiedProjectsIDs.Distinct().ToList();

            // Get tasks where any task in the project has been changed
            var tasksToAggregate = currentTasks.Where(t => modifiedProjectsIDs.Contains(t.project_id)).ToList();

            // Add any deleted tasks to our list of changed tasks (with their zero'd out hours)
            tasksToAggregate.AddRange(deletedTasks);

            // Build date range for tasks
            if (tasksToAggregate.Any())
            {
                var minTaskDate = tasksToAggregate.Min(t => t.start_date);
                var maxTaskDate = tasksToAggregate.Max(t => t.end_date);
                var weekStartDate = minTaskDate.StartOfWeek(DayOfWeek.Monday);
                var weekEndDate = weekStartDate.AddDays(4);

                // Group tasks into Project & Person
                var groupedTasksToAggregate = tasksToAggregate
                    .GroupBy(t => new { t.project_id, t.people_id })
                    .Select(t => new
                    {
                        project_id = t.Key.project_id,
                        person_id = t.Key.people_id,

                        tasks = t.ToList()
                    })
                    .ToList();

                // List to store Projectworks resources
                var resources = new List<Resource>();

                // Loop through all weeks in the task range
                while (weekStartDate <= maxTaskDate.StartOfWeek(DayOfWeek.Monday).AddDays(6))
                {
                    var resourcesToAdd = groupedTasksToAggregate
                        .Select(p =>
                        {
                            // Get the hours for each task for the days they intersect the week
                            var tasksForWeek = p.tasks
                                .Where(t => t.start_date <= weekEndDate && t.end_date >= weekStartDate)
                                .Select(task =>
                                {
                                    var startDate = task.start_date > weekStartDate ? task.start_date : weekStartDate;
                                    var endDate = task.end_date > weekEndDate ? weekEndDate : task.end_date;
                                    var days = endDate.Date.Subtract(startDate.Date).Duration().Days + 1;
                                    var hours = (decimal)days * task.hours;

                                    return new
                                    {
                                        Hours = hours
                                    };
                                })
                                .ToList();

                            // Create a new resource record for this Project / Person / Date and the aggregated task hours
                            return new Resource
                            {
                                project_id = p.project_id,
                                person_id = p.person_id,
                                StartDate = weekStartDate,
                                Hours = tasksForWeek.Sum(h => h.Hours)
                            };
                        })
                        .ToList();

                    // Add the new resources to our master list to export
                    resources.AddRange(resourcesToAdd);

                    // Increment dates to the next week (Monday - Friday)
                    weekStartDate = weekStartDate.AddDays(7);
                    weekEndDate = weekStartDate.AddDays(4);
                }

                // Add our new resource records into Projectworks
                foreach (var resource in resources)
                {
                    // Log output
                    Console.WriteLine("Person ID:  " + resource.person_id);
                    Console.WriteLine("Project ID: " + resource.project_id);
                    Console.WriteLine("Date:       " + resource.StartDate);
                    Console.WriteLine("Hours:      " + resource.Hours);
                    Console.WriteLine("==============================================");

                    // TODO: Lookup Float PersonID & ProjectID 

                    // TODO: Add resource record to Projectworks
                }

                // On successful export save the known task list
                SaveKnownTasks(response.Content);
            }

            Console.WriteLine("Done.");
            Console.ReadLine();
        }

        public static List<FloatTask> LoadKnownTasks()
        {
            var knownTasks = new List<FloatTask>();
            if (File.Exists("lastrun.json"))
            {
                using (StreamReader r = new StreamReader("lastrun.json"))
                {
                    string json = r.ReadToEnd();
                    knownTasks = JsonConvert.DeserializeObject<List<FloatTask>>(json);
                }
            }
            return knownTasks;
        }

        public static void SaveKnownTasks(string response)
        {
            File.WriteAllText(@"lastrun.json", response);
        }
    }
}