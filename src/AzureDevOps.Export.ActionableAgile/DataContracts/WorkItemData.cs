using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOps.Export.ActionableAgile.DataContracts
{
   public class WorkItemDataParent : WorkItemData
    {
        public List<WorkItemData>? Revisions { get;  set; }
        public dynamic WorkItemType { get;  set; }
    }
    public class WorkItemData
    {
        public int Id { get;  set; }
        public int Rev { get;  set; }
        public string? Tags { get;  set; }
        public string? Title { get;  set; }
        public DateTime ChangedDate { get;  set; }
        public string? ColumnField { get;  set; }
        public string? RowField { get;  set; }
        public bool? DoneField { get;  set; }
    }
}
