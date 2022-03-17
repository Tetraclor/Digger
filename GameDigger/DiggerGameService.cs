using Common;
using GameCore;
using System;

namespace GameDigger
{
    public class DiggerGameService : GameService
    {
        public DiggerGameService(string stringmap) : base(stringmap, typeof(Digger))
        {
        }

        public override bool AddPlayer(IPlayer player)
        {
            var creature = GameState.Map[2, 1];
            //GameState.AddPlayer(player, creature);

            return true;
        }

        public override IPlayerCommand ParsePlayerCommand(string command)
        {
            var move = Enum.Parse<FourDirMove>(command, true);
            var playerCommand = new PlayerCommand() { Move = move };

            return playerCommand;
        }

        public override bool RemovePlayer(IPlayer player)
        {
            throw new NotImplementedException();
        }

        public static string mapWithPlayerTerrain = @"
TTT T
TTD T
T TDT
TT TT";

        public const string bigmapWithPlayerTerrain = @"
TTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T
TTP TTTT TTTT TTTP TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T
T T TTTT TTTT TTTP TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T
TT TTTTT TTTT TTTP TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T
TTT TTTT TTTT TTTP TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T
TTP TTTT TTTT TTTP TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T
T T TTTT TTTT TTTP TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T
TT TTTTT TTTT TTTP TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T
TTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T
TTP TTTT TTTT TTTP TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T
T T TTTT TTTT TTTP TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T
TT TTTTT TTTT TTTP TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T
TTT TTTT TTTT TTTP TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T
TTP TTTT TTTT TTTP TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T
T T TTTT TTTT TTTP TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T
TT TTTTT TTTT TTTP TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T
TTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T
TTP TTTT TTTT TTTP TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T
T T TTTT TTTT TTTP TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T
TT TTTTT TTTT TTTP TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T
TTT TTTT TTTT TTTP TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T
TTP TTTT TTTT TTTP TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T
T T TTTT TTTT TTTP TTTT TTTT TTTT TTTT TTTT TTTT TTTT TTTT T";

        public const string mapWithPlayerTerrainSackGold = @"
DTTGTT TS
TST  TSTT
TTTTTTSTT
T TSTS TT
T TTTG ST
TSTSTT TT";

        public const string mapWithPlayerTerrainSackGoldMonster = @"
DTTGTT TST
TST  TSTTM
TTT TTSTTT
T TSTS TTT
T TTTGMSTS
T TMT M TS
TSTSTTMTTT
S TTST  TG
 TGST MTTT
 T  TMTTTT";
    }
}
