namespace HeadbangHeroes.Presentation
{
    /// <summary>Reusable hair motion tiers (HAIR_SYSTEM_V1). Length tier decides rig complexity;
    /// longer tiers are softer, heavier and settle slower. Presentation only.</summary>
    public enum HairTier { Bald, Short, Medium, MediumLong, Long }

    /// <summary>
    /// Data-driven configuration for a hair motion tier. Pure gameplay-presentation data (no Unity
    /// refs). Governs a multi-segment chain solver: more segments + lower stiffness + lower damping
    /// (more overshoot) + larger allowed bend for longer, heavier, more dramatic metal hair.
    /// </summary>
    public readonly struct HairMotionTier
    {
        /// <summary>Number of chain segments (0 = bald, no hair motion).</summary>
        public readonly int Segments;
        /// <summary>Spring pull of each segment toward the segment above it.</summary>
        public readonly float Stiffness;
        /// <summary>Velocity damping. Lower = more overshoot / longer settle.</summary>
        public readonly float Damping;
        /// <summary>Max angle (deg) a segment may bend away from the one above it.</summary>
        public readonly float MaxBend;
        /// <summary>Extra velocity injected into the top segment on a neck inversion (the whip kick).</summary>
        public readonly float InversionKick;

        public HairMotionTier(int segments, float stiffness, float damping, float maxBend, float inversionKick)
        {
            Segments = segments < 0 ? 0 : segments;
            Stiffness = stiffness < 0f ? 0f : stiffness;
            Damping = damping < 0f ? 0f : damping;
            MaxBend = maxBend < 1f ? 1f : maxBend;
            InversionKick = inversionKick < 0f ? 0f : inversionKick;
        }

        // Prototype tuning defaults. Longer tiers: more segments, softer, less damped, more bend.
        public static HairMotionTier Bald       => new HairMotionTier(0, 0f, 0f, 1f, 0f);
        public static HairMotionTier Short      => new HairMotionTier(1, 140f, 12f, 25f, 30f);
        public static HairMotionTier Medium     => new HairMotionTier(2, 110f, 9f, 40f, 60f);
        public static HairMotionTier MediumLong => new HairMotionTier(3, 90f, 7f, 55f, 90f);
        public static HairMotionTier Long       => new HairMotionTier(4, 70f, 4f, 85f, 200f);

        public static HairMotionTier For(HairTier tier) => tier switch
        {
            HairTier.Bald => Bald,
            HairTier.Short => Short,
            HairTier.Medium => Medium,
            HairTier.MediumLong => MediumLong,
            _ => Long,
        };
    }
}
