using AzureDevOps.Export.ActionableAgile.DataContracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace AzureDevOps.Export.ActionableAgile.ConsoleUI
{
    public class AzureDevOpsApi
    {
        private readonly string _authHeader;
        private OrgItem _orgItem;

        public AzureDevOpsApi(string token)
        {
            var authTool = new Authenticator();
            _authHeader = authTool.AuthenticationCommand(token).Result;
        }

        public void SetOrganisation(OrgItem orgItem)
        {
            _orgItem = orgItem;
        }

        public async Task<ProfileItem?>  GetProfileItem()
        {
            string apiCallUrl = $"https://app.vssps.visualstudio.com/_apis/profile/profiles/me";
            return await GetObjectResult<ProfileItem>(apiCallUrl, false);
        }

        public async Task<BoardItem?> GetBoard(string projectItemId, string teamItemId, string boardName)
        {
            if (_orgItem == null)
            {
                throw new Exception("Org cant be null! Use SetOrganisation before calling");
            }
            string apiGetSingle = $"https://dev.azure.com/{_orgItem.accountName}/{projectItemId}/{teamItemId}/_apis/work/boards/{boardName}?api-version=7.2-preview.1";
            var result = await GetResult(apiGetSingle);
            if (result != null)
            {
                return JsonConvert.DeserializeObject<BoardItem>(result);
            }
            return null;
        }

        public async Task<OrgItems?> GetOrgs(ProfileItem profileItem)
        {
            string apiCallUrl = $"https://app.vssps.visualstudio.com/_apis/accounts?memberId={profileItem.id}&api-version=7.1-preview.1";
            return await GetObjectResult<OrgItems>(apiCallUrl, false);
        }

        public async Task<BoardItems?> GetBoards(string projectItemId, string teamItemId)
        {
            string apiCallUrl = $"https://dev.azure.com/{_orgItem.accountName}/{projectItemId}/{teamItemId}/_apis/work/boards?api-version=7.2-preview.1";
            return await GetObjectResult<BoardItems>(apiCallUrl);
        }


        public async Task<ProjectItem?> GetProject(string projectName)
        {
            string apiCallUrl = $"https://dev.azure.com/{_orgItem.accountName}/_apis/projects/{projectName}?api-version=7.2-preview.4";
            return await GetObjectResult<ProjectItem>(apiCallUrl);
        }
        public async Task<ProjectItems?> GetProjects()
        {
            string apiCallUrl = $"https://dev.azure.com/{_orgItem.accountName}/_apis/projects?stateFilter=All&api-version=2.2";
            return await GetObjectResult<ProjectItems>(apiCallUrl);
        }



        public async Task<WorkItemDataParent>  GetWorkItemData(ProjectItem projectItem, BoardItem? boardItem, string fields, WorkItemElement wi)
        {
            string apiCallUrlWiSingle = $"https://dev.azure.com/{_orgItem.accountName}/{projectItem.id}/_apis/wit/workitems/{wi.target.id}?$fields={fields}&api-version=7.2-preview.3";
            var single = GetResult(apiCallUrlWiSingle).Result;
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
            string apiCallUrlWiRevisions = $"https://dev.azure.com/{_orgItem.accountName}/{projectItem.id}/_apis/wit/workitems/{wi.target.id}/revisions?$expand=fields&api-version=7.2-preview.3";
            var revs = GetResult( apiCallUrlWiRevisions).Result;
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
                witRevData.State = (string)jsonfields["System.State"];
                if (boardItem != null)
                {
                    witRevData.ColumnField = (string)jsonfields[boardItem.fields.columnField.referenceName];
                    witRevData.RowField = (string)jsonfields[boardItem.fields.rowField.referenceName];
                    witRevData.DoneField = jsonfields[boardItem.fields.doneField.referenceName]?.ToObject<bool>();
                }
                
                witData.Revisions.Add(witRevData);
            }

            return witData;
        }


        private async Task<string> GetResult(string apiToCall)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("User-Agent", "ManagedClientConsoleAppSample");
                client.DefaultRequestHeaders.Add("X-TFS-FedAuthRedirect", "Suppress");
                client.DefaultRequestHeaders.Add("Authorization", _authHeader);

                HttpResponseMessage response = await client.GetAsync(apiToCall);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException();
                }
                else
                {
                    throw new Exception($"Result::{response.StatusCode}:{response.ReasonPhrase}");
                }
            }
            return string.Empty;
        }

        public async Task<WorkItems?> GetWorkItemsFromBacklog(ProjectItem projectItem, TeamItem teamItem, BacklogItem backlogItem)
        {

            string apiCallUrl = $"https://dev.azure.com/{_orgItem.accountName}/{projectItem.id}/{teamItem.id}/_apis/work/backlogs/{backlogItem.id}/workItems?api-version=7.2-preview.1";
            return await GetObjectResult<WorkItems>(apiCallUrl);
        }

        public async Task<WorkItemStatesData?> GetBoardColumnsForProject(ProjectItem projectItem)
        {
            string apiCallUrl = $"https://dev.azure.com/{_orgItem.accountName}/{projectItem.id}/_apis/work/boardcolumns?api-version=7.2-preview.1";
            return await GetObjectResult<WorkItemStatesData>(apiCallUrl);
        }

        public async Task<TeamItem?> GetTeam(ProjectItem projectItem, string teamName)
        {
            string apiCallUrl = $"https://dev.azure.com/{_orgItem.accountName}/_apis/projects/{projectItem.id}/teams/{teamName}?api-version=7.2-preview.3";
            return await GetObjectResult<TeamItem>(apiCallUrl);
        }

        public async Task<TeamItems?> GetTeams(ProjectItem projectItem)
        {
            string apiCallUrl = $"https://dev.azure.com/{_orgItem.accountName}/_apis/projects/{projectItem.id}/teams?api-version=7.2-preview.3";
            return await GetObjectResult<TeamItems>(apiCallUrl);
        }

        private async Task<T?> GetObjectResult<T>(string apiCallUrl, bool valiateOrg = true)
        {
            if (valiateOrg && _orgItem == null)
            {
                throw new Exception("Org cant be null! Use SetOrganisation before calling");
            }
            string? result = "";
            try
            {
                result = await GetResult(apiCallUrl);
                if (!string.IsNullOrEmpty(result))
                {
                    return JsonConvert.DeserializeObject<T>(result);
                }
            }
            catch (Exception ex)
            {
                // Should be logger
                Console.WriteLine($"-----------------------------");
                Console.WriteLine($"Azure DevOps API Call Failed!");
                Console.WriteLine($"apiCallUrl: {apiCallUrl}");
                Console.WriteLine($"Result: {result}");
                Console.WriteLine($"ObjectType: {typeof(T).ToString}");
                Console.WriteLine($"-----------------------------");
                Console.WriteLine(ex.ToString());
                Console.WriteLine($"-----------------------------");
            }


            return default(T);
        }

        public async Task<BacklogItems?> GetBacklogs(ProjectItem projectItem, TeamItem teamItem)
        {
            string apiCallUrl = $"https://dev.azure.com/{_orgItem.accountName}/{projectItem.id}/{teamItem.id}/_apis/work/backlogs?api-version=7.2-preview.1";
            return await GetObjectResult<BacklogItems>(apiCallUrl);
        }
    }
}
