﻿using System;
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
    class Program
    {
        static void Main(string[] args)
        {
            YoutubeAPI youtubeAPI = new YoutubeAPI();
            Console.ReadLine();
        }
    }
}
