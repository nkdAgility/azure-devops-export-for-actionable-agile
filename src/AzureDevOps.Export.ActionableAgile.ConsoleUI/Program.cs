using AzureDevOps.Export.ActionableAgile.DataContracts;
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
        public enum RenderMode
        {
            Board =1,
            Backlog =2,
            Project =4
        }
        
        static async Task<int> Main(string[] args)
        {
            var tokenOption = new Option<String?>("--token", "The auth token to use.");
            var organizationNameOption = new Option<String?>("--org", "the name of the organisation / account to connect to. If not provided you will be asked to select.");
            var projectNameOption = new Option<String?>("--project", "The name of the project to connect to. If not provided you will be asked to select.");
            var teamNameOption = new Option<String?>("--team", "The name of the team to connect to. If not provided you will be asked to select.");
            var boardNameOption = new Option<String?>("--board", "The name of the board/backlog to connect to. If not provided you will be asked to select.");
            boardNameOption.AddAlias("--backlog");

            var modeOption = new Option<RenderMode>("--mode", "Do you want to use board columns or backlog work item states, or the whole project");
            modeOption.SetDefaultValue(RenderMode.Board);
 
            var outputOption = new Option<String?>("--output", "Where to output the file to.") { IsRequired = true};


            var rootCommand = new RootCommand("The Azure DevOps Export for Actionable Agile allows you to export data from Azure DevOps for import into Actionable Agile's standalone analytics tool on https://https://analytics.actionableagile.com/. Use when you are unable to use the extension or already have a standalone licence.");
            rootCommand.Add(organizationNameOption);
            rootCommand.Add(tokenOption);
            rootCommand.Add(teamNameOption);
            rootCommand.Add(projectNameOption);
            rootCommand.Add(boardNameOption);
            rootCommand.Add(modeOption);
            rootCommand.Add(outputOption);


            rootCommand.SetHandler(async (token, organizationName, projectName, teamName, boardName, renderMode, output) =>
            {
                ExecuteCommand(token, organizationName, projectName, teamName, boardName, renderMode, output);
            }, tokenOption, organizationNameOption, projectNameOption, teamNameOption, boardNameOption, modeOption, outputOption);

            return await rootCommand.InvokeAsync(args);

        }

        private static void ExecuteCommand(string token, string organizationName, string projectName, string teamName, string boardName, RenderMode mode, string output)
        {
            
            AzureDevOpsApi api = new AzureDevOpsApi(token);

            // test 
            ProfileItem? profileItem = api.GetProfileItem().Result;
            if (profileItem == null)
            {
                Console.WriteLine("Unable to connect and get profile... check your authentication method... ");
                Environment.Exit(1);
                
            } else
            {
                Console.WriteLine($"Connected as {profileItem.displayName} ");
            }

            OrgItem? orgItem = null;
            ProjectItem? projectItem = null;
            TeamItem? teamItem = null;
            BoardItem? boardItem = null;
            BacklogItem? backlogItem = null;

            WriteCurrentStatus(orgItem, projectItem, teamItem, boardItem, backlogItem);
            orgItem = GetOrganisationsItem(api, profileItem, organizationName);

            api.SetOrganisation(orgItem);

            WriteCurrentStatus(orgItem, projectItem, teamItem, boardItem, backlogItem);
            projectItem = GetProjectName(api, projectName);
            WriteCurrentStatus(orgItem, projectItem, teamItem, boardItem, backlogItem);
            teamItem = GetTeamName(api, projectItem, teamName);
            WriteCurrentStatus(orgItem, projectItem, teamItem, boardItem, backlogItem);
            boardItem = GetBoardName(api, projectItem, teamItem, boardName);
            WriteCurrentStatus(orgItem, projectItem, teamItem, boardItem, backlogItem);
            backlogItem = GetBacklogName(api, projectItem, teamItem, boardItem.name);
            WriteCurrentStatus(orgItem, projectItem, teamItem, boardItem, backlogItem);


            List<dynamic> recordsCSV;
            switch (mode)
            {
                case RenderMode.Backlog:
                    recordsCSV = BuildCSVForBacklog(api, projectItem, teamItem, backlogItem);
                    break;
                case RenderMode.Board:
                    recordsCSV = BuildCSVForBoard(api, projectItem, teamItem, boardItem, backlogItem);
                    break;
                case RenderMode.Project:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }


            using (var writer = new StringWriter())
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(recordsCSV);
                var outPath = Path.Combine(output);
                Console.WriteLine(writer.ToString());
                System.IO.File.WriteAllText(outPath, writer.ToString());
                Console.WriteLine("-------------------");
                Console.WriteLine($"Saved to: {outPath}");
            }

        }

        private static OrgItem? GetOrganisationsItem(AzureDevOpsApi api, ProfileItem profileItem, string orgName)
        {
            OrgItem? orgItem = null;

            OrgItems orgItems = api.GetOrgs(profileItem).Result;

            if (orgName != null)
            {
                orgItem = orgItems.value.SingleOrDefault(pi => pi.accountName == orgName);
            }
            if (orgItem == null)
            {
                CommandLineChooser chooser = new CommandLineChooser("Organisations");
                foreach (OrgItem bi in orgItems.value)
                {
                    chooser.Add(new CommandLineChoice(bi.accountName, bi.accountUri));
                }
                CommandLineChoice choice = chooser.Choose();
                orgItem = orgItems.value.SingleOrDefault(pi => pi.accountName == choice?.Name);
            }
            return orgItem;
        }

        private static BacklogItem? GetBacklogName(AzureDevOpsApi api, ProjectItem projectItem, TeamItem teamItem, string backlogName)
        {
            BacklogItem? backlogItem = null;
            //string apiCallUrl = $"{azureDevOpsOrganizationUrl}/{projectItem.id}/{teamItem.id}/_apis/work/backlogs?api-version=7.2-preview.1";
            var backlogItems = api.GetBacklogs(projectItem, teamItem).Result;

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

        private static void WriteCurrentStatus(OrgItem? orgItem, ProjectItem? projectItem, TeamItem? teamItem, BoardItem boardItem, BacklogItem backlogItem)
        {
            Console.Clear();
            Console.WriteLine("Azure DevOps Export for Actionable Agile");
            Console.WriteLine("================");
            Console.WriteLine($"Org: {orgItem?.accountName} // {orgItem?.accountUri} | use --org {orgItem?.accountName}" );
            Console.WriteLine($"Project: {projectItem?.name} // {projectItem?.id}");
            Console.WriteLine($"Team: {teamItem?.name} // {teamItem?.id}");
            Console.WriteLine($"Board: {boardItem?.name} // {boardItem?.id}");
            Console.WriteLine($"Backlog: {backlogItem?.name} // {backlogItem?.id}");
            Console.WriteLine("================");

        }


        private static BoardItem GetBoardName(AzureDevOpsApi api, ProjectItem projectItem, TeamItem teamItem, string boardName)
        {
            BoardItem? boardItem = null;
            if (boardName != null)
            {
                boardItem = api.GetBoard(projectItem.id, teamItem.id, boardName).Result; 
            }
            if (boardItem == null)
            {

                var boardItems = api.GetBoards(projectItem.id, teamItem.id).Result;
                CommandLineChooser chooser = new CommandLineChooser("Boards");
                foreach (BoardItem bi in boardItems.value)
                {
                    chooser.Add(new CommandLineChoice(bi.name, bi.id));
                }
                CommandLineChoice choice = chooser.Choose();
                boardItem = boardItems.value.SingleOrDefault(pi => pi.name == choice?.Name);
            }
            return boardItem;
        }

        private static TeamItem GetTeamName(AzureDevOpsApi api, ProjectItem? projectItem, string teamName)
        {
            TeamItem? teamItem = null;
            if (teamName != null)
            {
                teamItem = api.GetTeam(projectItem, teamName).Result;
            }
            if (teamItem == null)
            {
                var teamItems = api.GetTeams(projectItem).Result;

                CommandLineChooser chooser = new CommandLineChooser("Team");
                foreach (TeamItem ti in teamItems.value)
                {
                    chooser.Add(new CommandLineChoice(ti.name, ti.id));
                }
                CommandLineChoice choice = chooser.Choose();
                return teamItems.value.SingleOrDefault(pi => pi.name == choice?.Name);

            }
            return teamItem;
        }

        private static ProjectItem? GetProjectName(AzureDevOpsApi api, string? projectName)
        {
            ProjectItem? projectItem = null;
            if (projectName != null)
            {
                projectItem = api.GetProject(projectName).Result;
            }
            if (projectItem == null)
            {
                var projectItems = api.GetProjects().Result;
                CommandLineChooser chooser = new CommandLineChooser("Project");
                foreach (ProjectItem pi in projectItems.value)
                {
                    chooser.Add(new CommandLineChoice(pi.name, pi.id));
                }
                CommandLineChoice choice = chooser.Choose();
                return projectItems.value.SingleOrDefault(pi => pi.name == choice?.Name);
            }
            return projectItem;
        }

        private static List<dynamic> BuildCSVForBoard(AzureDevOpsApi api, ProjectItem projectItem, TeamItem teamItem, BoardItem boardItem, BacklogItem backlogItem)
        {

            WorkItems? workItems =  api.GetWorkItemsFromBacklog(projectItem, teamItem, backlogItem).Result;

            if (!boardItem.isValid)
            {
                Console.WriteLine("Board selected is not valid. Check that you can view the board in Azure DevOps on the web.");
                Environment.Exit(-1);
            }

            var columstring = string.Join(", ",boardItem.columns.Select(x => x.name));

            Console.WriteLine($"Will work through {workItems.workItems.Count()} work items and process the columns  '{columstring}' ");
          if ( ! UtilsConsole.Confirm("Do you want to continue?"))
            {
                Console.WriteLine("user termination");
                Environment.Exit(-1);
            }
           

            string fields = $"{boardItem.fields.columnField.referenceName},{boardItem.fields.rowField.referenceName},{boardItem.fields.doneField.referenceName}";

            var recordsCSV = new List<dynamic>();
            int count = 1;
            int total = workItems.workItems.Count();
            foreach (WorkItemElement wi in workItems.workItems)
            {
                WorkItemDataParent witData = api.GetWorkItemData(projectItem, boardItem, fields, wi).Result;
               Console.WriteLine($"[{count}/{total}][ID:{witData.Id}][Revisions:{witData.Revisions.Count}]");


                var recordCsv = new ExpandoObject() as IDictionary<string, Object>;
                recordCsv.Add("Id", witData.Id);
                recordCsv.Add("Name", witData.Title);


                DateTime recordLatest = DateTime.MinValue;
                // Find highest date for each column...
                foreach (BoardItem_Column bic in boardItem.columns)
                {
                    if (bic.isSplit)
                    {
                        //Console.WriteLine($"{bic.name}:{witData.Id}=Is Split");
                        var revsforColumn = witData.Revisions.Where(item => item.ColumnField == bic.name && item.DoneField == false).Select(item => item);
                        recordLatest = AddColumnToCsv(recordCsv, witData, recordLatest, $"{bic.name} Doing", revsforColumn);

                        var revsforColumn2 = witData.Revisions.Where(item => item.ColumnField == bic.name && item.DoneField == true).Select(item => item);
                        recordLatest = AddColumnToCsv(recordCsv, witData, recordLatest, $"{bic.name} Done", revsforColumn);
                    }
                    else
                    {
                        var revsforColumn = witData.Revisions.Where(item => item.ColumnField == bic.name).Select(item => item);
                        recordLatest = AddColumnToCsv(recordCsv, witData, recordLatest, bic.name, revsforColumn);
                    }

                }

                recordCsv.Add("Team", teamItem.name);
                recordCsv.Add("Type", witData.WorkItemType);
                recordCsv.Add("BlockedDays", 0);
                recordCsv.Add("Labels", witData.Tags);
                recordsCSV.Add(recordCsv);
                count++;
            }
            Console.WriteLine(workItems.workItems.ToList().Count);

            return recordsCSV;
        }

        private static List<dynamic> BuildCSVForBacklog(AzureDevOpsApi api, ProjectItem projectItem, TeamItem teamItem, BacklogItem backlogItem)
        {
            throw new NotImplementedException("This feature is not ready yet... use default for --mode or remove it entirly.");

            WorkItems? workItems = api.GetWorkItemsFromBacklog(projectItem, teamItem, backlogItem).Result;

            Console.WriteLine($"Will work through {workItems.workItems.Count()} work items ");

            WorkItemStatesData wisData = api.GetBoardColumnsForProject(projectItem).Result;



            var recordsCSV = new List<dynamic>();
            foreach (WorkItemElement wi in workItems.workItems)
            {
                WorkItemDataParent witData = api.GetWorkItemData(projectItem, null, null, wi).Result;
                Console.WriteLine($"Loaded {witData.Revisions.Count} revisions for {witData.Id}");

                var recordCsv = new ExpandoObject() as IDictionary<string, Object>;
                recordCsv.Add("Id", witData.Id);
                recordCsv.Add("Name", witData.Title);


                DateTime recordLatest = DateTime.MinValue;
                // Find highest date for each column...
                foreach (WorkItemStateData state in wisData.value)
                {
                        var revsforColumn = witData.Revisions.Where(item => item.State == state.name).Select(item => item);
                        recordLatest = AddColumnToCsv(recordCsv, witData, recordLatest, state.name, revsforColumn);
                }

                recordCsv.Add("Team", teamItem.name);
                recordCsv.Add("Type", witData.WorkItemType);
                recordCsv.Add("BlockedDays", 0);
                recordCsv.Add("Labels", witData.Tags);
                recordsCSV.Add(recordCsv);
            }
            Console.WriteLine(workItems.workItems.ToList().Count);

            return recordsCSV;
        }

        private static DateTime AddColumnToCsv(IDictionary<string, object> recordCsv, WorkItemDataParent witData, DateTime recordLatest, string columnName, IEnumerable<WorkItemData> revsforColumn)
        {
            if (revsforColumn.Count() > 0)
            {
                var finalForColumn = revsforColumn.Last();
                if (finalForColumn.ChangedDate > recordLatest)
                {
                    recordCsv.Add(columnName, finalForColumn.ChangedDate);
                    recordLatest = finalForColumn.ChangedDate;
                    //Console.WriteLine($"{columnName}:{witData.Id}={finalForColumn.ChangedDate.ToString()}");
                }
                else
                {
                    //skipping
                    //Console.WriteLine($"{columnName}:{witData.Id}={finalForColumn.ChangedDate.ToString()} SKIPP AS NOT LATEST");
                    recordCsv.Add(columnName, null);
                }

            }
            else
            {
                recordCsv.Add(columnName, null);
                //Console.WriteLine($"{columnName}:{witData.Id}=null");
            }

            return recordLatest;
        }

        private static string GetResult(string authHeader, string apiToCall)
        {
            try
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
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Call to: {apiToCall}");
                Console.WriteLine(ex.Message);
                throw ex;
            }
            return string.Empty;

        }

    }
}
