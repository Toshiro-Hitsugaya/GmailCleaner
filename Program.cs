using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace GmailQuickstart
{
    class Program
    {
        //Permisions Requested and Aquired by the application
        static string[] Scopes = { GmailService.Scope.GmailModify };

        //The application works using OAuth.
        //While creating you project at https://console.developers.google.com/apis/credentials/consent make sure this application name matches the Application name
        //as shown in this image http://imgur.com/a/XODky
        static string ApplicationName = "Gmail Deleter";

        static void Main(string[] args)
        {

            #region Authentication

            UserCredential credential;

            //client_secret.json is from https://console.developers.google.com/apis/dashboard
            //it is recomended that you replace this file with your own application credentials
            using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/gmail-dotnet-quickstart.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "User",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            #endregion

            //Starting Service
            var service = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            //Infinite Loop
            for (;;)
            {
                //Create a request for a list of (messages / emails)
                UsersResource.MessagesResource.ListRequest request =
                    service.Users.Messages.List("me");
                
                //Setting up the request
                request.MaxResults = 1000;
                
                //Executing and getting the response
                var messages = request.Execute().Messages;

                //Check if response isn't a null
                //if it is it means the mailbox is empty
                if (messages == null)
                {
                    break;
                }

                //Creating a BatchDeleteRequest
                BatchDeleteMessagesRequest requests = new BatchDeleteMessagesRequest();
                //Initializing a list of message Ids to be submited
                requests.Ids = new List<string>();


                //Add all the message Ids to the deletion request
                foreach (Message message in messages)
                {
                    requests.Ids.Add(message.Id);
                }
                
                //Send google the request to delete all the mails
                service.Users.Messages.BatchDelete(requests, "me").Execute();

                //Show user the number of mails that have been deleted
                Console.WriteLine($"A batch of {messages.Count} messages has been deleted.");
            }

            //Finish with a message for the user
            Console.WriteLine("You're entire gmail inbox has been deleted.");
            Console.ReadLine();
        }
    }
}