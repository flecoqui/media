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
            new MediaStream("HTTP MPEG-TS 1","http://livestreamingvm.cloudapp.net/vod/ts/test1.ts","",""),
            new MediaStream("HTTP MPEG-TS 2","http://livestreamingvm.cloudapp.net/vod/ts/test2.ts","",""),
            new MediaStream("HTTP HLS 1","http://livestreamingvm.cloudapp.net/vod/test1/HLS/test1-m3u8-aapl.ism/manifest(format=m3u8-aapl)","",""),
            new MediaStream("HTPP HLS 2","http://livestreamingvm.cloudapp.net/vod/test2/HLS/test2-m3u8-aapl.ism/manifest(format=m3u8-aapl)","",""),
            new MediaStream("HLS Apple","https://devimages.apple.com.edgekey.net/streaming/examples/bipbop_4x3/bipbop_4x3_variant.m3u8","",""),
            new MediaStream("HLS Big Buck Bunny","http://multiplatform-f.akamaihd.net/i/multi/will/bunny/big_buck_bunny_,640x360_400,640x360_700,640x360_1000,950x540_1500,.f4v.csmil/master.m3u8","",""),
            
            };
        }
    };
}
