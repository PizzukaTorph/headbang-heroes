using HeadbangHeroes.Charts;
using UnityEngine;

namespace HeadbangHeroes.Gameplay.Neck
{
    /// <summary>
    /// Immutable, value-type capture of the neck's physical arrival state and event-local
    /// evidence at a single moment — taken <b>before</b> a new inversion impulse is applied.
    ///
    /// Downstream Motion Quality evaluation (a later package) consumes this to judge the motion
    /// arriving into an inversion. Because it is an immutable struct copied by value, applying
    /// the subsequent bang to the live model cannot mutate the evidence being judged.
    /// </summary>
    public readonly struct NeckMotionSnapshot
    {
        /// <summary>Axis whose inversion is being evaluated.</summary>
        public readonly BangAxis Axis;

        /// <summary>Displacement (deg) on the evaluated axis at capture time.</summary>
        public readonly float Displacement;

        /// <summary>Velocity (deg/s) on the evaluated axis at capture time (incoming momentum).</summary>
        public readonly float Velocity;

        /// <summary>Travel (deg) accumulated on the evaluated axis since the last inversion boundary.</summary>
        public readonly float TravelSinceInversion;

        /// <summary>Peak speed (deg/s) on the evaluated axis since the last inversion boundary.</summary>
        public readonly float PeakSpeedSinceInversion;

        /// <summary>
        /// True when there was genuine preceding travel to judge. False for the first bang from
        /// a neutral/unprepared setup, where normal incoming-motion quality cannot be fabricated.
        /// </summary>
        public readonly bool Prepared;

        /// <summary>Authoritative simulation tick index at capture time (diagnostic/ordering).</summary>
        public readonly long Tick;

        public NeckMotionSnapshot(
            BangAxis axis,
            float displacement,
            float velocity,
            float travelSinceInversion,
            float peakSpeedSinceInversion,
            bool prepared,
            long tick)
        {
            Axis = axis;
            Displacement = displacement;
            Velocity = velocity;
            TravelSinceInversion = travelSinceInversion;
            PeakSpeedSinceInversion = peakSpeedSinceInversion;
            Prepared = prepared;
            Tick = tick;
        }

        /// <summary>Incoming speed magnitude (deg/s).</summary>
        public float IncomingSpeed => Mathf.Abs(Velocity);
    }
}
