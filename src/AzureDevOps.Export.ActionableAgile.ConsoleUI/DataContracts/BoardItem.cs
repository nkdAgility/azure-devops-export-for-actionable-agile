using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOps.Export.ActionableAgile.ConsoleUI.DataContracts
{

    public class BoardItems
    {
        public int count { get; set; }
        public BoardItem[] value { get; set; }
    }

    public class BoardItem
    {
        public string id { get; set; }
        public string url { get; set; }
        public string name { get; set; }
        public int revision { get; set; }
        public BoardItem_Column[] columns { get; set; }
        public BoardItem_Row[] rows { get; set; }
        public bool isValid { get; set; }
        public BoardItem_Allowedmappings allowedMappings { get; set; }
        public bool canEdit { get; set; }
        public BoardItem_Fields fields { get; set; }
        public BoardItem_Links _links { get; set; }
    }

    public class BoardItem_Allowedmappings
    {
        public BoardItem_Incoming Incoming { get; set; }
        public BoardItem_Inprogress InProgress { get; set; }
        public BoardItem_Outgoing Outgoing { get; set; }
    }

    public class BoardItem_Incoming
    {
        public string[] Feature { get; set; }
        public string[] Application { get; set; }
    }

    public class BoardItem_Inprogress
    {
        public string[] Feature { get; set; }
        public string[] Application { get; set; }
    }

    public class BoardItem_Outgoing
    {
        public string[] Feature { get; set; }
        public string[] Application { get; set; }
    }

    public class BoardItem_Fields
    {
        public BoardItem_Columnfield columnField { get; set; }
        public BoardItem_Rowfield rowField { get; set; }
        public BoardItem_Donefield doneField { get; set; }
    }

    public class BoardItem_Columnfield
    {
        public string referenceName { get; set; }
        public string url { get; set; }
    }

    public class BoardItem_Rowfield
    {
        public string referenceName { get; set; }
        public string url { get; set; }
    }

    public class BoardItem_Donefield
    {
        public string referenceName { get; set; }
        public string url { get; set; }
    }

    public class BoardItem_Links
    {
        public BoardItem_Self self { get; set; }
        public BoardItem_Project project { get; set; }
        public BoardItem_Team team { get; set; }
        public BoardItem_Charts charts { get; set; }
        public BoardItem_Columns columns { get; set; }
        public BoardItem_Rows rows { get; set; }
    }

    public class BoardItem_Self
    {
        public string href { get; set; }
    }

    public class BoardItem_Project
    {
        public string href { get; set; }
    }

    public class BoardItem_Team
    {
        public string href { get; set; }
    }

    public class BoardItem_Charts
    {
        public string href { get; set; }
    }

    public class BoardItem_Columns
    {
        public string href { get; set; }
    }

    public class BoardItem_Rows
    {
        public string href { get; set; }
    }

    public class BoardItem_Column
    {
        public string id { get; set; }
        public string name { get; set; }
        public int itemLimit { get; set; }
        public BoardItem_Statemappings stateMappings { get; set; }
        public string columnType { get; set; }
        public bool isSplit { get; set; }
        public string description { get; set; }
    }

    public class BoardItem_Statemappings
    {
        public string Application { get; set; }
        public string Feature { get; set; }
    }

    public class BoardItem_Row
    {
        public string id { get; set; }
        public object name { get; set; }
        public object color { get; set; }
    }


}
