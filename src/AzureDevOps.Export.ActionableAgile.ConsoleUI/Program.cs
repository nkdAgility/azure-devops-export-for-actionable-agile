using AzureDevOps.Export.ActionableAgile.ConsoleUI.DataContracts;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System.Dynamic;
using CsvHelper;
using System.Xml.Linq;

namespace AzureDevOps.Export.ActionableAgile.ConsoleUI
{
    internal class Program
    {

        static async Task<int> Main(string[] args)
        {
            var tokenOption = new Option<String?>("--token", "The auth token to use.");
            var organizationUrlOption = new Option<String?>("--org", "The Organisation to connect to.");
            var projectNameOption = new Option<String?>("--project", "The Organisation to connect to.");
            var teamNameOption = new Option<String?>("--team", "The Organisation to connect to.");
            var boardNameOption = new Option<String?>("--board", "The Organisation to connect to.");

            var rootCommand = new RootCommand("Sample app for System.CommandLine");
            rootCommand.Add(organizationUrlOption);
            rootCommand.Add(tokenOption);
            rootCommand.Add(teamNameOption);
            rootCommand.Add(projectNameOption);
            rootCommand.Add(boardNameOption);


            rootCommand.SetHandler(async (token, azureDevOpsOrganizationUrl, projectName, teamName, boardName) =>
            {
                ExecuteCommand(token, azureDevOpsOrganizationUrl, projectName, teamName, boardName);
            }, tokenOption, organizationUrlOption, projectNameOption, teamNameOption, boardNameOption);

            return await rootCommand.InvokeAsync(args);

        }

        private static void ExecuteCommand(string token, string azureDevOpsOrganizationUrl, string projectName, string teamName, string boardName)
        {
            var authTool = new Authenticator();
            var authHeader = authTool.AuthenticationCommand(token).Result;
            ProjectItem? projectItem = null;
            TeamItem? teamItem = null;
            BoardItem? boardItem = null;
            BacklogItem? backlogItem = null;

            WriteCurrentStatus(azureDevOpsOrganizationUrl, projectItem, teamItem, boardItem, backlogItem);
            projectItem = GetProjectName(authHeader, azureDevOpsOrganizationUrl, projectName);
            WriteCurrentStatus(azureDevOpsOrganizationUrl, projectItem, teamItem, boardItem, backlogItem);
            teamItem = GetTeamName(authHeader, azureDevOpsOrganizationUrl, projectItem, teamName);
            WriteCurrentStatus(azureDevOpsOrganizationUrl, projectItem, teamItem, boardItem, backlogItem);
            boardItem = GetBoardName(authHeader, azureDevOpsOrganizationUrl, projectItem, teamItem, boardName);
            WriteCurrentStatus(azureDevOpsOrganizationUrl, projectItem, teamItem, boardItem, backlogItem);
            backlogItem = GetBacklogName(authHeader, azureDevOpsOrganizationUrl, projectItem, teamItem, boardItem.name);
            WriteCurrentStatus(azureDevOpsOrganizationUrl, projectItem, teamItem, boardItem, backlogItem);

            ExportData(authHeader, azureDevOpsOrganizationUrl, projectItem, teamItem, boardItem, backlogItem);
        }

        private static BacklogItem? GetBacklogName(string authHeader, string azureDevOpsOrganizationUrl, ProjectItem projectItem, TeamItem teamItem, string backlogName)
        {
            BacklogItem? backlogItem = null;
            //GET https://dev.azure.com/{organization}/{project}/{team}/_apis/work/backlogs?api-version=7.2-preview.1
            string apiCallUrl = $"{azureDevOpsOrganizationUrl}/{projectItem.id}/{teamItem.id}/_apis/work/backlogs?api-version=7.2-preview.1";
            var result = GetResult(authHeader, apiCallUrl);
            var backlogItems = JsonConvert.DeserializeObject<BacklogItems>(result);

            backlogItem = backlogItems.value.SingleOrDefault(pi => pi.name == backlogName);

            if (backlogItem == null)
            {
                CommandLineChooser chooser = new CommandLineChooser("Backlogs");
                foreach (BacklogItem bi in backlogItems.value)
                {
                    chooser.Add(new CommandLineChoice(bi.name, bi.id));
                }
                backlogItem = backlogItems.value.SingleOrDefault(pi => pi.id == chooser.Choose()?.Id);
            }
            return backlogItem;
        }

