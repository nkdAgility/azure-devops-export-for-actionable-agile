using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOps.Export.ActionableAgile.DataContracts
{

    public class WorkItemStatesData
    {
        public int count { get; set; }
        public WorkItemStateData[] value { get; set; }
    }

    public class WorkItemStateData
    {
        public string name { get; set; }
    }

}
