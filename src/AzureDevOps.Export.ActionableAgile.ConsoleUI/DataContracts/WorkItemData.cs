using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOps.Export.ActionableAgile.ConsoleUI.DataContracts
{
    class WorkItemDataParent : WorkItemData
    {
        public List<WorkItemData>? Revisions { get; internal set; }
        public dynamic WorkItemType { get; internal set; }
    }
    class WorkItemData
    {
        public int Id { get; internal set; }
        public int Rev { get; internal set; }
        public string? Tags { get; internal set; }
        public string? Title { get; internal set; }
        public DateTime ChangedDate { get; internal set; }
        public string? ColumnField { get; internal set; }
        public string? RowField { get; internal set; }
        public string? DoneField { get; internal set; }
    }
}
