using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOps.Export.ActionableAgile.DataContracts
{

    public class WorkItems
    {
        public WorkItemElement[] workItems { get; set; }
    }

    public class WorkItemElement
    {
        public object rel { get; set; }
        public object source { get; set; }
        public WorkItemElement_Target target { get; set; }
    }

    public class WorkItemElement_Target
    {
        public int id { get; set; }
    }


    public class WorkItemItem
    {
        public int id { get; set; }
        public int rev { get; set; }
        public Fields fields { get; set; }
        public _Links3 _links { get; set; }
        public string url { get; set; }
    }

    public class Fields
    {
        public string SystemAreaPath { get; set; }
        public string SystemTeamProject { get; set; }
        public string SystemIterationPath { get; set; }
        public string SystemWorkItemType { get; set; }
        public string SystemState { get; set; }
        public string SystemReason { get; set; }
        public SystemAssignedto SystemAssignedTo { get; set; }
        public DateTime SystemCreatedDate { get; set; }
        public SystemCreatedby SystemCreatedBy { get; set; }
        public DateTime SystemChangedDate { get; set; }
        public SystemChangedby SystemChangedBy { get; set; }
        public string SystemTitle { get; set; }
        public DateTime MicrosoftVSTSCommonStateChangeDate { get; set; }
        public int MicrosoftVSTSCommonPriority { get; set; }
        public string MicrosoftVSTSCMMIBlocked { get; set; }
        public string MicrosoftVSTSCommonTriage { get; set; }
        public string MicrosoftVSTSCMMITaskType { get; set; }
        public string MicrosoftVSTSCMMIRequiresReview { get; set; }
        public string MicrosoftVSTSCMMIRequiresTest { get; set; }
    }

    public class SystemAssignedto
    {
        public string displayName { get; set; }
        public string url { get; set; }
        public _Links _links { get; set; }
        public string id { get; set; }
        public string uniqueName { get; set; }
        public string imageUrl { get; set; }
        public string descriptor { get; set; }
    }

    public class _Links
    {
        public Avatar avatar { get; set; }
    }

    public class Avatar
    {
        public string href { get; set; }
    }

    public class SystemCreatedby
    {
        public string displayName { get; set; }
        public string url { get; set; }
        public _Links1 _links { get; set; }
        public string id { get; set; }
        public string uniqueName { get; set; }
        public string imageUrl { get; set; }
        public string descriptor { get; set; }
    }

    public class _Links1
    {
        public Avatar1 avatar { get; set; }
    }

    public class Avatar1
    {
        public string href { get; set; }
    }

    public class SystemChangedby
    {
        public string displayName { get; set; }
        public string url { get; set; }
        public _Links2 _links { get; set; }
        public string id { get; set; }
        public string uniqueName { get; set; }
        public string imageUrl { get; set; }
        public string descriptor { get; set; }
    }

    public class _Links2
    {
        public Avatar2 avatar { get; set; }
    }

    public class Avatar2
    {
        public string href { get; set; }
    }

    public class _Links3
    {
        public Self self { get; set; }
        public Workitemrevisions workItemRevisions { get; set; }
        public Parent parent { get; set; }
    }

    public class Self
    {
        public string href { get; set; }
    }

    public class Workitemrevisions
    {
        public string href { get; set; }
    }

    public class Parent
    {
        public string href { get; set; }
    }


}

