using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace DawnOnline.AgentMatrix.Brains.Neural
{
    class NeuralNetwork
    {
        private Node[] _inputNodes;
        private ReinforcementNode[] _reinforcementInputNodes;
        private Node[] _layerNodes;
        private Node[] _outputNodes;

        internal Node[] InputNodes { get { return _inputNodes; } }
        internal ReinforcementNode[] ReinforcementInputNodes { get { return _reinforcementInputNodes; } }
        internal Node[] LayerNodes { get { return _layerNodes; } }
        internal Node[] OutputNodes { get { return _outputNodes; } }

        internal NeuralNetwork(int nrOfInputNodes, int nrOfLayerNodes, int nrOfOutputNodes)
            : this(nrOfInputNodes, nrOfLayerNodes, nrOfOutputNodes, 0)
        {}

        internal NeuralNetwork(int nrOfInputNodes, int nrOfLayerNodes, int nrOfOutputNodes, int nrOfReinforcementInputNodes)
        {
            _inputNodes = new Node[nrOfInputNodes];
            _reinforcementInputNodes = new ReinforcementNode[nrOfReinforcementInputNodes];
            _layerNodes = new Node[nrOfLayerNodes];
            _outputNodes = new Node[nrOfOutputNodes];

            // Create nodes
            for (var i = 0; i < nrOfInputNodes; i++)
            {
                _inputNodes[i] = new Node();
            }
            for (var i = 0; i < nrOfReinforcementInputNodes; i++)
            {
                _reinforcementInputNodes[i] = new ReinforcementNode();
            }
            for (var i = 0; i < nrOfLayerNodes; i++)
            {
                _layerNodes[i] = new Node();
            }
            for (var i = 0; i < nrOfOutputNodes; i++)
            {
                _outputNodes[i] = new Node();
            }

            // Create edges
            foreach (var inputNode in _inputNodes)
            {
                inputNode.OutGoingEdges = CreateEdges(_layerNodes);
            }
            foreach (var reinforcementInputNode in _reinforcementInputNodes)
            {
                reinforcementInputNode.OutGoingEdges = CreateEdges(_layerNodes);
            }
            foreach (var layerNode in _layerNodes)
            {
                layerNode.OutGoingEdges = CreateEdges(_outputNodes);
            }
        }

        private static Edge[] CreateEdges(Node[] toNodes)
        {
            var edges = new Edge[toNodes.Length];
            for (var i = 0; i < toNodes.Length; i++)
            {
                var newEdge = new Edge();
                newEdge.ToNode = toNodes[i];
                edges[i] = newEdge;
            }
            return edges;
        }

        internal void ClearInput()
        {
            // Do not clear the reinforcement nodes

            foreach (var inputNode in _inputNodes)
            {
                inputNode.CurrentValue = 0;
            }
            foreach (var layerNode in _layerNodes)
            {
                layerNode.CurrentValue = 0;
            }
            foreach (var outputNode in _outputNodes)
            {
                outputNode.CurrentValue = 0;
            }
        }

        internal void ClearReinforcementInput()
        {
            // Do not clear the reinforcement nodes

            foreach (var node in _reinforcementInputNodes)
            {
                node.CurrentValue = 0;
            }
        }

        internal void Propagate(TimeSpan timeDelta)
        {
            foreach (var inputNode in _inputNodes)
            {
                inputNode.Propagate();
            }
            foreach (var reinforcementNode in _reinforcementInputNodes)
            {
                reinforcementNode.Propagate();
            }
            foreach (var layerNode in _layerNodes)
            {
                layerNode.Propagate();
            }

            // Reinforcement
            foreach (var node in _reinforcementInputNodes)
            {
                node.Reinforce();
            }
        }

        internal NeuralNetwork Replicate()
        {
            var newNetwork = new NeuralNetwork(_inputNodes.Length,
                                               _layerNodes.Length,
                                               _outputNodes.Length,
                                               _reinforcementInputNodes.Length);

            for (int i = 0; i < _inputNodes.Length; i++)
            {
                ReplicateEdges(_inputNodes[i], newNetwork._inputNodes[i]);
            }

            for (int i = 0; i < _reinforcementInputNodes.Length; i++)
            {
                ReplicateEdges(_reinforcementInputNodes[i], newNetwork._reinforcementInputNodes[i]);
            }

            for (int i = 0; i < _layerNodes.Length; i++)
            {
                ReplicateEdges(_layerNodes[i], newNetwork._layerNodes[i]);
            }

            return newNetwork;
        }

        private static void ReplicateEdges(Node oldNode, Node newNode)
        {
            for (var j=0; j < oldNode.OutGoingEdges.Length; j++)
            {
                newNode.OutGoingEdges[j].Multiplier = oldNode.OutGoingEdges[j].Multiplier;
            }
        }

        private static int _mutationRate = 150;
        private static int _mutationImpact = 10;

        internal void Mutate()
        {
            //Console.WriteLine("old: ");
            //Console.WriteLine(DebugInfo());

            MutateNodes(_inputNodes);
            MutateNodes(_reinforcementInputNodes);
            MutateNodes(_layerNodes);

            //Console.WriteLine("new: ");
            //Console.WriteLine(DebugInfo());
        }

        private static void MutateNodes(IEnumerable<Node> nodes)
        {
            foreach (var node in nodes)
            {
                MutateThreshold(node);
                MutateEdges(node);
            }
        }

        private static void MutateThreshold(Node node)
        {
            if (Globals.Radomizer.Next(_mutationRate) != 0)
                return;

            var impact = Globals.Radomizer.Next(_mutationImpact + 1) - _mutationImpact / 2;
            if (impact == 0)
            {
                // Reset thresHold
                node.Threshold = 0;
                return;
            }
            node.Threshold += impact;
        }

        private static void MutateEdges(Node node)
        {
            foreach (var edge in node.OutGoingEdges)
            {
                if (Globals.Radomizer.Next(_mutationRate) == 0)
                {
                    var impact = Globals.Radomizer.Next(_mutationImpact + 1) - _mutationImpact / 2;
                    if (impact == 0)
                    {
                        // Reset edge
                        edge.Multiplier = 0;
                        continue;
                    }
                    edge.Multiplier += impact / 10.0;
                }
            }
        }

        public string DebugInfo()
        {
            string info = "";
            for (int i = 0; i < _inputNodes.Length; i++)
            {
                info += string.Format("[ {0}: {1} ] ", i, DebugInfo(_inputNodes[i]));
            }
            info += "|";
            for (int i = 0; i < _reinforcementInputNodes.Length; i++)
            {
                info += string.Format("[ {0}: {1} ] ", i, DebugInfo(_reinforcementInputNodes[i]));
            }
            info += "\n";
            for (int i = 0; i < _layerNodes.Length; i++)
            {
                info += string.Format("[ {0}: {1} ] ", i, DebugInfo(_layerNodes[i]));
            }
            info += "\n";
            for (int i = 0; i < _outputNodes.Length; i++)
            {
                info += string.Format("[ {0}: {1} ] ", i, DebugInfo(_outputNodes[i]));
            }
            return info;
        }

        private static string DebugInfo(Node node)
        {
            string em = "[";
            if (node.OutGoingEdges != null)
            {
                for (var j = 0; j < node.OutGoingEdges.Length; j++)
                {
                    em += node.OutGoingEdges[j].Multiplier + ";";
                }
            }
            em += "]";

            string info = string.Format("[ th: {0}, em: {1} ]", node.Threshold, em);
            return info;
        }

        public void Serialize(BinaryWriter writer)
        {
            SerializeNodes(writer, _inputNodes);
            SerializeNodes(writer, _reinforcementInputNodes);
            SerializeNodes(writer, _layerNodes);
            SerializeNodes(writer, _outputNodes);
        }

        private static void SerializeNodes(BinaryWriter writer, Node[] nodes)
        {
            writer.Write(nodes.Length);
            foreach (var node in nodes)
            {
                node.Serialize(writer);
            }
        }

        public void Deserialize(BinaryReader reader)
        {
            DeserializeNodes(reader, _inputNodes);
            DeserializeNodes(reader, _reinforcementInputNodes);
            DeserializeNodes(reader, _layerNodes);
            DeserializeNodes(reader, _outputNodes);
        }

        private static void DeserializeNodes(BinaryReader reader, Node[] nodes)
        {
            var nrOfNodes = reader.ReadInt32();
            Debug.Assert(nrOfNodes == nodes.Length, "Nodes should have been initialized");
            foreach (var node in nodes)
            {
                node.Deserialize(reader);
            }
        }
    }
}
