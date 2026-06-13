using System.Collections.Generic;

namespace DialogueSystem.Model
{
    [System.Serializable]
    public class DialogueOption
    {
        public string Text;
        public int NextNodeId;
    }

    [System.Serializable]
    public class DialogueNode
    {
        public int Id;
        public string SpeakerName;
        public string Text;
        public List<DialogueOption> Options = new List<DialogueOption>();
        public int NextNodeId = -1; // -1 means end of dialogue or options exist
    }
}
