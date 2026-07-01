using System.Collections.Generic;
using UnityEngine;

namespace GraphProcessor
{
    /// <summary>
    /// Group the selected node when created
    /// </summary>
    [System.Serializable]
    public class ProviderGroup : Group
    {
        [System.Serializable]
        public struct ConnectPortInfo
        {
            public string nodeGUID;
            public string portName;
            public string portIdentifier;
            public bool input;
        }

        [System.Serializable]
        public struct EdgeInfo
        {
            public int inputNodeIndex;
            public string inputNodePortField;
            public string inputNodePortIdentifier;
            public int outputNodeIndex;
            public string outputNodePortField;
            public string outputNodePortIdentifier;
        }

        [System.Serializable]
        public struct GroupConnectPort
        {
            public int connectPortInfoIndex;
            public int nodeIndex;
            public string portField;
            public string portIdentifier;
        }

        [System.Serializable]
        public struct GroupNodeEdgeInfo
        {
            public List<string> nodes;
            public List<EdgeInfo> edges;
            public List<GroupConnectPort> connectPorts;
        }

        // For serialization loading
        public ProviderGroup() : base() { }

        public float dropdownFieldWidth = 200f;

        public List<string> groupNames = new List<string>();

        public int currentNameIndex = 0;

        public List<GroupNodeEdgeInfo> constructInfo = new List<GroupNodeEdgeInfo>();

        public List<ConnectPortInfo> connnectPortList = new List<ConnectPortInfo>();
        /// <summary>
        /// Create a new group with a title and a position
        /// </summary>
        /// <param name="title"></param>
        /// <param name="position"></param>
        public ProviderGroup(string title, Vector2 position, List<string> groupNames,
             List<GroupNodeEdgeInfo> constructInfo, float dropdownFieldWidth) : base()
        {
            this.title = title;
            this.position.position = position;
            this.groupNames = groupNames;
            this.constructInfo = constructInfo;
            this.dropdownFieldWidth = dropdownFieldWidth;
        }

        public void setConnectPort(BaseNode node, string name, string identifier, bool input)
        {
            connnectPortList.Add(new ConnectPortInfo() { nodeGUID = node.GUID, portName = name, portIdentifier = identifier, input = input });
        }

        public void addNodeBeforeInit(BaseNode node)
        {
            innerNodeGUIDs.Add(node.GUID);
        }
    }
}