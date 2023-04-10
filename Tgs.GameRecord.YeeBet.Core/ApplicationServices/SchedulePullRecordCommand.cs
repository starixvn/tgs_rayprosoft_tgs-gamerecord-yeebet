using Tgs.GameInterface.YeeBet.Protos;

namespace Tgs.GameRecord.YeeBet.Core.ApplicationServices;
public record SchedulePullRecordCommand(IEnumerable<GameRecordInfo> Records);