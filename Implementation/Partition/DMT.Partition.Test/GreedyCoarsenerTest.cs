﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Entities;
using DMT.Core.Interfaces;
using DMT.Core.Partition;
using Xunit;

namespace DMT.Partition.Test
{
    public class GreedyCoarsenerTest
    {
        GreedyCoarsener coarsener;

        public GreedyCoarsenerTest()
        {
            this.coarsener = new GreedyCoarsener(new PartitionEntityFactory(new CoreEntityFactory()));
        }

        [Fact]
        public void CalculateRequiredPasses()
        {
            // with 0.5 reduction factor and 0.1 factor
            var passes = coarsener.GetNumberOfRequiredPasses(100);
            Assert.Equal(4, passes);
        }

        [Fact]
        public void CalculateRequiredPassesWithMoreAggressiveReductionFactor()
        {
            coarsener.ReductionFactor = 0.2;
            var passes = coarsener.GetNumberOfRequiredPasses(100);
            Assert.Equal(2, passes);
        }

        [Fact]
        public void ClusterAroundNodeWithHighestDegree()
        {
            var n1 = new Node(new CoreEntityFactory());
            for (int i = 0; i < 5; i++)
            {
                n1.ConnectTo(new Node(), EdgeDirection.Outbound);
            }
            List<INode> nodes = new List<INode>(n1.GetAdjacentNodes());
            nodes.Add(n1);

            var cs = coarsener.BuildClusters(nodes);
            var c1 = cs.Single(sn => sn.Nodes.Contains(n1));

            Assert.Equal(6, c1.Nodes.Count);
        }

        [Fact]
        public void ClusterOfStarWithDefaultReductionFactor()
        {
            var clusters = coarsener.BuildClusters(StarLike());
            // star is a cluster
            Assert.True(clusters.Any(c => c.Nodes.Count == 7));
            // other nodes wrapped in clusters
            Assert.Equal(7, clusters.Count());
        }

        [Fact]
        public void ClusterOfStarForAggressiveReductionFactor()
        {
            coarsener.ReductionFactor = 0.2;
            Assert.Equal(2, coarsener.BuildClusters(StarLike()).Count());
        }

        [Fact]
        public void ClusterForCircle5Makes3Clasters()
        {
            Assert.Equal(3, coarsener.BuildClusters(Circle5()).Count());
        }

        [Fact]
        public void ClusterForCircle5HasOneClasterWithMoreNodes()
        {
            var c = coarsener.BuildClusters(Circle5());
            var e = c.Where(x => x.Nodes.Count > 1);
            Assert.Single(e);
        }

        [Fact]
        public void StarlikeGraphBuildClusterConnections()
        {
            var clusters = coarsener.BuildClusters(StarLike());
            coarsener.BuildEdgesBetweenClusters(clusters);

            var cl = clusters.Single(c => c.Nodes.Count > 1);
            Assert.Single(cl.InboundEdges);
        }

        [Fact]
        public void Circle5ConnectionsAfterBuild()
        {
            var clusters = coarsener.BuildClusters(Circle5());
            coarsener.BuildEdgesBetweenClusters(clusters);

            var cl = clusters.Single(c => c.Nodes.Count > 1);
            Assert.Single(cl.InboundEdges);
            Assert.Single(cl.OutboundEdges);
        }


        private List<INode> StarLike()
        {
            // 5 neighbours of n1 --- n1 --- conn --- n2 --- 5 neightbours of n2
            var n1 = new Node(new CoreEntityFactory());
            var n2 = new Node(new CoreEntityFactory());
            for (int i = 0; i < 5; i++)
            {
                n1.ConnectTo(new Node(), EdgeDirection.Outbound);
                n2.ConnectTo(new Node(), EdgeDirection.Outbound);
            }

            List<INode> nodes = new List<INode> { n1, n2 };
            nodes.AddRange(n1.GetAdjacentNodes());
            nodes.AddRange(n2.GetAdjacentNodes());

            var conn = new Node();
            n1.ConnectTo(conn, EdgeDirection.Outbound);
            n2.ConnectTo(conn, EdgeDirection.Outbound);

            nodes.Add(conn);

            return nodes;
        }

        private List<INode> Circle5()
        {
            INode n1 = new Node(new CoreEntityFactory());
            INode n2 = new Node(new CoreEntityFactory());
            INode n3 = new Node(new CoreEntityFactory());
            INode n4 = new Node(new CoreEntityFactory());
            INode n5 = new Node(new CoreEntityFactory());

            n1.ConnectTo(n2, EdgeDirection.Outbound);
            n2.ConnectTo(n3, EdgeDirection.Outbound);
            n3.ConnectTo(n4, EdgeDirection.Outbound);
            n4.ConnectTo(n5, EdgeDirection.Outbound);
            n5.ConnectTo(n1, EdgeDirection.Outbound);

            return new List<INode> { n1, n2, n3, n4, n5 };
        }
    }
}