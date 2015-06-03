//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace MediaPlayer
{
    /// <summary>
    /// MediaStream Class used to define a media stream with Title, Uri, PlayReady Uri (if used) and PlayReady CustomData (if used)
    /// </summary>
    public class MediaStream
    {
        public MediaStream(string title, string mediaUri, string playReadyUri, string customData)
        {
            Title = title;
            MediaUri = mediaUri;
            PlayReadyUri = playReadyUri;
            CustomData = customData;
        }
        public string Title { get; set; }
        public string MediaUri { get; set; }
        public string PlayReadyUri { get; set; }
        public string CustomData { get; set; }

        public override string ToString() { return Title; }

        public static MediaStream[] GetMediaStreamArray()
        {
            return new MediaStream[]{

            new MediaStream("HTTP WMV 1","http://livestreamingvm.cloudapp.net/vod/wmv/test1.wmv","",""),
            new MediaStream("HTTP WMV 2","http://livestreamingvm.cloudapp.net/vod/wmv/test2.wmv","",""),
            new MediaStream("HTTP MP4 1","http://livestreamingvm.cloudapp.net/vod/mp4/test1.mp4","",""),
            new MediaStream("HTTP MP4 2","http://livestreamingvm.cloudapp.net/vod/mp4/test2.mp4","",""),
            new MediaStream("Smooth Streaming 1","http://livestreamingvm.cloudapp.net/vod/test1/test1.ism/manifest","",""),
            new MediaStream("Smooth Streaming 2","http://livestreamingvm.cloudapp.net/vod/test2/test2.ism/manifest","",""),
            new MediaStream("Smooth Streaming PlayReady","http://playready.directtaps.net/smoothstreaming/SSWSS720H264PR/SuperSpeedway_720.ism/Manifest","http://playready.directtaps.net/pr/svc/rightsmanager.asmx",""),
            
            };
        }
    };
}
