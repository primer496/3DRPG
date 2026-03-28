namespace HSM {
    /// <summary>
    /// Centralized animator parameter and state names.
    /// </summary>
    public static class AnimatorKeys {
        public static class Params {
            public const string MoveX = "MoveX";
            public const string MoveZ = "MoveZ";
            public const string Speed = "Speed";
            public const string VerticalSpeed = "VerticalSpeed";
            public const string DodgeX = "DodgeX";
            public const string DodgeY = "DodgeY";
            public const string StopFoot = "StopFoot";
        }

        public static class States {
            public const string NormalMove = "NormalMove";
            public const string StopType = "StopType";
            public const string Airborne = "Airborne";
            public const string Dodge = "Dodge";
            public const string Landing = "Landing";
        }

        public static string ComboState(int step) => $"Combo{step}";
    }
}
