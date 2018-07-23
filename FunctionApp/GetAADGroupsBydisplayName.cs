using System.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

public static class GetAADGroupsBydisplayName
{


    public class HttpResponce
    {

        public string token { get; set; }

        public string[] GroupsSub { get; set; }

    }


    public class InputTrigger
    {

        public string token { get; set; }

        public IDictionary<string, string> Groups { get; set; }

        public InputTrigger()
        {
            this.Groups = new Dictionary<string, string>();
        }
    }


    public class GroupContents
    {
        public string odatatype { get; set; }
        public string objectType { get; set; }
        public string objectId { get; set; }
        public object deletionTimestamp { get; set; }
        public object description { get; set; }
        public object dirSyncEnabled { get; set; }
        public string displayName { get; set; }
        public object lastDirSyncTime { get; set; }
        public object mail { get; set; }
        public string mailNickname { get; set; }
        public bool mailEnabled { get; set; }
        public object onPremisesDomainName { get; set; }
        public object onPremisesNetBiosName { get; set; }
        public object onPremisesSamAccountName { get; set; }
        public object onPremisesSecurityIdentifier { get; set; }
        public List<object> provisioningErrors { get; set; }
        public List<object> proxyAddresses { get; set; }
        public bool securityEnabled { get; set; }
    }

    public class GroupsObject
    {
        public string odatmetadata { get; set; }
        public List<GroupContents> value { get; set; }
    }

    [FunctionName("GetAADGroupsBydisplayName")]
    public static async Task<object> Run([HttpTrigger(WebHookType = "genericJson")]HttpRequestMessage req, TraceWriter log)
    {
        log.Info($"Webhook was triggered!");

        string jsonContent = await req.Content.ReadAsStringAsync();
        dynamic data = JsonConvert.DeserializeObject<InputTrigger>(jsonContent);

        //log.Info(data.Groups.Values);
        if (!data.Groups.ContainsKey("key0"))
        {
            return req.CreateResponse(HttpStatusCode.BadRequest, new
            {
                error = "Please pass groups properties in the input object"
            });
        }

        string tenant = ConfigurationManager.AppSettings["B2C_TENANT_DEV"];
        string clientId = ConfigurationManager.AppSettings["B2C_CLIENTID_DEV"];
        string clientSecret = ConfigurationManager.AppSettings["B2C_CLIENTSECRET_DEV"];
        string url = ConfigurationManager.AppSettings["B2C_GRAPHURL_DEV"];
        string apiversion = ConfigurationManager.AppSettings["API_VERSION"];
        string aadInstance = "https://login.microsoftonline.com/";
        string aadGraphResourceId = "https://graph.windows.net/";
        string aadGraphEndpoint = "https://graph.windows.net/";

        string token;
        var responseString = String.Empty;
        string query = "$filter=";
        string AD_extention_role = "";


        foreach (var group in data.Groups)
        {
            query = query + "startswith(displayName,\'" + group.Value + "\') or ";
            AD_extention_role = group.Value + ";";

        }

        if (data.token == "")
        {
            // seting azure AD graph API 
            string authString = aadInstance + tenant;
            var authenticationContext = new AuthenticationContext(authString);

            //get access token; 
            ClientCredential clientCred = new ClientCredential(clientId, clientSecret);
            AuthenticationResult authenticationResult = await authenticationContext.AcquireTokenAsync(aadGraphResourceId, clientCred);
            token = authenticationResult.AccessToken;
        }
        else
        {
            token = data.token;
        }

        query = query.Substring(0, query.Length - 4);
        //string requestUrl = url + "groups?$filter=" + query_filter + apiversion;
        string requestUrl = "https://graph.windows.net/" + tenant + "/groups/" + "?" + apiversion;

        if (!string.IsNullOrEmpty(query))
        {
            requestUrl += "&" + query;
        }

        Dictionary<string, string> groups_ids = new Dictionary<string, string>();
        string[] GroupsSub = new string[0];


        // ŠY“–groups ID ‚ÌŽæ“¾;
        using (var client = new HttpClient())
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            HttpResponseMessage response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                object formatted = JsonConvert.DeserializeObject(error);
                throw new WebException("Error Calling the Graph API: \n" + JsonConvert.SerializeObject(formatted, Formatting.Indented));
            }


            await response.Content.ReadAsStringAsync().ContinueWith(stringTask =>
            {
                var groups_json = stringTask.Result;
                var resgroups = JsonConvert.DeserializeObject<GroupsObject>(groups_json);

                IEnumerable<GroupContents> collection = resgroups.value;

                foreach (var element in collection.Select((Value, Index) => new { Value, Index }))

                {
                    //groups_ids.Add(group_data.id);
                    groups_ids.Add("key" + element.Index.ToString(), element.Value.objectId);
                    Array.Resize(ref GroupsSub, element.Index + 1);
                    GroupsSub[element.Index] = element.Value.objectId;

                }
            });

        }

        HttpResponce res = new HttpResponce();
        res.token = token;
        res.GroupsSub = GroupsSub;

        return req.CreateResponse(HttpStatusCode.OK, new
        {
            Content = res
        });
    }

}
