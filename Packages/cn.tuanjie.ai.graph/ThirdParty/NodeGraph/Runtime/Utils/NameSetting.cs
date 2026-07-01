using System;
using System.Collections.Generic;

namespace GraphProcessor
{
    /// <summary>
    /// 命名系统，确保每个生成的Node的名字都是唯一的
    /// </summary>
    [Serializable]
    internal class NodeName
    {
        public List<string> NameList;

        public NodeName() { 
            NameList = new List<string>();
        }

        public string AddNewName(string name)
        {
            var tmp = name;
            int i = 0;
            while (NameList.IndexOf(tmp) != -1)
                tmp = name + " " + (++i).ToString();
            NameList.Add(tmp);
            return tmp;
        }

        public string ChangeName(string oldName, string newName)
        { 
            NameList.Remove(oldName);
            return AddNewName(newName);
        }

        public void RemoveName(string oldName)
        {
            NameList.Remove(oldName);
        }
    }


}