namespace HSM {
    /// <summary>
    /// Centralized animator parameter and state names.
    /// </summary>
    public static class AnimatorKeys {
        public static class Layers {
            public const string Base = "Base Layer";
        }

        public static class Params {
            public const string MoveX = "MoveX";
            public const string MoveZ = "MoveZ";
            public const string Speed = "Speed";
            public const string DodgeX = "DodgeX";
            public const string DodgeY = "DodgeY";
            public const string StopFoot = "StopFoot";
        }

        public static class States {
            // 定义状态机层级路径，方便后续维护和避免拼写错误
            public const string Alive = Layers.Base + ".Alive";
            public const string AirbornePath = Alive + ".Airborne";
            public const string GroundedPath = Alive + ".Grounded";
            public const string Locomotion = GroundedPath + ".Locomotion";
            public const string Action = GroundedPath + ".Action";
            public const string ClimbPath = Action + ".Climb";

            // ==== Locomotion (位移状态) ====
            public const string NormalMove = Locomotion + ".NormalMove";
            public const string StopType = Locomotion + ".StopType";

            // ==== Airborne (空中状态) ====
            // 备注：着陆在此结构中属于空中结束的瞬间过渡
            public const string Airborne = AirbornePath;
            public const string AirborneFall = AirbornePath + ".AirborneFall";
            public const string Landing = AirbornePath + ".Landing";

            // ==== Action (主动行为状态) ====
            public const string Dodge = Action + ".Dodge";
            public const string Vault = Action + ".Vault";

            // Use full state paths to avoid ambiguity with same-name states in other sub-state machines.
            public const string Climb05 = ClimbPath + ".Climb05";
            public const string Climb10 = ClimbPath + ".Climb10";
            public const string Climb17 = ClimbPath + ".Climb17";
            public const string Climb20 = ClimbPath + ".Climb20";
        }

        public static string ComboState(int step) => $"Combo{step}";
    }
}
