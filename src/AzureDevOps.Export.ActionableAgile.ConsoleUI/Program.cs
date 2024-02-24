﻿using Microsoft.Identity.Client;
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
            WriteCurrentStatus(azureDevOpsOrganizationUrl, projectName, teamName, boardName);
            projectName = GetProjectName(authHeader, azureDevOpsOrganizationUrl, projectName);
            WriteCurrentStatus(azureDevOpsOrganizationUrl, projectName, teamName, boardName);
            teamName = GetTeamName(authHeader, azureDevOpsOrganizationUrl, projectName, teamName);
            WriteCurrentStatus(azureDevOpsOrganizationUrl, projectName, teamName, boardName);
            boardName = GetBoardName(authHeader, azureDevOpsOrganizationUrl, projectName, teamName, boardName);
            WriteCurrentStatus(azureDevOpsOrganizationUrl, projectName, teamName, boardName);

            ExportData(authHeader, azureDevOpsOrganizationUrl, projectName, teamName, boardName);
        }

        private static void WriteCurrentStatus(string azureDevOpsOrganizationUrl, string projectName, string teamName, string boardName)
        {
            Console.Clear();
            Console.WriteLine("Azure DevOps Export for Actionable Agile");
            Console.WriteLine("================");
            Console.WriteLine($"Org: {azureDevOpsOrganizationUrl}");
            Console.WriteLine($"Project: {projectName}");
            Console.WriteLine($"Team: {teamName}");
            Console.WriteLine($"Board: {boardName}");
            Console.WriteLine("================");

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

        private static void ExportData(string authHeader, string azureDevOpsOrganizationUrl, string? projectName, string teamName, string boardName)
        {
            //GET https://dev.azure.com/{organization}/{project}/{team}/_apis/work/boards/{id}?api-version=7.2-preview.1
            string apiCallUrl = $"{azureDevOpsOrganizationUrl}/{projectName}/{teamName}/_apis/work/boards/{boardName}?api-version=7.2-preview.1";
            var result = GetResult(authHeader, apiCallUrl);

            dynamic data = JObject.Parse(result);

            Console.WriteLine($"Name: {data.name}");
            foreach (dynamic mo in data.columns)
            {
                Console.WriteLine($"Column: {mo.name}");
            }
            Console.WriteLine(".");

            /// get Work Items From Boards
            string apiCallUrlWi = $"{azureDevOpsOrganizationUrl}/{projectName}/{teamName}/_apis/work/backlogs/{boardName}/workItems?api-version=7.2-preview.1";
            var resultWi = GetResult(authHeader, apiCallUrlWi);
            dynamic data2 = JObject.Parse(resultWi);
            Console.WriteLine(data2.count);

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
