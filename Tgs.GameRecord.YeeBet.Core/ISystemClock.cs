namespace Tgs.GameRecord.YeeBet.Core;

public interface ISystemClock
{
    DateTimeOffset CurrentTime { get; }
}
