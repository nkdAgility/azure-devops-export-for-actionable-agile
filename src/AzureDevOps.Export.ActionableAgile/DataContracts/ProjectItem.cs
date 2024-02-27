using AzureDevOps.Export.ActionableAgile.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOps.Export.ActionableAgile.DataContracts
{

    public class ProjectItem
    {
        public string id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public string state { get; set; }
        public int revision { get; set; }
        public ProjectItem_Links _links { get; set; }
        public string visibility { get; set; }
        public Defaultteam defaultTeam { get; set; }
        public DateTime lastUpdateTime { get; set; }
    }

    public class ProjectItem_Links
    {
        public ProjectItem_Self self { get; set; }
        public ProjectItem_Collection collection { get; set; }
        public ProjectItem_Web web { get; set; }
    }

    public class ProjectItem_Self
    {
        public string href { get; set; }
    }

    public class ProjectItem_Collection
    {
        public string href { get; set; }
    }

    public class ProjectItem_Web
    {
        public string href { get; set; }
    }

    public class Defaultteam
    {
        public string id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
    }
    public class ProjectItems
    {
        public int count { get; set; }
        public ProjectItem[] value { get; set; }
    }


}