        private static void WriteCurrentStatus(string azureDevOpsOrganizationUrl, ProjectItem? projectItem, TeamItem? teamItem, BoardItem boardItem, BacklogItem backlogItem)
        {
            Console.Clear();
            Console.WriteLine("Azure DevOps Export for Actionable Agile");
            Console.WriteLine("================");
            Console.WriteLine($"Org: {azureDevOpsOrganizationUrl}");
            Console.WriteLine($"Project: {projectItem?.name} // {projectItem?.id}");
            Console.WriteLine($"Team: {teamItem?.name} // {teamItem?.id}");
            Console.WriteLine($"Board: {boardItem?.name} // {boardItem?.id}");
            Console.WriteLine($"Backlog: {backlogItem?.name} // {backlogItem?.id}");
            Console.WriteLine("================");

        }


        private static BoardItem GetBoardName(string authHeader, string azureDevOpsOrganizationUrl, ProjectItem projectItem, TeamItem teamItem, string boardName)
        {
            BoardItem? boardItem = null;
            if (boardName != null)
            {

                //GET https://dev.azure.com/{organization}/{project}/{team}/_apis/work/boards/{id}?api-version=7.2-preview.1
                string apiGetSingle = $"{azureDevOpsOrganizationUrl}/{projectItem.id}/{teamItem.id}/_apis/work/boards/{boardName}?api-version=7.2-preview.1";
                var singleResult = GetResult(authHeader, apiGetSingle);
                if (string.IsNullOrEmpty(singleResult))
                {
                    boardItem = null;
                }
                else
                {
                    boardItem = JsonConvert.DeserializeObject<BoardItem>(singleResult);
                }
            }
            if (boardItem == null)
            {
                //GET https://dev.azure.com/{organization}/{project}/{team}/_apis/work/boards?api-version=7.2-preview.1
                string apiCallUrl = $"{azureDevOpsOrganizationUrl}/{projectItem.id}/{teamItem.id}/_apis/work/boards?api-version=7.2-preview.1";
                var result = GetResult(authHeader, apiCallUrl);

                var boardItems = JsonConvert.DeserializeObject<BoardItems>(result);

                CommandLineChooser chooser = new CommandLineChooser("Boards");
                foreach (BoardItem bi in boardItems.value)
                {
                    chooser.Add(new CommandLineChoice(bi.name, bi.id));
                }
                boardItem = boardItems.value.SingleOrDefault(pi => pi.name == chooser.Choose()?.Name);
            }
            return boardItem;
        }

        private static TeamItem GetTeamName(string authHeader, string azureDevOpsOrganizationUrl, ProjectItem projectItem, string teamName)
        {
            TeamItem? teamItem = null;
            if (teamName != null)
            {

                //GET https://dev.azure.com/{organization}/_apis/projects/{projectId}/teams/{teamId}?api-version=7.2-preview.3
                string apiGetSingle = $"{azureDevOpsOrganizationUrl}/_apis/projects/{projectItem.id}/teams/{teamName}?api-version=7.2-preview.3";
                var singleResult = GetResult(authHeader, apiGetSingle);
                if (string.IsNullOrEmpty(singleResult))
                {
                    teamItem = null;
                }
                else
                {
                    teamItem = JsonConvert.DeserializeObject<TeamItem>(singleResult);
                }
            }
            if (teamItem == null)
            {
                //GET https://dev.azure.com/{organization}/_apis/projects/{projectId}/teams?api-version=7.2-preview.3
                string apiCallUrl = $"{azureDevOpsOrganizationUrl}/_apis/projects/{projectItem.id}/teams?api-version=7.2-preview.3";
                var result = GetResult(authHeader, apiCallUrl);

                var teamItems = JsonConvert.DeserializeObject<TeamItems>(result);

                CommandLineChooser chooser = new CommandLineChooser("Team");
                foreach (TeamItem ti in teamItems.value)
                {
                    chooser.Add(new CommandLineChoice(ti.name, ti.id));
                }
                return teamItems.value.SingleOrDefault(pi => pi.name == chooser.Choose()?.Name);

            }
            return teamItem;
        }

