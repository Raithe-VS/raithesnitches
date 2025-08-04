using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace raithesnitches.src.Violations
{
    public class DeduplicationFilter
    {
        private readonly Dictionary<ViolationFingerprint, double> _lastTriggerTime = new();
        private readonly ICoreServerAPI _sapi;
        private readonly double _deduplicationIntervalSeconds;
        private readonly double _cleanupAfterSeconds;

        public DeduplicationFilter(ICoreServerAPI sapi, double deduplicationIntervalSeconds = 10.0, double cleanupAfterSeconds = 60.0)
        {
            _sapi = sapi;
            _deduplicationIntervalSeconds = deduplicationIntervalSeconds;
            _cleanupAfterSeconds = cleanupAfterSeconds;
        }

        public bool ShouldSuppress(SnitchViolation violation)
        {
            if (ViolationSeverity.IsHighPriority(violation.Type)) return false;

            double now = _sapi.World.Calendar.ElapsedSeconds;
            var key = new ViolationFingerprint(violation.playerUID, violation.Type, SimplifyPos(violation.position));

            if (_lastTriggerTime.TryGetValue(key, out double last))
            {
                if (now - last < _deduplicationIntervalSeconds)
                {
                    return true;
                }
            }

            _lastTriggerTime[key] = now;
            return false;
        }

        public void Cleanup(float dt)
        {
            double now = _sapi.World.Calendar.ElapsedSeconds;
            foreach (var kvp in _lastTriggerTime.ToList())
            {
                if (now - kvp.Value > _cleanupAfterSeconds)
                {
                    _lastTriggerTime.Remove(kvp.Key);
                }
            }
        }

        private BlockPos SimplifyPos(BlockPos pos)
        {
            return new BlockPos(
                (pos.X / 3) * 3,
                pos.Y,
                (pos.Z / 3) * 3
            );
        }

        private readonly record struct ViolationFingerprint(string PlayerUID, EnumViolationType Type, BlockPos Pos);
    }
}
