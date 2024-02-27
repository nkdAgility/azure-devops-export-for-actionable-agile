using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOps.Export.ActionableAgile.DataContracts
{

    public class OrgItems
    {
        public int count { get; set; }
        public OrgItem[] value { get; set; }
    }

    public class OrgItem
    {
        public string accountId { get; set; }
        public string accountUri { get; set; }
        public string accountName { get; set; }
        public OrgItem_Properties properties { get; set; }
    }

    public class OrgItem_Properties
    {
    }



}