        private static ProjectItem? GetProjectName(string authHeader, string azureDevOpsOrganizationUrl, string? projectName)
        {
            ProjectItem projectItem = null;
            if (projectName != null)
            {
                string apiGetProject = $"{azureDevOpsOrganizationUrl}/_apis/projects/{projectName}?api-version=7.2-preview.4";
                var projectResult = GetResult(authHeader, apiGetProject);
                if (string.IsNullOrEmpty(projectResult))
                {
                    projectItem = null;
                }
                else
                {
                    projectItem = JsonConvert.DeserializeObject<ProjectItem>(projectResult);
                }
            }
            if (projectItem == null)
            {
                string apiCallUrl = $"{azureDevOpsOrganizationUrl}/_apis/projects?stateFilter=All&api-version=2.2";
                var result = GetResult(authHeader, apiCallUrl);

                var projectItems = JsonConvert.DeserializeObject<ProjectItems>(result);

                CommandLineChooser chooser = new CommandLineChooser("Project");
                foreach (ProjectItem pi in projectItems.value)
                {
                    chooser.Add(new CommandLineChoice(pi.name, pi.id));
                }
                return projectItems.value.SingleOrDefault(pi => pi.name == chooser.Choose()?.Name);

            }
            return projectItem;
        }

        private static void ExportData(string authHeader, string azureDevOpsOrganizationUrl, ProjectItem projectItem, TeamItem teamItem, BoardItem boardItem, BacklogItem backlogItem)
        {


            /// get Work Items From Boards
            string apiCallUrlWi = $"{azureDevOpsOrganizationUrl}/{projectItem.id}/{teamItem.id}/_apis/work/backlogs/{backlogItem.id}/workItems?api-version=7.2-preview.1";
            var resultWi = GetResult(authHeader, apiCallUrlWi);
            WorkItems workItems = JsonConvert.DeserializeObject<WorkItems>(resultWi);

            string fields = $"{boardItem.fields.columnField.referenceName},{boardItem.fields.rowField.referenceName},{boardItem.fields.doneField.referenceName}";

            var recordsCSV = new List<dynamic>();
            foreach (WorkItemElement wi in workItems.workItems)
            {
                WorkItemDataParent witData = GetWorkItemData(authHeader, azureDevOpsOrganizationUrl, projectItem, boardItem, fields, wi);
                Console.WriteLine($"Loaded {witData.Revisions.Count} revisions for {witData.Id}");


                var recordCsv = new ExpandoObject() as IDictionary<string, Object>;
                recordCsv.Add("Id", witData.Id);
                recordCsv.Add("Name", witData.Title);

                // Find highest date for each column...
                foreach (BoardItem_Column bic in boardItem.columns)
                {
                    var revsforColumn = witData.Revisions.Where(item => item.ColumnField == bic.name).Select(item => item);
                    if (revsforColumn.Count() > 0)
                    {

                        var finalForColumn = revsforColumn.Last();

                        recordCsv.Add(bic.name, finalForColumn.ChangedDate);

                        //Console.WriteLine($"{bic.name}:{witData.Id}={finalForColumn.ChangedDate.ToString()}");
                    }
                    else
                    {
                        recordCsv.Add(bic.name, null);
                        //Console.WriteLine($"{bic.name}:{witData.Id}=null");
                    }

                }

                recordCsv.Add("Team", teamItem.name);
                recordCsv.Add("Type", witData.WorkItemType);
                recordCsv.Add("BlockedDays", 0);
                recordCsv.Add("Labels", witData.Tags);
                recordsCSV.Add(recordCsv);
            }
            Console.WriteLine(workItems.workItems.ToList().Count);

            using (var writer = new StringWriter())
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(recordsCSV);

                Console.WriteLine(writer.ToString());
            }
        }

