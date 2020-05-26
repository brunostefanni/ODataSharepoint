using Microsoft.SharePoint.Client;
using Simple.OData.Client;
using System;
using System.Linq;
using System.Net;
using System.Security;

namespace ODataSharepoint
{
    class Program
    {
        const string _USER = "";
        const string _PASS = "";
        const string _URL = "";

        static void Main(string[] args)
        {
            //Read from Project Online: all projects with their id and name.
            var odataEndpoint = _URL + "_api/ProjectData/";

            var odataCommand = "Projects?$select=ProjectId,ProjectName";

            //Get specific project
            //var odataCommand = $"Projects(guid'{_PROJECTGUID}')";
            
            SecureString secpassword = new SecureString();
            foreach (char c in _PASS.ToCharArray()) secpassword.AppendChar(c);

            var credentials = new SharePointOnlineCredentials(_USER, secpassword);
            var authCookieValue = credentials.GetAuthenticationCookie(new Uri(_URL));

            ODataClientSettings settings = new ODataClientSettings()
            {
                BaseUri = new Uri(odataEndpoint),
                Credentials = credentials,
                IgnoreResourceNotFoundException = true,
                OnTrace = (x, y) => Console.WriteLine(string.Format(x, y)),
                PayloadFormat = ODataPayloadFormat.Json
            };

            settings.OnApplyClientHandler = (System.Net.Http.HttpClientHandler clientHandler) =>
            {
                //Deactivate cookie handling to be able to set my own one.
                clientHandler.UseCookies = false;
            };
            settings.BeforeRequest = (System.Net.Http.HttpRequestMessage request) =>
            {
                request.Headers.Add("Cookie", authCookieValue);
            };

            var client = new ODataClient(settings);

            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            var result = client.FindEntriesAsync(odataCommand).Result;
            var packages = result.ToList();

            foreach (var package in packages)
            {
                Console.WriteLine(package["ProjectName"]);
            }

            Console.Read();
        }
    }
}
