﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using DMT.Core.Interfaces;
using DMT.Core.Interfaces.Serialization;
using DMT.Matcher.Data.Interfaces;

namespace DMT.VIR.Matcher.Local.Patterns
{
    public class Pattern : IPattern
    {
        private List<PatternNode> patternNodes;

        public bool IsMatched
        {
            get { return this.patternNodes.All(pn => pn.IsMatched); }
        }

        public Pattern()
        {
            this.patternNodes = new List<PatternNode>();
        }

        public IEnumerable<INode> GetMatchedNodes()
        {
            return this.patternNodes.Where(pn => pn.IsMatched);
        }

        /// <summary>
        /// Add nodes to the pattern.
        /// </summary>
        /// <param name="nodes"></param>
        public void AddNodes(params PatternNode[] nodes)
        {
            this.patternNodes.AddRange(nodes);
        }

        /// <summary>
        /// Clears the matched nodes.
        /// </summary>
        public void Reset()
        {
            patternNodes.ForEach(pn => pn.MatchedNode = null);
        }

        /// <summary>
        /// Gets a pattern node that matches the given name.
        /// </summary>
        /// <param name="name">name of the node</param>
        /// <returns>the pattern node</returns>
        /// <exception cref="InvalidOperationException">there is more than one element OR no element</exception>
        public PatternNode GetNodeByName(string name)
        {
            return this.patternNodes.Single(pn => pn.Name == name);
        }

        /// <summary>
        /// Gets the matched node of a pattern node that matches the
        /// given name.
        /// </summary>
        /// <param name="name">name of the pattern node</param>
        /// <returns>the matched node or null if the pattern node does not have one</returns>
        /// <exception cref="InvalidOperationException">there is more than one element OR no element</exception>
        public INode GetMatchedNodeByName(string name)
        {
            return GetNodeByName(name).MatchedNode;
        }

        /// <summary>
        /// Sets the matched node for a pattern node.
        /// </summary>
        /// <param name="name">name of the pattern node</param>
        /// <param name="match">matched node to set</param>
        public void SetMatchedNodeForPatternNode(string name, INode match)
        {
            GetNodeByName(name).MatchedNode = match;
        }

        /// <summary>
        /// Determines whether the node with the given name has been matched or not.
        /// </summary>
        /// <param name="name">name of the node</param>
        /// <returns>true only if the node is matched</returns>
        /// <exception cref="InvalidOperationException">there is more than one node with given name OR no node with the given name</exception>
        public bool HasMatchedNodeFor(string name)
        {
            var node = GetNodeByName(name);
            return node.IsMatched;
        }

        #region ISerializable

        public void Serialize(XmlWriter writer)
        {
            writer.WriteStartElement("PatternNodes");
            foreach (var patternNode in this.patternNodes)
            {
                writer.WriteStartElement("PatternNode");
                patternNode.Serialize(writer);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        public void Deserialize(XmlReader reader, IContext context)
        {
            List<PatternNode> nodes = new List<PatternNode>();
            while (reader.ReadToFollowing("PatternNode"))
            {
                var pn = new PatternNode(context.EntityFactory);
                pn.Deserialize(reader, context);
                nodes.Add(pn);
            }

            this.patternNodes = nodes;
        }

        #endregion
    }
}