        private static WorkItemDataParent GetWorkItemData(string authHeader, string azureDevOpsOrganizationUrl, ProjectItem projectItem, BoardItem boardItem, string fields, WorkItemElement wi)
        {
            //GET https://dev.azure.com/{organization}/{project}/_apis/wit/workitems/{id}?$expand=fields&api-version=7.2-preview.3

            string apiCallUrlWiSingle = $"{azureDevOpsOrganizationUrl}/{projectItem.id}/_apis/wit/workitems/{wi.target.id}?$fields={fields}&api-version=7.2-preview.3";
            var single = GetResult(authHeader, apiCallUrlWiSingle);
            JObject jsondata = JObject.Parse(single);
            WorkItemDataParent witData = new WorkItemDataParent();
            witData.Id = (int)jsondata["id"];
            witData.Rev = (int)jsondata["rev"];
            witData.Title = (string)jsondata["fields"]["System.Title"];
            witData.ChangedDate = (DateTime)jsondata["fields"]["System.ChangedDate"];
            witData.Tags = (string)jsondata["fields"]["System.Tags"];
            witData.WorkItemType = (string)jsondata["fields"]["System.WorkItemType"];
            witData.Revisions = new List<WorkItemData>();

            //GET https://dev.azure.com/{organization}/{project}/_apis/wit/workItems/{id}/revisions?$top={$top}&$skip={$skip}&$expand={$expand}&api-version=7.2-preview.3
            string apiCallUrlWiRevisions = $"{azureDevOpsOrganizationUrl}/{projectItem.id}/_apis/wit/workitems/{wi.target.id}/revisions?$expand=fields&api-version=7.2-preview.3";
            var revs = GetResult(authHeader, apiCallUrlWiRevisions);
            JObject revdata = JObject.Parse(revs);

            var count = (int)revdata["count"];

            for (int i = 0; i < count; i++)
            {
                var jsonrev = revdata["value"][i];
                var witRevData = new WorkItemData();
                witRevData.Rev = (int)jsonrev["rev"];
                witRevData.Id = (int)jsonrev["id"];
                var jsonfields = revdata["value"][i]["fields"];
                witRevData.Title = (string)jsonfields["System.Title"];
                witRevData.ChangedDate = (DateTime)jsonfields["System.ChangedDate"];
                witRevData.Tags = (string)jsonfields["System.Tags"];
                witRevData.ColumnField = (string)jsonfields[boardItem.fields.columnField.referenceName];
                witRevData.RowField = (string)jsonfields[boardItem.fields.rowField.referenceName];
                witRevData.DoneField = (string)jsonfields[boardItem.fields.doneField.referenceName];
                witData.Revisions.Add(witRevData);
            }

            return witData;
        }

        private static string GetResult(string authHeader, string apiToCall)
        {


            // use the httpclient
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("User-Agent", "ManagedClientConsoleAppSample");
                client.DefaultRequestHeaders.Add("X-TFS-FedAuthRedirect", "Suppress");
                client.DefaultRequestHeaders.Add("Authorization", authHeader);

                // connect to the REST endpoint            
                HttpResponseMessage response = client.GetAsync(apiToCall).Result;

                // check to see if we have a succesfull respond
                if (response.IsSuccessStatusCode)
                {
                    return response.Content.ReadAsStringAsync().Result;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException();
                }
                else
                {
                    Console.WriteLine("Result::{0}:{1}", response.StatusCode, response.ReasonPhrase);
                }
            }
            return string.Empty;
        }

    }
}
