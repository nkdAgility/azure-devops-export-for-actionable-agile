using Microsoft.Identity.Client;
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

            projectName = GetProjectName(authHeader, azureDevOpsOrganizationUrl, projectName);
            Console.WriteLine($"Project: {projectName}");
            teamName = GetTeamName(authHeader, azureDevOpsOrganizationUrl, projectName, teamName);
            Console.WriteLine($"Team: {teamName}");
            boardName = GetBoardName(authHeader, azureDevOpsOrganizationUrl, projectName, teamName, boardName);
            Console.WriteLine($"Board: {boardName}");

            throw new NotImplementedException();
        }

        private static string GetBoardName(string authHeader, string azureDevOpsOrganizationUrl, string? projectName, string teamName, string boardName)
        {
            if (boardName != null)
            {

                //GET https://dev.azure.com/{organization}/{project}/{team}/_apis/work/boards/{id}?api-version=7.2-preview.1
                string apiGetSingle = $"{azureDevOpsOrganizationUrl}/{projectName}/{teamName}/_apis/work/boards/{boardName}?api-version=7.2-preview.1";
                var singleResult = GetResult(authHeader, apiGetSingle);
                if (string.IsNullOrEmpty(singleResult))
                {
                    boardName = string.Empty;
                }
                else
                {
                    dynamic data = JObject.Parse(singleResult);
                    boardName = data.name;
                }
            }
            if (string.IsNullOrEmpty(boardName))
            {
                //GET https://dev.azure.com/{organization}/{project}/{team}/_apis/work/boards?api-version=7.2-preview.1
                string apiCallUrl = $"{azureDevOpsOrganizationUrl}/{projectName}/{teamName}/_apis/work/boards?api-version=7.2-preview.1";
                var result = GetResult(authHeader, apiCallUrl);
                dynamic data = JObject.Parse(result);

                CommandLineChooser chooser = new CommandLineChooser("Boards");
                foreach (dynamic mo in data.value)
                {
                    chooser.Add(new CommandLineChoice(mo.name, mo.id));
                }
                boardName = chooser.Choose()?.Id;
            }
            return boardName;
        }

        private static string GetTeamName(string authHeader, string azureDevOpsOrganizationUrl, string? projectName, string teamName)
        {
            if (teamName != null)
            {

                //GET https://dev.azure.com/{organization}/_apis/projects/{projectId}/teams/{teamId}?api-version=7.2-preview.3
                string apiGetSingle = $"{azureDevOpsOrganizationUrl}/_apis/projects/{projectName}/teams/{teamName}?api-version=7.2-preview.3";
                var singleResult = GetResult(authHeader, apiGetSingle);
                if (string.IsNullOrEmpty(singleResult))
                {
                    teamName = string.Empty;
                }
                else
                {
                    dynamic data = JObject.Parse(singleResult);
                    teamName = data.name;
                }
            }
            if (string.IsNullOrEmpty(teamName))
            {
                //GET https://dev.azure.com/{organization}/_apis/projects/{projectId}/teams?api-version=7.2-preview.3
                string apiCallUrl = $"{azureDevOpsOrganizationUrl}/_apis/projects/{projectName}/teams?api-version=7.2-preview.3";
                var result = GetResult(authHeader, apiCallUrl);
                dynamic data = JObject.Parse(result);

                CommandLineChooser chooser = new CommandLineChooser("Team");
                foreach (dynamic mo in data.value)
                {
                    chooser.Add(new CommandLineChoice(mo.name, mo.id));
                }
                teamName = chooser.Choose()?.Name;
            }
            return teamName;
        }

        private static string? GetProjectName(string authHeader, string azureDevOpsOrganizationUrl, string? projectName)
        {
            if (projectName != null)
            {

              //GET https://dev.azure.com/{organization}/_apis/projects/{projectId}/teams?api-version=7.2-preview.3
              string apiGetProject = $"{azureDevOpsOrganizationUrl}/_apis/projects/{projectName}?api-version=7.2-preview.4";
              var projectResult = GetResult(authHeader, apiGetProject);
                if (string.IsNullOrEmpty(projectResult))
                {
                    projectName = string.Empty;
                } else
                {
                    dynamic data = JObject.Parse(projectResult);
                    projectName = data.name;
                }
            } 
            if (string.IsNullOrEmpty(projectName))
            {
                string apiCallUrl = $"{azureDevOpsOrganizationUrl}/_apis/projects?stateFilter=All&api-version=2.2";
                var result = GetResult(authHeader, apiCallUrl);
                dynamic data = JObject.Parse(result);

                CommandLineChooser chooser = new CommandLineChooser("Project");
                foreach (dynamic mo in data.value)
                {
                    chooser.Add(new CommandLineChoice(mo.name, mo.id));
                }
                projectName = chooser.Choose()?.Name;
            }

            return projectName;
        }

        private static void ExportData(string token, string azureDevOpsOrganizationUrl, string projectName, string boardName)
        {
            string id = "ba3e157a-c809-4d24-aedd-da8a080ec6da";
            //GET https://dev.azure.com/{organization}/{project}/{team}/_apis/work/boards/{id}?api-version=7.2-preview.1
            string apiCallUrl = $"{azureDevOpsOrganizationUrl}/{projectName}/Application Overview/_apis/work/boards/{id}?api-version=7.2-preview.1";
            var result = GetResult(token, apiCallUrl);

            dynamic data = JObject.Parse(result);
            Console.WriteLine($"Name: {data.name}");
            foreach (dynamic mo in data.columns)
            {
                Console.WriteLine($"Column: {mo.name}");
            }
            Console.WriteLine("All main boards found listed above.");
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get all boards in the project that the authenticated user has access to and print the results.
        /// </summary>
        /// <param name="authHeader"></param>
        private static void ListBoards(string token, string azureDevOpsOrganizationUrl, string project)
        {
            
            string apiCallUrl = $"{azureDevOpsOrganizationUrl}/{project}/Application Overview/_apis/work/boards?api-version=7.2-preview.1";
            var result = GetResult(token, apiCallUrl);

            dynamic data = JObject.Parse(result);
            foreach (dynamic mo in data.value)
            {
                Console.WriteLine($"{mo.name} - {mo.id}");
            }
            Console.WriteLine("All main boards found listed above.");
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
