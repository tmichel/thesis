﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Exceptions;
using DMT.Core.Interfaces.Exceptions;
using DMT.Core.Serialization;
using DMT.Core.Test.Utils;
using Xunit;

namespace DMT.Core.Test
{
    public class EdgeTest
    {
        private Edge e;
        private Node n1, n2;

        public EdgeTest()
        {
            e = new Edge((n1 = new Node()), (n2 = new Node()));
        }

        [Fact]
        public void AfterSerializationEdgeHasStartNode()
        {
            Edge e = new Edge(new Node(), new Node());

            var doc = SerializerHelper.SerializeObject(e);

            Assert.NotEmpty(doc.Descendants(Edge.StartNodeTag));
        }

        [Fact]
        public void AfterSerializationEdgeHasEndNode()
        {
            Edge e = new Edge(new Node(), new Node());

            var doc = SerializerHelper.SerializeObject(e);

            Assert.NotEmpty(doc.Descendants(Edge.EndNodeTag));
        }

        [Fact]
        public void AfterDeserializationEdgeHasTheSameStartNode()
        {
            Edge e, e2;
            SerializeAndDeserializeEdgeWithContext(out e, out e2);

            Assert.Equal(e.Start.Id, e2.Start.Id);
        }

        [Fact]
        public void AfterDeserializationEdgeHasTheSameEndNode()
        {
            Edge e, e2;
            SerializeAndDeserializeEdgeWithContext(out e, out e2);

            Assert.Equal(e.End.Id, e2.End.Id);
        }

        [Fact]
        public void ConnectingNodesAddsEdgeToNodes()
        {
            Node n1 = new Node(), n2 = new Node();
            Edge e = new Edge();

            e.ConnectNodes(n1, n2);

            Assert.Contains(e, n1.OutboundEdges);
            Assert.Contains(e, n2.InboundEdges);
        }

        [Fact]
        public void ConnectingNodesSetsNodesOnEdge()
        {
            Node n1 = new Node(), n2 = new Node();
            Edge e = new Edge();

            e.ConnectNodes(n1, n2);

            Assert.Equal(n1, e.Start);
            Assert.Equal(n2, e.End);
        }

        [Fact]
        public void ConnectingNodesThrowsExceptionForNullNode()
        {
            Edge e = new Edge();
            Assert.Throws(typeof(ArgumentNullException), () => e.ConnectNodes(null, null));
        }

        [Fact]
        public void ReconnectingAnAlreadyConnectedEdgeIsNotAllowed()
        {
            Assert.Throws(typeof(EdgeAlreadyConnectedException), () => e.ConnectNodes(new Node(), new Node()));
        }

        [Fact]
        public void ToStringContainsClassNameAndId()
        {
            Edge e = new Edge();
            var toString = string.Format("DMT.Core.Edge [{0}]", e.Id);
            Assert.Equal(toString, e.ToString());
        }

        [Fact]
        public void RemovingEdgeRemovesItFromNodesEdgeCollection()
        {
            e.Remove();

            Assert.DoesNotContain(e, n1.OutboundEdges);
            Assert.DoesNotContain(e, n2.InboundEdges);
        }

        [Fact]
        public void RemovingEdgeSetsNullToStartAndEnd()
        {
            e.Remove();

            Assert.Null(e.Start);
            Assert.Null(e.End);
        }

        [Fact]
        public void RemovingNotConnectedEdgeReturnsFalse()
        {
            Edge e = new Edge();
            Assert.Equal(false, e.Remove());
        }

        #region private helper methods

        private static void SerializeAndDeserializeEdgeWithContext(out Edge e, out Edge e2)
        {
            DeserializationContext ctx = new DeserializationContext(new CoreEntityFactory());

            e = new Edge(new Node(), new Node());
            ctx.AddNodes(e.Start, e.End);

            e2 = SerializerHelper.SerializeAndDeserialize(e, ctx);
        }

        #endregion
    }
}
