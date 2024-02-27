﻿using AzureDevOps.Export.ActionableAgile.DataContracts;
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

            var modeOption = new Option<RenderMode>("--format", "Do you want to use board columns or backlog work item states, or the whole project");
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
            var authTool = new Authenticator();
            var authHeader = authTool.AuthenticationCommand(token).Result;
            AzureDevOpsApi api = new AzureDevOpsApi(authHeader);

            // test 
            ProfileItem profileItem = GetProfileItem(authHeader);
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
            projectItem = GetProjectName(authHeader, organizationName, projectName);
            WriteCurrentStatus(orgItem, projectItem, teamItem, boardItem, backlogItem);
            teamItem = GetTeamName(authHeader, organizationName, projectItem, teamName);
            WriteCurrentStatus(orgItem, projectItem, teamItem, boardItem, backlogItem);
            boardItem = GetBoardName(api, projectItem, teamItem, boardName);
            WriteCurrentStatus(orgItem, projectItem, teamItem, boardItem, backlogItem);
            backlogItem = GetBacklogName(authHeader, organizationName, projectItem, teamItem, boardItem.name);
            WriteCurrentStatus(orgItem, projectItem, teamItem, boardItem, backlogItem);


            List<dynamic> recordsCSV;
            switch (mode)
            {
                case RenderMode.Backlog:
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
            }

        }

        private static ProfileItem? GetProfileItem(string authHeader)
        {
            ProfileItem? profileItem = null;
            try
            {
                
                string apiCallUrl = $"https://app.vssps.visualstudio.com/_apis/profile/profiles/me";
                var result = GetResult(authHeader, apiCallUrl);
                profileItem = JsonConvert.DeserializeObject<ProfileItem>(result);
                return profileItem;
            }
            catch (Exception)
            {
                 
                return profileItem;
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
                CommandLineChoice choice = chooser.Choose();
                return teamItems.value.SingleOrDefault(pi => pi.name == choice?.Name);

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

            string fields = $"{boardItem.fields.columnField.referenceName},{boardItem.fields.rowField.referenceName},{boardItem.fields.doneField.referenceName}";

            var recordsCSV = new List<dynamic>();
            foreach (WorkItemElement wi in workItems.workItems)
            {
                WorkItemDataParent witData = api.GetWorkItemData(projectItem, boardItem, fields, wi).Result;
               Console.WriteLine($"Loaded {witData.Revisions.Count} revisions for {witData.Id}");


                var recordCsv = new ExpandoObject() as IDictionary<string, Object>;
                recordCsv.Add("Id", witData.Id);
                recordCsv.Add("Name", witData.Title);


                DateTime recordLatest = DateTime.MinValue;
                // Find highest date for each column...
                foreach (BoardItem_Column bic in boardItem.columns)
                {
                    if (bic.isSplit)
                    {
                        Console.WriteLine($"{bic.name}:{witData.Id}=Is Split");
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
