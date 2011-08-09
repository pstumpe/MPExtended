﻿#region Copyright (C) 2011 MPExtended
// Copyright (C) 2011 MPExtended Developers, http://mpextended.codeplex.com/
// 
// MPExtended is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MPExtended is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MPExtended. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using MPExtended.Services.StreamingService.Units;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.StreamingService.Code;
using MPExtended.Libraries.ServiceLib;

namespace MPExtended.Services.StreamingService.Transcoders
{
    internal class VLC : ITranscoder
    {
        public TranscoderProfile Profile { get; set; }
        public string Input { get; set; }
        public WebMediaInfo MediaInfo { get; set; }
        public string Identifier { get; set; }

        protected bool readOutputStream = true;

        public virtual string GetStreamURL()
        {
            return WCFUtil.GetCurrentRoot() + "StreamingService/stream/RetrieveStream?identifier=" + Identifier;
        }

        public virtual void AlterPipeline(Pipeline pipeline, WebResolution outputSize, Reference<WebTranscodingInfo> einfo, int position, int? audioId, int? subtitleId)
        {
            // VLC doesn't support output parsing, but subclasses do
            AlterPipeline(pipeline, outputSize, einfo, position, audioId, subtitleId, EncoderUnit.LogStream.None);
        }

        public void AlterPipeline(Pipeline pipeline, WebResolution outputSize, Reference<WebTranscodingInfo> einfo, int position, int? audioId, int? subtitleId, EncoderUnit.LogStream output)
        {
            // input
            bool doInputReader = Input.EndsWith(".ts.tsbuffer");
            if (doInputReader)
            {
                pipeline.AddDataUnit(new InputUnit(Input), 1);
            }

            // audio language selection
            string audioTrack = "";
            if (audioId != null)
                audioTrack = "--audio-track " + MediaInfo.AudioStreams.Where(x => x.ID == audioId).First().Index;

            // subtitle selection
            string subtitleTranscoder = "";
            string subtitleArguments = "";
            if (subtitleId != null)
            {
                WebSubtitleStream stream = MediaInfo.SubtitleStreams.Where(x => x.ID == subtitleId).First();
                if (stream.Filename != null)
                {
                    subtitleArguments = "--sub-file=" + stream.Filename;
                }
                else
                {
                    subtitleArguments = "--sub-track " + stream.Index;
                }
                subtitleTranscoder += ",soverlay";
            }

            // prepare output path (some trickying for VLC to accept named pipes)
            string path = @"\#OUT#";
            string muxer = Profile.CodecParameters["muxer"].Replace("#OUT#", path);

            // arguments
            string sout = "#transcode{" + Profile.CodecParameters["encoder"] + ",width=" + outputSize.Width + ",height=" + outputSize.Height + subtitleTranscoder + "}" + muxer;
            string arguments = GenerateArguments("#IN#", sout, subtitleArguments + " " + audioTrack);
            if(!doInputReader)
                arguments = arguments.Replace("#IN#", Input);

            // add the unit
            EncoderUnit.TransportMethod input = doInputReader ? EncoderUnit.TransportMethod.NamedPipe : EncoderUnit.TransportMethod.Other;
            EncoderUnit.TransportMethod outputMethod = readOutputStream ? EncoderUnit.TransportMethod.NamedPipe : EncoderUnit.TransportMethod.Other;
            // waiting for output pipe is meaningless for VLC as it opens it way earlier then that it actually writes to it. Instead, log parsing
            // in VLCWrapped handles the delay (yes, this class is standalone probably useless but is provided for debugging).
            EncoderUnit unit = new EncoderUnit(Profile.CodecParameters["path"], arguments, input, outputMethod, output);
            unit.DebugOutput = false; // change this for debugging
            pipeline.AddDataUnit(unit, 5);
        }

        protected virtual string GenerateArguments(string input, string sout, string args)
        {
            return String.Format("-I dummy -vvv \"{0}\" {1} --sout \"{2}\"", input, args, sout);
        }
    }
}