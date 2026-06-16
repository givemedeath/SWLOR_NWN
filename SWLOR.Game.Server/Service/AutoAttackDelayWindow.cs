namespace SWLOR.Game.Server.Service
{
    public readonly struct AutoAttackDelayWindow
    {
        public int DesiredDelayMilliseconds { get; }
        public int GateDelayMilliseconds { get; }
        public int AdditionalAttacks { get; }
        public double OverflowCarry { get; }

        public AutoAttackDelayWindow(
            int desiredDelayMilliseconds,
            int gateDelayMilliseconds,
            int additionalAttacks,
            double overflowCarry)
        {
            DesiredDelayMilliseconds = desiredDelayMilliseconds;
            GateDelayMilliseconds = gateDelayMilliseconds;
            AdditionalAttacks = additionalAttacks;
            OverflowCarry = overflowCarry;
        }
    }
}
