using Tgs.GameInterface.YeeBet.Protos;

namespace Tgs.GameRecord.YeeBet.Core.ApplicationServices;
public record ManualPullRecordCommand(IEnumerable<GameRecordInfo> Records);
