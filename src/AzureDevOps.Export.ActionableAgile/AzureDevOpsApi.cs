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

        public AzureDevOpsApi(string authHeader)
        {
            _authHeader = authHeader;
        }

        public void SetOrganisation(OrgItem orgItem)
        {
            _orgItem = orgItem;
        }

        public async Task<BoardItem?> GetBoard(string projectItemId, string teamItemId, string boardName)
        {
            if (_orgItem == null)
            {
                throw new Exception("Org cant be null! Use SetOrganisation before calling");
            }
            string apiGetSingle = $"{_orgItem.accountUri}/{projectItemId}/{teamItemId}/_apis/work/boards/{boardName}?api-version=7.2-preview.1";
            var result = await GetResult(apiGetSingle);
            if (result != null)
            {
                return JsonConvert.DeserializeObject<BoardItem>(result);
            }
            return null;
        }

        public async Task<OrgItems?> GetOrgs(ProfileItem profileItem)
        {
            if (_orgItem == null)
            {
                throw new Exception("Org cant be null! Use SetOrganisation before calling");
            }
            string apiCallUrl = $"https://app.vssps.visualstudio.com/_apis/accounts?memberId={profileItem.id}&api-version=7.1-preview.1";
            var result = await GetResult(apiCallUrl);
            if (result != null)
            {
                return JsonConvert.DeserializeObject<OrgItems>(result);
            }
            return null;
        }

        public async Task<BoardItems?> GetBoards(string projectItemId, string teamItemId)
        {
            if (_orgItem == null)
            {
                throw new Exception("Org cant be null! Use SetOrganisation before calling");
            }
            string apiCallUrl = $"{_orgItem.accountUri}/{projectItemId}/{teamItemId}/_apis/work/boards?api-version=7.2-preview.1";
            var result = await GetResult(apiCallUrl);
            if (result != null)
            {
                return JsonConvert.DeserializeObject<BoardItems>(result);
            }
            return null;
        }

        public async Task<WorkItemDataParent>  GetWorkItemData(ProjectItem projectItem, BoardItem boardItem, string fields, WorkItemElement wi)
        {
            string apiCallUrlWiSingle = $"{_orgItem.accountUri}/{projectItem.id}/_apis/wit/workitems/{wi.target.id}?$fields={fields}&api-version=7.2-preview.3";
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
            string apiCallUrlWiRevisions = $"{_orgItem.accountUri}/{projectItem.id}/_apis/wit/workitems/{wi.target.id}/revisions?$expand=fields&api-version=7.2-preview.3";
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
                witRevData.ColumnField = (string)jsonfields[boardItem.fields.columnField.referenceName];
                witRevData.RowField = (string)jsonfields[boardItem.fields.rowField.referenceName];

                witRevData.DoneField = jsonfields[boardItem.fields.doneField.referenceName]?.ToObject<bool>();
                witData.Revisions.Add(witRevData);
            }

            return witData;
        }


        public async Task<string> GetResult(string apiToCall)
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
                    Console.WriteLine("Result::{0}:{1}", response.StatusCode, response.ReasonPhrase);
                }
            }
            return string.Empty;
        }

        public async Task<WorkItems?> GetWorkItemsFromBacklog(ProjectItem projectItem, TeamItem teamItem, BacklogItem backlogItem)
        {
            if (_orgItem == null)
            {
                throw new Exception("Org cant be null! Use SetOrganisation before calling");
            }
            string apiCallUrlWi = $"{_orgItem.accountUri}/{projectItem.id}/{teamItem.id}/_apis/work/backlogs/{backlogItem.id}/workItems?api-version=7.2-preview.1";
            var resultWi = await GetResult(apiCallUrlWi);
            if (resultWi != null)
            {
                return JsonConvert.DeserializeObject<WorkItems>(resultWi);
            }
           return null;
        }
    }
}
