using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOps.Export.ActionableAgile.ConsoleUI.DataContracts
{

    public class BacklogItems
    {
        public int count { get; set; }
        public BacklogItem[] value { get; set; }
    }

    public class BacklogItem
    {
        public string id { get; set; }
        public string name { get; set; }
        public int rank { get; set; }
        public int workItemCountLimit { get; set; }
        public BacklogItem_Addpanelfield[] addPanelFields { get; set; }
        public BacklogItem_Columnfield[] columnFields { get; set; }
        public BacklogItem_Workitemtype[] workItemTypes { get; set; }
        public BacklogItem_Defaultworkitemtype defaultWorkItemType { get; set; }
        public string color { get; set; }
        public bool isHidden { get; set; }
        public string type { get; set; }
    }

    public class BacklogItem_Defaultworkitemtype
    {
        public string name { get; set; }
        public string url { get; set; }
    }

    public class BacklogItem_Addpanelfield
    {
        public string referenceName { get; set; }
        public string name { get; set; }
        public string url { get; set; }
    }

    public class BacklogItem_Columnfield
    {
        public BacklogItem_Columnfieldreference columnFieldReference { get; set; }
        public int width { get; set; }
    }

    public class BacklogItem_Columnfieldreference
    {
        public string referenceName { get; set; }
        public string name { get; set; }
        public string url { get; set; }
    }

    public class BacklogItem_Workitemtype
    {
        public string name { get; set; }
        public string url { get; set; }
    }

}
