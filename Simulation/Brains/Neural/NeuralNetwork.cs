using System;

namespace DawnOnline.Simulation.Brains.Neural
{
    class NeuralNetwork
    {
        private Node[] _inputNodes;
        private Node[] _layerNodes;
        private Node[] _outputNodes;

        internal Node[] InputNodes { get { return _inputNodes; } }
        internal Node[] LayerNodes { get { return _layerNodes; } }
        internal Node[] OutputNodes { get { return _outputNodes; } }

        internal NeuralNetwork(int nrOfInputNodes, int nrOfLayerNodes, int nrOfOutputNodes)
        {
            _inputNodes = new Node[nrOfInputNodes];
            _layerNodes = new Node[nrOfLayerNodes];
            _outputNodes = new Node[nrOfOutputNodes];

            // Create nodes
            for (var i = 0; i < nrOfInputNodes; i++)
            {
                _inputNodes[i] = new Node();
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
                inputNode.OutGoingEdges = new Edge[nrOfLayerNodes];
                for (var i=0; i < nrOfLayerNodes; i++)
                {
                    var newEdge = new Edge();
                    newEdge.ToNode = _layerNodes[i];
                    inputNode.OutGoingEdges[i] = newEdge;
                }
            }
            foreach (var layerNode in _layerNodes)
            {
                layerNode.OutGoingEdges = new Edge[nrOfOutputNodes];
                for (var i = 0; i < nrOfOutputNodes; i++)
                {
                    var newEdge = new Edge();
                    newEdge.ToNode = _outputNodes[i];
                    layerNode.OutGoingEdges[i] = newEdge;
                }
            }
        }

        internal void Clear()
        {
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

        internal void Propagate()
        {
            foreach (var inputNode in _inputNodes)
            {
                inputNode.Propagate();
            }
            foreach (var layerNode in _layerNodes)
            {
                layerNode.Propagate();
            }
        }

        internal NeuralNetwork Replicate()
        {
            var newNetwork = new NeuralNetwork(_inputNodes.Length, _layerNodes.Length, _outputNodes.Length);

            for (int i = 0; i < _inputNodes.Length; i++)
            {
                ReplicateEdges(_inputNodes[i], newNetwork._inputNodes[i]);
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

        private static int _mutationRate = 10;

        internal void Mutate()
        {
            Console.WriteLine("old: ");
            Console.WriteLine(DebugInfo());

            for (int i = 0; i < _inputNodes.Length; i++)
            {
                if (Globals.Radomizer.Next(_mutationRate) == 0)
                    _inputNodes[i].Threshold += Globals.Radomizer.Next(5) - 2;
                MutateEdges(_inputNodes[i]);
            }

            for (int i = 0; i < _layerNodes.Length; i++)
            {
                if (Globals.Radomizer.Next(_mutationRate) == 0)
                    _layerNodes[i].Threshold += Globals.Radomizer.Next(5) - 2;
                MutateEdges(_layerNodes[i]);
            }

            Console.WriteLine("new: ");
            Console.WriteLine(DebugInfo());
        }

        private static void MutateEdges(Node node)
        {
            for (var j = 0; j < node.OutGoingEdges.Length; j++)
            {
                if (Globals.Radomizer.Next(_mutationRate) == 0)
                    node.OutGoingEdges[j].Multiplier += (Globals.Radomizer.Next(5) - 2) / 10.0;
            }
        }

        public string DebugInfo()
        {
            string info = "";
            for (int i = 0; i < _inputNodes.Length; i++)
            {
                info += string.Format("[ {0}: {1} ] ", i, DebugInfo(_inputNodes[i]));
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

    }
}
