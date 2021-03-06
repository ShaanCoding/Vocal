﻿using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Vocal
{
    class YoutubeAPI
    {
        private YouTubeService service;
        private LiveBroadcast returnedBroadCast;
        private LiveStream returnedStream;

        //BTW you have to enable live streaming on your YT account, it takes up to 24 hours, to enable this this isn't via API its through you're account.
        private YouTubeService OAuth()
        {
            string directoryString = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)?.Replace("file:\\", "") + "\\" + this.GetType().ToString();
            UserCredential creds;
            using (
                var stream = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "client_secret.json", FileMode.Open, FileAccess.Read))
            {
                creds = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { YouTubeService.Scope.Youtube, YouTubeService.Scope.YoutubeUpload },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(directoryString, true)
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
            returnedBroadCast = GenerateBroadCast();
            returnedStream = GenerateAPIKey();
            BindBroadCast();

            Thread ffMPEGThread = new Thread(FFMPEGStream.Start);
            ffMPEGThread.Start();

            PrepareStream();
            Console.WriteLine("\nFinished Youtube API Function");
        }

        public LiveBroadcast GenerateBroadCast()
        {
            //broadcast snippet contains basic info about the broadcast and this is shown on youtube when launched
            LiveBroadcastSnippet broadCastSnippet = new LiveBroadcastSnippet();
            broadCastSnippet.Title = "Test";
            broadCastSnippet.Description = "A test of the youtube data API";
            //TODO: Add a thumbnail icon
            broadCastSnippet.ScheduledStartTime = DateTime.Now;

            //Applies settings of made for kids and unlisted to snippet i.e broadcast settings
            LiveBroadcastStatus broadCastStatus = new LiveBroadcastStatus();
            broadCastStatus.SelfDeclaredMadeForKids = false;
            broadCastStatus.PrivacyStatus = "unlisted";

            //enables the broadcast to view a different "special stream" in case of time delays etc good for debugging
            MonitorStreamInfo broadCastMonitorStream = new MonitorStreamInfo();
            broadCastMonitorStream.EnableMonitorStream = true;

            //contains info about the monitor stream
            LiveBroadcastContentDetails broadCastContentDetails = new LiveBroadcastContentDetails();
            broadCastContentDetails.MonitorStream = broadCastMonitorStream;

            LiveBroadcast broadCast = new LiveBroadcast();
            broadCast.Snippet = broadCastSnippet; //binds the info regarding stream
            broadCast.Status = broadCastStatus; //privacy settings & reg settings
            broadCast.ContentDetails = broadCastContentDetails; //binds monitor info

            //Allows to finalise the setting up and is ready to be transmitted
            LiveBroadcastsResource.InsertRequest liveBroadCastInsert = service.LiveBroadcasts.Insert(broadCast, "snippet,status,contentDetails");
            //Reads the returned var

            LiveBroadcast returnedBroadCast = liveBroadCastInsert.Execute();

            Console.WriteLine("\n================== Returned Broadcast ==================\n");
            Console.WriteLine("  - Id: " + returnedBroadCast.Id);
            Console.WriteLine("  - Title: " + returnedBroadCast.Snippet.Title);
            Console.WriteLine("  - Description: " + returnedBroadCast.Snippet.Description);
            Console.WriteLine("  - Published At: " + returnedBroadCast.Snippet.PublishedAt);
            Console.WriteLine(
                    "  - Scheduled Start Time: " + returnedBroadCast.Snippet.ScheduledStartTime);
            Console.WriteLine(
                    "  - Scheduled End Time: " + returnedBroadCast.Snippet.ScheduledEndTime);

            return returnedBroadCast;
        }

        public LiveStream GenerateAPIKey()
        {
            //Creation of Stream Key & description
            LiveStreamSnippet streamSnippet = new LiveStreamSnippet();
            streamSnippet.Title = "Vocal - Youtube Bot Key";
            streamSnippet.Description = "Youtube API Auto-Generated Key.";

            //Codex settings
            CdnSettings codexSettings = new CdnSettings();
            codexSettings.Format = "720p";
            codexSettings.IngestionType = "rtmp";

            //Actual live stream binding
            LiveStream stream = new LiveStream();
            stream.Snippet = streamSnippet; //Binds the title
            stream.Cdn = codexSettings; //Binds codex

            //Finalising of settings same as broadcasting
            LiveStreamsResource.InsertRequest liveStreamInsert = service.LiveStreams.Insert(stream, "snippet,cdn");
            //returned stream
            LiveStream returnedStream = liveStreamInsert.Execute();

            Console.WriteLine("\n================== Returned Stream ==================\n");
            Console.WriteLine("  - Id: " + returnedStream.Id);
            Console.WriteLine("  - Title: " + returnedStream.Snippet.Title);
            Console.WriteLine("  - Description: " + returnedStream.Snippet.Description);
            Console.WriteLine("  - Published At: " + returnedStream.Snippet.PublishedAt);
            Console.WriteLine("  - URL: " + returnedStream.Cdn.IngestionInfo.IngestionAddress);
            Console.WriteLine("  - Name: " + returnedStream.Cdn.IngestionInfo.StreamName);

            Program.RTMPURL = returnedStream.Cdn.IngestionInfo.IngestionAddress;
            Program.YtPrivateKey = returnedStream.Cdn.IngestionInfo.StreamName;

            return returnedStream;
        }

        public void BindBroadCast()
        {
            //Executes live broadcast by binding the livestream and broadcast together and submitting it
            var liveBroadcastBind = service.LiveBroadcasts.Bind(returnedBroadCast.Id, "id, contentDetails");
            liveBroadcastBind.StreamId = returnedStream.Id;
            returnedBroadCast = liveBroadcastBind.Execute();

            Console.WriteLine("\n================== Returned Bound Broadcast ==================\n");
            Console.WriteLine("  - Broadcast Id: " + returnedBroadCast.Id);
            Console.WriteLine("  - Bound Stream Id: " + returnedBroadCast.ContentDetails.BoundStreamId);
        }

        public void PrepareStream()
        {
            //recieves returned info id
            var liveStreamRequest = service.LiveStreams.List("id,status");
            liveStreamRequest.Id = returnedStream.Id;


            //Repeats until ready is sent. Continously searches for livestream for stream status to see if it is ready to get streamed to
            string broadCastLoop = "";
            while (broadCastLoop != "ready")
            {
                Thread.Sleep(5000);
                var returnedStreamListResponse = liveStreamRequest.Execute();
                var foundStream = returnedStreamListResponse.Items.Single();
                broadCastLoop = foundStream.Status.StreamStatus;
            }

            //When stream is ready to be streamed we disable monitorstream
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
        }
    }
}
