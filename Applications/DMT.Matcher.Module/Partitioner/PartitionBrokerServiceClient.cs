﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using DMT.Common.Rest;
using DMT.Module.Common.Service;

namespace DMT.Matcher.Module.Partitioner
{
    class PartitionBrokerServiceClient
    {
        public readonly string BaseAddress;

        public const string MatchersPath = "/matchers";

        public PartitionBrokerServiceClient(string baseAddress)
        {
            this.BaseAddress = baseAddress;
        }

        public PartitionBrokerServiceClient(Uri baseAddress)
            : this(baseAddress.AbsoluteUri)
        {

        }

        public bool RegisterMatcher(MatcherInfo matcher)
        {
            byte[] response;
            XmlSerializer s;
            BoolResponse parsedResonse;

            using (var wc = new WebClient { BaseAddress = this.BaseAddress })
            using (var stream = new MemoryStream())
            {
                s = new XmlSerializer(typeof(MatcherInfo));
                s.Serialize(stream, matcher);
                response = wc.UploadData(MatchersPath, HttpMethod.Post, stream.ToArray());
            }

            s = new XmlSerializer(typeof(BoolResponse));
            using (var stream = new MemoryStream(response))
            {
                parsedResonse = (BoolResponse)s.Deserialize(stream);
            }

            return parsedResonse.Result;
        }

        public void MarkMatcherReady(Guid id)
        {
            string url = string.Format("{0}{1}/{2}/ready", this.BaseAddress.TrimEnd('/'), MatchersPath, id);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = HttpMethod.Put;
            req.ContentLength = 0;
            using (req.GetResponse()) ;
        }
    }
}
