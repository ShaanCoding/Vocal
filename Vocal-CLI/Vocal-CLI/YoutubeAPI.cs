using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace Vocal_CLI
{
    class YoutubeAPI
    {
        public static string RTMPURL = "";
        public static string YtPrivateKey = "";

        private YouTubeService service;

        //BTW you have to enable live streaming on your YT account, it takes up to 24 hours, to enable this this isn't via API its through you're account.
        private YouTubeService OAuth()
        {
            UserCredential creds;
            using (var stream = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "client_secret.json", FileMode.Open, FileAccess.Read))
            {
                creds = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { YouTubeService.Scope.Youtube, YouTubeService.Scope.YoutubeUpload },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(this.GetType().ToString())
                    ).Result;
            }

            var service = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = creds,
                ApplicationName = this.GetType().ToString()
            });

            return service;
        }

        public YoutubeAPI()
        {
            service = OAuth();

            //broadcast snippet contains basic info about the broadcast and this is shown on youtube when launched
            var broadCastSnippet = new LiveBroadcastSnippet();
            broadCastSnippet.Title = "Test";
            broadCastSnippet.Description = "A test of the youtube data API";
            //TODO: Add a thumbnail icon
            broadCastSnippet.ScheduledStartTime = DateTime.Now;

            //Applies settings of made for kids and unlisted to snippet i.e broadcast settings
            var broadCastStatus = new LiveBroadcastStatus();
            broadCastStatus.SelfDeclaredMadeForKids = false;
            broadCastStatus.PrivacyStatus = "unlisted";

            //enables the broadcast to view a different "special stream" in case of time delays etc good for debugging
            var broadCastMonitorStream = new MonitorStreamInfo();
            broadCastMonitorStream.EnableMonitorStream = true;

            //contains info about the monitor stream
            var broadCastContentDetails = new LiveBroadcastContentDetails();
            broadCastContentDetails.MonitorStream = broadCastMonitorStream;
            //Probs unnessary
            //broadCastContentDetails.ClosedCaptionsType = "closedCaptionsDisabled";
            //broadCastContentDetails.EnableAutoStart = true;
            //broadCastContentDetails.EnableClosedCaptions = false;
            //broadCastContentDetails.StereoLayout;


            var broadCast = new LiveBroadcast();
            //broadCast.Kind = "youtube#liveBroadcast"; //Uncessary
            broadCast.Snippet = broadCastSnippet; //binds the info regarding stream
            broadCast.Status = broadCastStatus; //privacy settings & reg settings
            broadCast.ContentDetails = broadCastContentDetails; //binds monitor info

            //Allows to finalise the setting up and is ready to be transmitted
            var liveBroadCastInsert = service.LiveBroadcasts.Insert(broadCast, "snippet,status,contentDetails");
            //Reads the returned var
            var returnedBroadCast = liveBroadCastInsert.Execute();

            Console.WriteLine("\n================== Returned Broadcast ==================\n");
            Console.WriteLine("  - Id: " + returnedBroadCast.Id);
            Console.WriteLine("  - Title: " + returnedBroadCast.Snippet.Title);
            Console.WriteLine("  - Description: " + returnedBroadCast.Snippet.Description);
            Console.WriteLine("  - Published At: " + returnedBroadCast.Snippet.PublishedAt);
            Console.WriteLine(
                    "  - Scheduled Start Time: " + returnedBroadCast.Snippet.ScheduledStartTime);
            Console.WriteLine(
                    "  - Scheduled End Time: " + returnedBroadCast.Snippet.ScheduledEndTime);

            //Creation of Stream Key & description
            var streamSnippet = new LiveStreamSnippet();
            streamSnippet.Title = "Vocal - Youtube Bot Key";
            streamSnippet.Description = "Youtube API Auto-Generated Key.";

            //Codex settings
            var codexSettings = new CdnSettings();
            codexSettings.Format = "720p";
            codexSettings.IngestionType = "rtmp";

            //Actual live stream binding
            var stream = new LiveStream();
            stream.Kind = "youtube#liveStream"; //should be unnecessary
            stream.Snippet = streamSnippet; //Binds the title
            stream.Cdn = codexSettings; //Binds codex

            //Finalising of settings same as broadcasting
            var liveStreamInsert = service.LiveStreams.Insert(stream, "snippet,cdn");
            //returned stream
            var returnedStream = liveStreamInsert.Execute();

            Console.WriteLine("\n================== Returned Stream ==================\n");
            Console.WriteLine("  - Id: " + returnedStream.Id);
            Console.WriteLine("  - Title: " + returnedStream.Snippet.Title);
            Console.WriteLine("  - Description: " + returnedStream.Snippet.Description);
            Console.WriteLine("  - Published At: " + returnedStream.Snippet.PublishedAt);
            Console.WriteLine("  - URL: " + returnedStream.Cdn.IngestionInfo.IngestionAddress);
            Console.WriteLine("  - Name: " + returnedStream.Cdn.IngestionInfo.StreamName);

            /* INFO */
            //returnedStream.Cdn.IngestionInfo.IngestionAddress stream to this HTML or RMPT
            RTMPURL = returnedStream.Cdn.IngestionInfo.IngestionAddress;
            YtPrivateKey = returnedStream.Cdn.IngestionInfo.StreamName;


            //Executes live broadcast by binding the livestream and broadcast together and submitting it
            var liveBroadcastBind = service.LiveBroadcasts.Bind(returnedBroadCast.Id, "id, contentDetails");
            liveBroadcastBind.StreamId = returnedStream.Id;
            returnedBroadCast = liveBroadcastBind.Execute();

            Console.WriteLine("\n================== Returned Bound Broadcast ==================\n");
            Console.WriteLine("  - Broadcast Id: " + returnedBroadCast.Id);
            Console.WriteLine("  - Bound Stream Id: " + returnedBroadCast.ContentDetails.BoundStreamId);

            //recieves returned info id
            var liveStreamRequest = service.LiveStreams.List("id,status");
            liveStreamRequest.Id = returnedStream.Id;


            //Repeats until A is sent it continously searches for livestream for stream status to see if it is ready to get streamed to
            string broadCastLoop = "";
            while (broadCastLoop != "ready")
            {
                Thread.Sleep(5000); //temp replace with multithreading safe
                var returnedStreamListResponse = liveStreamRequest.Execute();
                var foundStream = returnedStreamListResponse.Items.Single();
                broadCastLoop = foundStream.Status.StreamStatus;
            }

            //USEAEHUAEUAHHUDAHUDAHUSD

            Thread ffMPEGThread = new Thread(FFMPEGStream.Temp);
            ffMPEGThread.Start();

            //when streamstatus is good it disables monitoring of stream and you update the streams content details
            returnedBroadCast.ContentDetails.MonitorStream.EnableMonitorStream = false;
            service.LiveBroadcasts.Update(returnedBroadCast, "contentDetails");
            var liveBroadcastRequest = service.LiveBroadcasts.List("id,status,contentDetails");
            liveBroadcastRequest.Id = returnedBroadCast.Id;


            //broadcastloop until disables monitoring of stream
            broadCastLoop = "";
            while (broadCastLoop != "True")
            {
                Thread.Sleep(5000);
                var returnedBroadcastListResponse = liveBroadcastRequest.Execute();
                var foundBroadcast = returnedBroadcastListResponse.Items.Single();
                broadCastLoop = foundBroadcast.ContentDetails.MonitorStream.EnableMonitorStream.ToString();
            }

            //After broadcast is good we now request to change to Testing status of broadcast
            service.LiveBroadcasts.Transition(LiveBroadcastsResource.TransitionRequest.BroadcastStatusEnum.Testing, returnedBroadCast.Id, "");

            broadCastLoop = "";
            while (broadCastLoop != "ready")
            {
                Thread.Sleep(5000);
                var returnedBroadcastListResponse = liveBroadcastRequest.Execute();
                var foundBroadcast = returnedBroadcastListResponse.Items.Single();
                broadCastLoop = foundBroadcast.Status.LifeCycleStatus;
            }

            
            //Once we're able to successfully enter testing we should then be able to enter live mode so lets go
            service.LiveBroadcasts.Transition(LiveBroadcastsResource.TransitionRequest.BroadcastStatusEnum.Live, returnedBroadCast.Id, "");

            //waits for broadcast status to change again
            broadCastLoop = "";
            while (broadCastLoop != "ready")
            {

                var returnedBroadcastListResponse = liveBroadcastRequest.Execute();
                var foundBroadcast = returnedBroadcastListResponse.Items.Single();
                broadCastLoop = foundBroadcast.Status.LifeCycleStatus;
            }


            Console.WriteLine("\nExited Youtube API Loops");
        }
    }
}
