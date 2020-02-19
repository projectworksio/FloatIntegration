using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FloatIntegration
{
    public class Models
    {
        public class FloatTask
        {
            public int task_id { get; set; }
            public int project_id { get; set; }
            public DateTime start_date { get; set; }
            public DateTime end_date { get; set; }
            public string start_time { get; set; }
            public decimal hours { get; set; }
            public int people_id { get; set; }
            public int status { get; set; }
            public int priority { get; set; }
            public string name { get; set; }
            public string notes { get; set; }
            public int repeat_state { get; set; }
            public string repeat_end_date { get; set; }
            public int created_by { get; set; }
            public DateTime created { get; set; }
            public int modified_by { get; set; }
            public DateTime modified { get; set; }
        }

        public class FloatProject
        {
            public int project_id { get; set; }
            public List<FloatPerson> people { get; set; }
    }

        public class FloatPerson
        {
            public int person_id { get; set; }
            public List<FloatTask> tasks { get; set; }

        }

        public class Resource
        {
            public int project_id { get; set; }
            public int person_id { get; set; }
            public DateTime StartDate { get; set; }
            public decimal Hours { get; set; }
        }
    }
}
