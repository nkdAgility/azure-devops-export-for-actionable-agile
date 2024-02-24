﻿using AzureDevOps.Export.ActionableAgile.ConsoleUI.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOps.Export.ActionableAgile.ConsoleUI.DataContracts
{

    public class ProjectItem
    {
        public string id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public string state { get; set; }
        public int revision { get; set; }
        public _Links _links { get; set; }
        public string visibility { get; set; }
        public Defaultteam defaultTeam { get; set; }
        public DateTime lastUpdateTime { get; set; }
    }

    public class _Links
    {
        public Self self { get; set; }
        public Collection collection { get; set; }
        public Web web { get; set; }
    }

    public class Self
    {
        public string href { get; set; }
    }

    public class Collection
    {
        public string href { get; set; }
    }

    public class Web
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


