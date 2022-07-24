using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace NEAT
{
    // READ THIS BEFORE EXPLORING THE CODE
    // the main idea of this class is same as NEAT but approached in a bit diffrent way.
    // so to avoid confusion i think i must explain how it work. First we need to know about the
    // Paring Function, Source: https://en.wikipedia.org/wiki/Pairing_function
    // this function in simple you give it two numbers x and y and it will give you a unique number q
    // so in example x = 3 and y = 2 then q = (x + y) * (x + y + 1) * 0.5 + y so its 17
    // and what make this function unique is that q in this example 17 will only appear if x = 3 and y = 2 ONLY
    // not even a revese (x = 2 and y = 3). so its like a hash function with two inputs
    // to make it clear x is the input node and y is the output node and q is the innovation number for the connection
    // between x and y. nodes id can be just sequential (so 0, 1, 2, 3, 4, 5 ..) and in that way two connections with the same
    // input and output nodes must be the same and there no like them (in Innovation Number).
    //
    // Secondly, how propagation work for getting the output. in the normal way we start from the input nodes and propagate forward
    // until we reach to the output. here... well technicly its the reverse way. we start from the outputs propagate backwords.
    // so if we a network with one output we start in it and go through every node its connected from and request their values.
    // and they also request value from behind until we reach a input node were it just return its value. then the nodes its connected
    // to calculate their values. I think its better to check the code because its confusing to explain.
    //
    // Thirdly, how layers system work. every node is in a certain layer. inputs in layer 0 and outputs in layer 1.
    // The hidden nodes in between. In a normal NEAT this will increase as integer (+1). but this will create a problem were.
    // if we created a new layer in between by creating a new hidden node. we need to recalculate every node in the layers after it.
    // to fix it the layers will be doubles between 0 to 1. so for example if we want to create a node in and conncection between input node
    // and an output node. its layer will be the average between the two nodes. 0 and 1 so it would be 0.5 and so on.
    [Serializable]
    public class Genome
    {
        public enum NodeType
        {
            Input,
            Hidden,
            Output
        }
        public enum ActivationFunction
        {
            ReLU,
            Sigmoid,
            SharpSigmoid,
            Tanh,
            SharpTanh,
        }

        /// <summary>
        /// Stores how good this genome performed. It must be non-negative number.
        /// </summary>
        public double Fitness { get; set; }
        /// <summary>
        /// Number of input nodes.
        /// </summary>
        public int InputNodesNumber { get; }
        /// <summary>
        /// Number of output nodes.
        /// </summary>
        public int OutputNodesNumber { get; }
        /// <summary>
        /// Defines the range of the connections weight when creating them.
        /// </summary>
        public double ConnectionsWeightRange { get; }
        /// <summary>
        /// Defines the range of the nodes bias when creating them.
        /// </summary>
        public double NodesBiasRange { get; }
        /// <summary>
        /// The percent of connection between input and output nodes when creating the genome
        /// </summary>
        public double InitConnectionsPercent { get; }
        /// <summary>
        /// Number of hidden nodes.
        /// </summary>
        public int HiddenNodesNumber { get { return nodes.Count - (InputNodesNumber + OutputNodesNumber); } }
        /// <summary>
        /// Number of connections.
        /// </summary>
        public int ConnectionsNumber { get { return connections.Count; } }
        /// <summary>
        /// If true the genome can create recurrent connection (closed loops connections).
        /// </summary>
        public bool EnableRecurrent { get; }
        /// <summary>
        /// The activation function for the hidden nodes.
        /// </summary>
        public ActivationFunction HiddenFunction { get; }
        /// <summary>
        /// The activation function for the output nodes.
        /// </summary>
        public ActivationFunction OutputFunction { get; }

        // list of all the nodes group in a certain way nodes[(input nodes first), (output nodes then), (lastly hidden nodes)]
        // so to access the input nodes go from index 0 to InputNodesNumber - 1.
        // and to access the output nodes go from index InputNodesNumber to InputNodesNumber + OutputNodesNumber - 1.
        // lastly to acces the hidden nodes go from InputNodesNumber + OutputNodesNumber to the end of the list.
        public List<Node> Nodes { get => nodes; }
        private List<Node> nodes;
        /// <summary>
        /// Contains all the connections in the genome.
        /// its sortedList for when crossovering so its sorted by the innovation number of every connection.
        /// </summary>
        private SortedList<int, Connection> connections;

        /// <summary>
        /// Create new Genome (NEAT class)
        /// </summary>
        /// <param name="inputNum">Number of input nodes in the network</param>
        /// <param name="outputNum">Number of output nodes in the network</param>
        /// <param name="connectionsRange">The range of the connections weight when creating them.</param>
        /// <param name="nodesRange">The range of the nodes bias when creating them.</param>
        /// <param name="initConnectionsPercent">The percent of connection between input and output nodes when creating the genome</param>
        /// <param name="enableRecurrent">Determine whether the the network can contain recurrent net structure</param>
        /// <param name="hiddenAct">The activation function type for the output layer</param>
        /// <param name="outputAct">The activation function type for the hidden layer</param>
        /// <param name="random">Random class for random init</param>
        public Genome(int inputNum, int outputNum, double connectionsRange, double nodesRange, double initConnectionsPercent, bool enableRecurrent, ActivationFunction hiddenAct, ActivationFunction outputAct, Random random)
        {
            InputNodesNumber = inputNum;
            OutputNodesNumber = outputNum;
            EnableRecurrent = enableRecurrent;
            HiddenFunction = hiddenAct;
            OutputFunction = outputAct;
            InitConnectionsPercent = initConnectionsPercent;
            ConnectionsWeightRange = connectionsRange;
            NodesBiasRange = nodesRange;

            nodes = new List<Node>(inputNum + outputNum);
            connections = new SortedList<int, Connection>();

            for (int i = 0; i < inputNum; i++)
            {

                //creates new input node and add it to the list.
                // it doesent matter what bias is because its a input node so puts it 0 and the same to activation function
                // the id will just be the list length so for the first node it will be 0
                nodes.Add(new Node(nodes.Count, 0, NodeType.Input, ActivationFunction.ReLU, 0));
            }
            for (int i = 0; i < outputNum; i++)
            {
                nodes.Add(new Node(nodes.Count, Function.RandomGaussain(0, NodesBiasRange, random), NodeType.Output, outputAct, 1));
            }

            for (int input = 0; input < inputNum; input++)
            {
                for (int output = inputNum; output < inputNum + outputNum; output++)
                {
                    if (random.NextDouble() < initConnectionsPercent)
                        AddConnection(nodes[input], nodes[output], Function.RandomGaussain(0, ConnectionsWeightRange, random));

                }
            }
        }
        /// <summary>
        /// Create new Genome (NEAT class) by copying from another Genome
        /// </summary>
        /// <param name="copyFrom">The Genome to copy from</param>
        public Genome(Genome copyFrom)
        {
            InputNodesNumber = copyFrom.InputNodesNumber;
            OutputNodesNumber = copyFrom.OutputNodesNumber;
            EnableRecurrent = copyFrom.EnableRecurrent;
            HiddenFunction = copyFrom.HiddenFunction;
            OutputFunction = copyFrom.OutputFunction;

            nodes = new List<Node>(copyFrom.nodes.Count);
            connections = new SortedList<int, Connection>(copyFrom.connections.Count);

            for (int i = 0; i < copyFrom.nodes.Count; i++)
            {
                nodes.Add(new Node(copyFrom.nodes[i], connections));
            }
        }
        public Genome(GenomeSaveFile saveFile)
        {
            InputNodesNumber = saveFile.InputNodesNumber;
            OutputNodesNumber = saveFile.OutputNodesNumber;
            ConnectionsWeightRange = saveFile.ConnectionsWeightRange;
            NodesBiasRange = saveFile.NodesBiasRange;
            InitConnectionsPercent = saveFile.InitConnectionsPercent;
            EnableRecurrent = saveFile.EnableRecurrent;
            HiddenFunction = saveFile.HiddenFunction;
            OutputFunction = saveFile.OutputFunction;

            nodes = new List<Node>(saveFile.Nodes.Count);
            connections = new SortedList<int, Connection>();
            for (int i = 0; i < saveFile.Nodes.Count; i++)
            {
                nodes.Add(new Node(saveFile.Nodes[i]));
                for (int i2 = 0; i2 < nodes[i].InputConnections.Count; i2++)
                {
                    connections.Add(nodes[i].InputConnections.Values[i2].InnovationNumber, nodes[i].InputConnections.Values[i2]);
                }
            }
        }

        public GenomeSaveFile Save() => new GenomeSaveFile(this);

        /// <summary>
        /// feed the Genome input and activate the network
        /// </summary>
        /// <param name="input">Input parms</param>
        /// <returns>Array of the output results in the output nodes</returns>
        public double[] CalculateOutput(double[] input)
        {
            ResetAllNodes();

            for (int i = 0; i < InputNodesNumber; i++)
            {
                nodes[i].Value = input[i];
            }

            double[] output = new double[OutputNodesNumber];
            for (int i = InputNodesNumber; i < InputNodesNumber + OutputNodesNumber; i++)
            {
                output[i - InputNodesNumber] = nodes[i].Activate(nodes);
            }

            return output;
        }
        private void AddConnection(Node inputNode, Node outputNode, double weight)
        {
            Connection connection = new Connection(inputNode, outputNode, weight);
            connections.Add(connection.InnovationNumber, connection);
            outputNode.AddConnection(connection);
        }
        /// <summary>
        /// Mutates the Genome
        /// </summary>
        /// <param name="info">Contains the info of the mutation/</param>
        /// <param name="random">The random class.</param>
        public void Mutate(MutationInfo info, Random random)
        {
            for (int i = 0; i < connections.Count; i++)
            {
                if (random.NextDouble() < info.ConnectionWeightMutationChance)
                {
                    if (random.NextDouble() < info.ConnectionWeightChangeChance)
                    {
                        connections.Values[i].Weight += Function.RandomGaussain(0, info.ConnectionWeightOffsetPower, random);
                    }
                    else
                    {
                        connections.Values[i].Weight = Function.RandomGaussain(0, ConnectionsWeightRange, random);
                    }
                }

                if (!connections.Values[i].State)
                {
                    if (random.NextDouble() < info.ConnectionEnableChance)
                    {
                        connections.Values[i].State = true;
                    }
                }
            }
            
            if (random.NextDouble() < info.AddNodeChance && connections.Count != 0)
            {
                Connection connection = connections.Values[random.Next(0, connections.Count)];
                Node inputNode = nodes[connection.InputNodeID];
                Node outputNode = nodes[connection.OutputNodeID];

                Node node = new Node(nodes.Count, Function.RandomGaussain(0, NodesBiasRange, random), NodeType.Hidden, HiddenFunction, (inputNode.Layer + outputNode.Layer) / 2);
                connection.State = false;

                AddConnection(inputNode, node, Function.RandomGaussain(0, ConnectionsWeightRange, random));
                AddConnection(node, outputNode, Function.RandomGaussain(0, ConnectionsWeightRange, random));

                nodes.Add(node);
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                if (random.NextDouble() < info.NodeBiasMutationChance)
                {
                    if (random.NextDouble() < info.NodeBiasChangeChance)
                    {
                        nodes[i].Bias += Function.RandomGaussain(0, info.NodeBiasOffsetPower, random);
                    }
                    else
                    {
                        nodes[i].Bias = Function.RandomGaussain(0, NodesBiasRange, random);
                    }
                }
            }

            if (random.NextDouble() < info.AddConnectionChance)
            {
                double ran = random.NextDouble();
                int n = random.Next(0, InputNodesNumber);
                Node inNode = nodes[ran < (InputNodesNumber / (float)(InputNodesNumber + HiddenNodesNumber)) ? n : random.Next(OutputNodesNumber, nodes.Count)];
                Node outNode = nodes[random.Next(InputNodesNumber, nodes.Count)];
                Connection con = outNode.FindConnection(inNode.ID);

                if (con == null)
                {
                    con = new Connection(inNode, outNode, Function.RandomGaussain(0, ConnectionsWeightRange, random));
                    if (EnableRecurrent)
                    {
                        connections.Add(con.InnovationNumber, con);
                        outNode.AddConnection(con);
                    }
                    else
                    {
                        ResetAllNodes();
                        if (!outNode.RecurrentTest(nodes, con))
                        {
                            connections.Add(con.InnovationNumber, con);
                            outNode.AddConnection(con);
                        }
                    }
                    
                }
                else
                {
                    con.State = true;
                }
            }
        }
        /// <summary>
        /// Resets nodes properties (automaticly used when using FeedForward)
        /// </summary>
        public void ResetAllNodes()
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].Reset();
            }
        }
        /// <summary>
        /// Resets the values for the nodes (important if RecurrentNetEnable is true when you want to reset the network completely)
        /// </summary>
        public void ResetAllNodesValue()
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].ResetValue();
            }
        }
        /// <summary>
        /// return an offspring from 2 genomes as sexual reproduction.
        /// </summary>
        /// <param name="partner1">the first genome</param>
        /// <param name="partner2">the second genome</param>
        /// <param name="random">The random class</param>
        /// <returns></returns>
        public static Genome Crossover(Genome partner1, Genome partner2, Random random)
        {
            bool firstHeigher = partner1.Fitness > partner2.Fitness;

            Genome baby = new Genome(firstHeigher ? partner1 : partner2);

            int myIndex = 0;
            int anotherIndex = 0;

            while (myIndex < partner1.connections.Count - 1 && anotherIndex < partner2.connections.Count - 1)
            {
                Connection partner1Connection = partner1.connections.Values[myIndex];
                Connection partner2Connection = partner2.connections.Values[anotherIndex];

                if (partner1Connection.InnovationNumber == partner2Connection.InnovationNumber)
                {
                    if (random.NextDouble() < 0.5)
                    {
                        baby.connections.Values[firstHeigher ? myIndex : anotherIndex].Weight = !firstHeigher ? partner1Connection.Weight : partner2Connection.Weight;
                    }
                }
                else if (partner1Connection.InnovationNumber > partner2Connection.InnovationNumber)
                {
                    myIndex--;
                }
                else
                {
                    anotherIndex--;
                }

                myIndex++;
                anotherIndex++;
            }

            return baby;
        }
        /// <summary>
        /// Returns the distance between 2 genome. the distance is how diffrent the topologies are for the 2 genomes.
        /// </summary>
        /// <param name="genome1">The first genome.</param>
        /// <param name="genome2">The second genome</param>
        /// <param name="execessDisjointStrenght">How important is the diffrent connections.</param>
        /// <param name="matchingWeightsStrenght">How important is the diffrence between the weights of the matching connections.</param>
        /// <returns></returns>
        public static double Distace(Genome genome1, Genome genome2, double execessDisjointStrenght, double matchingWeightsStrenght)
        {
            int N = genome2.connections.Count > genome1.connections.Count ? genome2.connections.Count : genome1.connections.Count;
            N = N < 20 ? 1 : N;

            double deltaW = 0;
            int Match = 0;
            int disjointAndExess = 0;

            int myIndex = 0;
            int anotherIndex = 0;

            while (myIndex < genome1.connections.Count && anotherIndex < genome2.connections.Count)
            {
                Connection genome1Connection = genome1.connections.Values[myIndex];
                Connection genome2Connection = genome2.connections.Values[anotherIndex];

                if (genome1Connection.InnovationNumber == genome2Connection.InnovationNumber)
                {
                    deltaW += Math.Abs(genome1Connection.Weight - genome2Connection.Weight);
                    Match++;
                }
                else if (genome1Connection.InnovationNumber > genome2Connection.InnovationNumber)
                {
                    myIndex--;
                    disjointAndExess++;
                }
                else
                {
                    anotherIndex--;
                    disjointAndExess++;
                }

                myIndex++;
                anotherIndex++;
            }

            if (Match == 0)
                Match = 1;

            return (execessDisjointStrenght * disjointAndExess / N) + (matchingWeightsStrenght * (deltaW / Match));
        }

        [Serializable]
        public class Node
        {
            /// <summary>
            /// The bias value for this node. instead of using a new node and connection for it.
            /// </summary>
            public double Bias { get; set; }
            /// <summary>
            /// The value calculated in this node. its stored so it can be used for recurrent nets.
            /// </summary>
            public double Value { get; set; }

            /// <summary>
            /// The unique ID for this node.
            /// </summary>
            public int ID { get; }
            /// <summary>
            /// The type of the node if its input, hidden or output.
            /// </summary>
            public NodeType Type { get; }
            /// <summary>
            /// The activation function for this node.
            /// </summary>
            public ActivationFunction ActFunction { get; }
            public double Layer { get; }

            /// <summary>
            /// The list of connections thats connecting to it (input node: any node / output node: this node)
            /// these are stored here so the node can calculate its value from looping this list.
            /// </summary>
            public SortedList<double, Connection> InputConnections { get; set; }

            /// <summary>
            /// This defines the current state of the node if its calculating its value or not.
            /// This is used to determine if there a recurrent structure in the Genome.
            /// so when the start calculating its value this will be true and if its ran into a node with this state true that means is itself.
            /// then it can either return Value or break the process if its checking for recurrent structure.
            /// </summary>
            private bool thinking;
            /// <summary>
            /// This is for saving time. when the node finish calculating its value this will be true.
            /// and if a node request its value it will return Value instantly without recalculating again.
            /// NOTE: this value will reset (set to false) when the whole genome finished calculating its outputs.
            /// </summary>
            private bool calculated;

            public Node(int id, double bias, NodeType type, ActivationFunction function, double layer)
            {
                ID = id;
                Bias = bias;
                Type = type;
                ActFunction = function;
                Layer = layer;

                InputConnections = new SortedList<double, Connection>(new DuplicateKeyComparer<double>());
            }
            public Node(Node copyFrom, SortedList<int, Connection> genomeConnections)
            {
                ID = copyFrom.ID;
                Bias = copyFrom.Bias;
                Type = copyFrom.Type;
                ActFunction = copyFrom.ActFunction;
                Layer = copyFrom.Layer;

                InputConnections = new SortedList<double, Connection>(copyFrom.InputConnections.Count, new DuplicateKeyComparer<double>());
                for (int i = 0; i < copyFrom.InputConnections.Count; i++)
                {
                    Connection clone = new Connection(copyFrom.InputConnections.Values[i]);
                    InputConnections.Add(clone.Length, clone);
                    genomeConnections.Add(clone.InnovationNumber, clone);
                }
            }
            public Node(GenomeSaveFile.NodeSaveFile saveFile)
            {
                Bias = saveFile.Bias;
                ID = saveFile.ID;
                Type = saveFile.Type;
                ActFunction = saveFile.ActFunction;
                Layer = saveFile.Layer;

                InputConnections = new SortedList<double, Connection>(saveFile.InputConnections.Count, new DuplicateKeyComparer<double>());
                for (int i = 0; i < saveFile.InputConnections.Count; i++)
                {
                    InputConnections.Add(saveFile.InputConnections.Keys[i], new Connection(saveFile.InputConnections.Values[i]));
                }
            }

            public double Activate(List<Node> nodes)
            {
                // if the node is not input node nor currently calculating (maybe went back to its self it a circle)
                // or finished already
                if (Type != NodeType.Input && !thinking && !calculated)
                {
                    // state that its currently thinking (calculating its value)
                    thinking = true;
                    double newValue = 0;
                    // go through every connection in the local connections 
                    for (int i = InputConnections.Count - 1; i >= 0; i--)
                    {
                        // if its enabled
                        if (InputConnections.Values[i].State)
                        {
                            // get its value and add it up to newValue
                            newValue += nodes[(int)InputConnections.Values[i].InputNodeID].Activate(nodes) * InputConnections.Values[i].Weight;
                        }
                    }
                    // activate the node depending on the node Activation Function
                    switch (ActFunction)
                    {
                        case ActivationFunction.ReLU:
                            Value = ReLU(newValue + Bias);
                            break;
                        case ActivationFunction.Sigmoid:
                            Value = Sigmoid(newValue + Bias);
                            break;
                        case ActivationFunction.SharpSigmoid:
                            Value = SharpSigmoid(newValue + Bias);
                            break;
                        case ActivationFunction.Tanh:
                            Value = Tanh(newValue + Bias);
                            break;
                        case ActivationFunction.SharpTanh:
                            Value = SharpTanh(newValue + Bias);
                            break;
                    }
                }

                thinking = false;
                calculated = true;
                return Value;
            }
            public void AddConnection(Connection connection)
            {
                InputConnections.Add(connection.Length, connection);
            }
            public Connection FindConnection(int inputID)
            {
                for (int i = 0; i < InputConnections.Count; i++)
                {
                    if (InputConnections.Values[i].InputNodeID == inputID)
                        return InputConnections.Values[i];
                }

                return null;
            }
            public bool RecurrentTest(List<Node> nodes, Connection connection)
            {
                if (thinking == true)
                    return true;

                if (Type != NodeType.Input && !calculated)
                {
                    thinking = true;

                    if (connection.OutputNodeID == ID)
                    {
                        return nodes[connection.InputNodeID].RecurrentTest(nodes, connection);
                    }

                    for (int i = 0; i < InputConnections.Count; i++)
                    {
                        if (InputConnections.Values[i].State)
                        {
                            if (nodes[(int)InputConnections.Values[i].InputNodeID].RecurrentTest(nodes, connection))
                                return true;
                        }
                    }
                }

                thinking = false;
                calculated = true;
                return false;
            }
            public Connection GetConnection(int input)
            {
                return InputConnections.First(x => { return x.Value.InputNodeID == input; }).Value;
            }

            public void Reset()
            {
                //reseting for the feedforward
                thinking = false;
                calculated = false;
            }
            public void ResetValue()
            {
                // reseting in case of recurent neural net
                Value = 0;
            }

            private double ReLU(double Num)
            {
                return Math.Max(0, Num);
            }
            private double Sigmoid(double Num)
            {
                return 1 / (1 + Math.Exp(-Num));
            }
            private double SharpSigmoid(double Num)
            {
                return 1 / (1 + Math.Exp(-5 * Num));
            }
            private double Tanh(double Num)
            {
                double exp = Math.Exp(Num);
                double minus_exp = 1 / exp;
                double ans = (exp - minus_exp) / (exp + minus_exp);
                return double.IsNaN(ans) ? Function.Sign(Num) : ans;
            }
            private double SharpTanh(double Num)
            {
                Num *= 2;

                double exp = Math.Exp(Num);
                double minus_exp = 1 / exp;
                double ans = (exp - minus_exp) / (exp + minus_exp);
                return double.IsNaN(ans) ? Function.Sign(Num) : ans;
            }
        }
        public class Connection : IComparable<Connection>
        {
            /// <summary>
            /// Defines if the connection is enabled or disabled.
            /// </summary>
            public bool State { get; set; }

            public double Weight { get; set; }

            /// <summary>
            /// The unique id that defines the connection from other connections.
            /// </summary>
            public int InnovationNumber { get; }

            /// <summary>
            /// The node ID its connection from.
            /// </summary>
            public int InputNodeID { get; }

            /// <summary>
            /// The node ID its connection to.
            /// </summary>
            public int OutputNodeID { get; }
            /// <summary>
            /// The length of the connection defined by the diffrence between the layers depth of the input node and the output node.
            /// </summary>
            public double Length { get; }

            public Connection(Node inputNode, Node outputNode, double weight)
            {
                InputNodeID = inputNode.ID;
                OutputNodeID = outputNode.ID;
                Weight = weight;
                State = true;
                InnovationNumber = (int)Function.Paring2D((ulong)InputNodeID, (ulong)OutputNodeID);
                Length = Math.Abs(outputNode.Layer - inputNode.Layer);
            }
            public Connection(Connection copyFrom)
            {
                InputNodeID = copyFrom.InputNodeID;
                OutputNodeID = copyFrom.OutputNodeID;
                Weight = copyFrom.Weight;
                State = copyFrom.State;
                InnovationNumber = copyFrom.InnovationNumber;
                Length = copyFrom.Length;
            }
            public Connection(GenomeSaveFile.ConnectionSaveFile saveFile)
            {
                State = saveFile.State;
                Weight = saveFile.Weight;
                InnovationNumber = saveFile.InnovationNumber;
                InputNodeID = saveFile.InputNodeID;
                OutputNodeID = saveFile.OutputNodeID;
                Length = saveFile.Length;
            }

            public int CompareTo(Connection another)
            {
                return InnovationNumber - another.InnovationNumber;
            }
        }
    }
    [Serializable]
    public struct MutationInfo
    {
        /// <summary>
        /// The chance for a connection weight to get mutated. Range [0, 1]
        /// </summary>
        public double ConnectionWeightMutationChance { get; set; }
        /// <summary>
        /// The offset range when changing a connection weight.
        /// <br>EXAMPLE: if its 0.2 so when mutating a connection. if the mutation was offseting the weight.</br>
        /// <br>Then the weight value will change by a normal distribution random value between -0.2 and 0.2 (not exactly).</br>
        /// </summary>
        public double ConnectionWeightOffsetPower { get; set; }
        /// <summary>
        /// The chance to decide what to do with a connection weight if its choosen to be mutated. Range [0, 1]
        /// <br>EXAMPLE: if its 0.9 that means there is 90% chance that it will only change the weight by a random offset</br>
        /// <br>and a 10% chance of completely randomizing the weight. either of them must happen.</br>
        /// </summary>
        public double ConnectionWeightChangeChance { get; set; }
        /// <summary>
        /// The chance for a connection to get enabled if its disabled. Range [0, 1]
        /// </summary>
        public double ConnectionEnableChance { get; set; }
        /// <summary>
        /// The chance for a node bias to get mutated. Range [0, 1]
        /// </summary>
        public double NodeBiasMutationChance { get; set; }
        /// <summary>
        /// The offset range when changing a node bias.
        /// <br>EXAMPLE: if its 0.2 so when mutating a node. if the mutation was offseting the bias.</br>
        /// <br>Then the bias value will change by a normal distribution random value between -0.2 and 0.2 (not exactly).</br>
        /// </summary>
        public double NodeBiasOffsetPower { get; set; }
        /// <summary>
        /// The chance to decide what to do with a node bias if its choosen to be mutated. Range [0, 1]
        /// <br>EXAMPLE: if its 0.9 that means there is 90% chance that it will only change the bias by a random offset</br>
        /// <br>and a 10% chance of completely randomizing the bias. either of them must happen.</br>
        /// </summary>
        public double NodeBiasChangeChance { get; set; }
        /// <summary>
        /// The chance of adding one new connection to the Genome. Range [0, 1]
        /// </summary>
        public double AddConnectionChance { get; set; }
        /// <summary>
        /// The chance of adding one new node to the Genome. Range [0, 1]
        /// </summary>
        public double AddNodeChance { get; set; }

        public static MutationInfo ConvertToDynamic(MutationInfo mutationInfo, Dynamics dynamics, double genPercent, double fitness, double avg, double max)
        {
            MutationInfo info = new MutationInfo();

            info.AddConnectionChance = CalculatePercentRate(fitness, dynamics.DynamicAddConnection, mutationInfo.AddConnectionChance, genPercent, avg, max);
            info.AddNodeChance = CalculatePercentRate(fitness, dynamics.DynamicAddNode, mutationInfo.AddNodeChance, genPercent, avg, max);
            info.ConnectionEnableChance = CalculatePercentRate(fitness, dynamics.DynamicConnectionEnable, mutationInfo.ConnectionEnableChance, genPercent, avg, max);
            info.ConnectionWeightChangeChance = CalculatePercentRate(fitness, dynamics.DynamicConnectionWeightChange, mutationInfo.ConnectionWeightChangeChance, genPercent, avg, max);
            info.ConnectionWeightMutationChance = CalculatePercentRate(fitness, dynamics.DynamicConnectionWeightMutation, mutationInfo.ConnectionWeightMutationChance, genPercent, avg, max);
            info.ConnectionWeightOffsetPower = CalculatePercentRate(fitness, dynamics.DynamicConnectionWeightOffset, mutationInfo.ConnectionWeightOffsetPower, genPercent, avg, max);
            info.NodeBiasChangeChance = CalculatePercentRate(fitness, dynamics.DynamicNodeBiasChange, mutationInfo.NodeBiasChangeChance, genPercent, avg, max);
            info.NodeBiasMutationChance = CalculatePercentRate(fitness, dynamics.DynamicNodeBiasMutation, mutationInfo.NodeBiasMutationChance, genPercent, avg, max);
            info.NodeBiasOffsetPower = CalculatePercentRate(fitness, dynamics.DynamicNodeBiasOffset, mutationInfo.NodeBiasOffsetPower, genPercent, avg, max);

            return info;
        }
        private static double CalculatePercentRate(double fitness, DynamicValue dynamic, double defaultRate, double genPercent, double avg, double max)
        {
            if (!dynamic.Enabled)
                return defaultRate;

            double fitnessPercent = fitness / max;

            if (fitnessPercent < avg)
            {
                double minRange = Function.Map(genPercent, 0, 1, defaultRate, dynamic.LowLimit);
                return Function.Map(Function.BiasFunction(fitnessPercent / avg, dynamic.Bias), 0, 1, minRange, defaultRate);
            }
            else
            {
                double minRange = Function.Map(genPercent, 0, 1, defaultRate, dynamic.HighLimit);
                return Function.Map(Function.BiasFunction(1 - (fitnessPercent - avg) / (1 - avg), dynamic.Bias), 0, 1, minRange, defaultRate);
            }
        }
    }
    public struct SpeciateInfo
    {
        public double ExecessDisjointStrenght { get; set; }
        public double MatchingWeightsStrenght { get; set; }
        public double DistanceThreshHold { get; set; }
        /// <summary>
        /// Create struct for all Distance variables
        /// </summary>
        /// <param name="execessDisjointStrenght">The strength of the excess and disjoint genes</param>
        /// <param name="matchingWeightsStrenght">The strength of the matching genes</param>
        /// <param name="distanceThreshHold">The threshhold for if 2 networks in the same species</param>
        public SpeciateInfo(double execessDisjointStrenght, double matchingWeightsStrenght, double distanceThreshHold)
        {
            ExecessDisjointStrenght = execessDisjointStrenght;
            MatchingWeightsStrenght = matchingWeightsStrenght;
            DistanceThreshHold = distanceThreshHold;
        }
    }
    /// <summary>
    /// Comparer for comparing two keys, handling equality as beeing greater
    /// Use this Comparer e.g. with SortedLists or SortedDictionaries, that don't allow duplicate keys
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    [Serializable]
    public class DuplicateKeyComparer<TKey> : IComparer<TKey> where TKey : IComparable
    {
        #region IComparer<TKey> Members

        public int Compare(TKey x, TKey y)
        {
            int result = x.CompareTo(y);

            if (result == 0)
                return 1; // Handle equality as being greater. Note: this will break Remove(key) or
            else          // IndexOfKey(key) since the comparer never returns 0 to signal key equality
                return result;
        }

        #endregion
    }

    [Serializable]
    public class GenomeSaveFile
    {
        /// <summary>
        /// Number of input nodes.
        /// </summary>
        public int InputNodesNumber { get; }
        /// <summary>
        /// Number of output nodes.
        /// </summary>
        public int OutputNodesNumber { get; }
        /// <summary>
        /// Defines the range of the connections weight when creating them.
        /// </summary>
        public double ConnectionsWeightRange { get; }
        /// <summary>
        /// Defines the range of the nodes bias when creating them.
        /// </summary>
        public double NodesBiasRange { get; }
        /// <summary>
        /// The percent of connection between input and output nodes when creating the genome
        /// </summary>
        public double InitConnectionsPercent { get; }
        /// <summary>
        /// If true the genome can create recurrent connection (closed loops connections).
        /// </summary>
        public bool EnableRecurrent { get; }
        /// <summary>
        /// The activation function for the hidden nodes.
        /// </summary>
        public Genome.ActivationFunction HiddenFunction { get; }
        /// <summary>
        /// The activation function for the output nodes.
        /// </summary>
        public Genome.ActivationFunction OutputFunction { get; }

        public List<NodeSaveFile> Nodes { get; }

        public GenomeSaveFile(Genome genome)
        {
            InputNodesNumber = genome.InputNodesNumber;
            OutputNodesNumber = genome.OutputNodesNumber;
            ConnectionsWeightRange = genome.ConnectionsWeightRange;
            NodesBiasRange = genome.NodesBiasRange;
            InitConnectionsPercent = genome.InitConnectionsPercent;
            EnableRecurrent = genome.EnableRecurrent;
            HiddenFunction = genome.HiddenFunction;
            OutputFunction = genome.OutputFunction;

            Nodes = new List<NodeSaveFile>(genome.Nodes.Count);
            for (int i = 0; i < genome.Nodes.Count; i++)
            {
                Nodes.Add(new NodeSaveFile(genome.Nodes[i]));
            }
        }

        [Serializable]
        public class NodeSaveFile
        {
            /// <summary>
            /// The bias value for this node. instead of using a new node and connection for it.
            /// </summary>
            public double Bias { get; }

            /// <summary>
            /// The unique ID for this node.
            /// </summary>
            public int ID { get; }
            /// <summary>
            /// The type of the node if its input, hidden or output.
            /// </summary>
            public Genome.NodeType Type { get; }
            /// <summary>
            /// The activation function for this node.
            /// </summary>
            public Genome.ActivationFunction ActFunction { get; }
            public double Layer { get; }

            /// <summary>
            /// The list of connections thats connecting to it (input node: any node / output node: this node)
            /// these are stored here so the node can calculate its value from looping this list.
            /// </summary>
            public SortedList<double, ConnectionSaveFile> InputConnections { get;}

            public NodeSaveFile(Genome.Node node)
            {
                Bias = node.Bias;
                ID = node.ID;
                Type = node.Type;
                ActFunction = node.ActFunction;
                Layer = node.Layer;

                InputConnections = new SortedList<double, ConnectionSaveFile>(node.InputConnections.Count, new DuplicateKeyComparer<double>());
                for (int i = 0; i < node.InputConnections.Count; i++)
                {
                    InputConnections.Add(node.InputConnections.Keys[i], new ConnectionSaveFile(node.InputConnections.Values[i]));
                }
            }
        }
        [Serializable]
        public class ConnectionSaveFile : IComparable<ConnectionSaveFile>
        {
            /// <summary>
            /// Defines if the connection is enabled or disabled.
            /// </summary>
            public bool State { get; set; }

            public double Weight { get; set; }

            /// <summary>
            /// The unique id that defines the connection from other connections.
            /// </summary>
            public int InnovationNumber { get; }

            /// <summary>
            /// The node ID its connection from.
            /// </summary>
            public int InputNodeID { get; }

            /// <summary>
            /// The node ID its connection to.
            /// </summary>
            public int OutputNodeID { get; }
            /// <summary>
            /// The length of the connection defined by the diffrence between the layers depth of the input node and the output node.
            /// </summary>
            public double Length { get; }

            public ConnectionSaveFile(Genome.Connection connection)
            {
                State = connection.State;
                Weight = connection.Weight;
                InnovationNumber = connection.InnovationNumber;
                InputNodeID = connection.InputNodeID;
                OutputNodeID = connection.OutputNodeID;
                Length = connection.Length;
            }
            
            public int CompareTo(ConnectionSaveFile another)
            {
                return InnovationNumber - another.InnovationNumber;
            }
        }

    }
}
