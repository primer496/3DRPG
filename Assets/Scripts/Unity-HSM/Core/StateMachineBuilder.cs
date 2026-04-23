using System.Collections.Generic;
using System.Reflection;

namespace HSM {
    public class StateMachineBuilder {
        readonly State root;
        
        public StateMachineBuilder(State root) {
            this.root = root;
        }

        public StateMachine Build() {
            var m = new StateMachine(root);
            Wire(root, m, new HashSet<State>());
            return m;
        }
        /// <summary>
        /// 连接状态和状态机，不包括状态间
        /// </summary>
        /// <param name="s"></param>
        /// <param name="m"></param>
        /// <param name="visited"></param>
        void Wire(State s, StateMachine m, HashSet<State> visited) {
            if (s == null) return;
            //状态机已连接
            if (!visited.Add(s)) return; 
            //找实例字段/公共字段/非公共字段/继承链向上找。
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            //将flag中所有State的Machine设置为m，即状态绑定状态机
            var machineField = typeof(State).GetField("Machine", flags);
            if (machineField != null) machineField.SetValue(s, m);

            foreach (var fld in s.GetType().GetFields(flags)) {
                //只考虑类型为State的
                if (!typeof(State).IsAssignableFrom(fld.FieldType)) continue;
                //跳过父字段，避免循环引用
                if (fld.Name == "Parent") continue;
                //State字段除了父亲就只剩孩子了
                var child = (State)fld.GetValue(s);
                if (child == null) continue;
                //确保这孩子是s的直接孩子
                if (!ReferenceEquals(child.Parent, s)) continue; 
                //递归进入孩子继续连接
                Wire(child, m, visited); 
            }
        }
    }
}