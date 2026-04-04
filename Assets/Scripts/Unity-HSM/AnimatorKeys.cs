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
            public const string VerticalSpeed = "VerticalSpeed";
            public const string DodgeX = "DodgeX";
            public const string DodgeY = "DodgeY";
            public const string StopFoot = "StopFoot";
        }

        public static class States {
            public const string NormalMove = "NormalMove";
            public const string StopType = "StopType";
            public const string Airborne = Layers.Base + ".Airborne";
            public const string AirborneHang = Layers.Base + ".Airborne.AirborneFall";
            public const string AirborneFall = Layers.Base + ".Airborne.AirborneFall";
            public const string Dodge = "Dodge";
            public const string Landing = "Landing";
            public const string Vault = "Vault";
            // Use full state paths to avoid ambiguity with same-name states in other sub-state machines.
            public const string Climb05 = Layers.Base + ".Grounded.Climb.Climb05";
            public const string Climb10 = Layers.Base + ".Grounded.Climb.Climb10";
            public const string Climb17 = Layers.Base + ".Grounded.Climb.Climb17";
            public const string Climb20 = Layers.Base + ".Grounded.Climb.Climb20";
        }

        public static string ComboState(int step) => $"Combo{step}";
    }
}
