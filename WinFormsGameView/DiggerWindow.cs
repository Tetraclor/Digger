using Common;
using GameCore;
using GameDigger;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsGameView
{
    public class DiggerWindow : Form
    {
        private const string mapWithPlayerTerrain = @"
TTT T
TTD T
T TDT
TT TT";

        private const string bigmapWithPlayerTerrain = @"
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

        private const string mapWithPlayerTerrainSackGold = @"
DTTGTT TS
TST  TSTT
TTTTTTSTT
T TSTS TT
T TTTG ST
TSTSTT TT";

        private const string mapWithPlayerTerrainSackGoldMonster = @"
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

        private readonly Dictionary<string, Bitmap> bitmaps = new Dictionary<string, Bitmap>();
        private readonly HashSet<Keys> pressedKeys = new HashSet<Keys>();
        private int tickCount;
        public static int ElementSize = 32;

        private GameState GameState;
        private Game game;
        private UserPlayer userPlayer;
        private List<CreatureTransformation> Transformations = new List<CreatureTransformation>();

        private int MapWidth;
        private int MapHeight;

        private Guid guid;

        public DiggerWindow()
        {
            // InitLocalGame();
            CreatureMapCreator.assembly = Assembly.GetAssembly(typeof(Digger));
            guid = Client.RegisterPlayer();
            userPlayer = new UserPlayer();
            var gameStateDto = Client.GetGameState();
            MapWidth = gameStateDto.MapWidth;
            MapHeight = gameStateDto.MapHeight;
            InitForm(gameStateDto.MapWidth, gameStateDto.MapHeight);
            StartLocalGame(RemoteGameTimerTick);
        }

        void InitForm(int MapWidth, int MapHeight, DirectoryInfo imagesDirectory = null) 
        {
            ClientSize = new Size(
                    ElementSize * MapWidth,
                    ElementSize * MapHeight + ElementSize);

            FormBorderStyle = FormBorderStyle.FixedDialog;
            if (imagesDirectory == null)
                imagesDirectory = new DirectoryInfo("Images");

            foreach (var e in imagesDirectory.GetFiles("*.png"))
                bitmaps[e.Name] = (Bitmap)Image.FromFile(e.FullName);
        }

        void InitLocalGame()
        {
            var map = CreatureMapCreator.CreateMap(mapWithPlayerTerrain, Assembly.GetAssembly(typeof(Digger))); 
            GameState = new GameState(map);
            MapWidth = GameState.MapWidth;
            MapHeight = GameState.MapHeight;
            game = new Game(GameState);

           
            var bot = new BotPlayer();

            GameState.AddPlayer(userPlayer, GameState.Map[2, 1]);
            GameState.AddPlayer(bot, GameState.Map[3, 2]);
        }

        void StartLocalGame(Action<object, EventArgs> action)
        {
            var timer = new Timer();
            timer.Interval = 15;
            timer.Tick += (o, e) => action(o, e);
            timer.Start();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Text = "Digger";
            DoubleBuffered = true;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            pressedKeys.Add(e.KeyCode);
            userPlayer.PressedKey = e.KeyCode;
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            pressedKeys.Remove(e.KeyCode);
            userPlayer.PressedKey = pressedKeys.Any() ? pressedKeys.Min() : Keys.None;
        }

        List<GameCore.Point> drawPoints = new List<GameCore.Point>();

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.TranslateTransform(0, ElementSize);
            e.Graphics.FillRectangle(
                Brushes.Black, 0, 0, ElementSize * MapWidth, ElementSize * MapHeight);

            for (int i = 0; i < Transformations.Count; i++)
            {
                var a = Transformations[i];
                var sprite = bitmaps[CreatureImageNameGetter.Get((dynamic)a.Creature)];
                var drawLocation = drawPoints[i];
                e.Graphics.DrawImage(sprite, drawLocation.X, drawLocation.Y);
            }

            e.Graphics.ResetTransform();
           // e.Graphics.DrawString(GameState.Scores.ToString(), new Font("Arial", 16), Brushes.Green, 0, 0);
        }    

        private void RemoteGameTimerTick(object sendet, EventArgs args)
        {
            Transformations = Client.Turn(guid, (PlayerCommand)userPlayer.GetCommand(null));
            MapToAnimation();

            tickCount++;
            if (tickCount == 8) tickCount = 0;

            Invalidate();
            ;
        }

        private void MapToAnimation()
        {
            var size = ElementSize;
            var d = 4 * (tickCount + 1);
            drawPoints.Clear();

            foreach (var a in Transformations)
            {
                var temp = new GameCore.Point(a.Location.X * size + d * a.Command.DeltaX, a.Location.Y * size + d * a.Command.DeltaY);
                drawPoints.Add(temp);
            }
        }

        private void LocalGameTimerTick(object sender, EventArgs args)
        {
            if (tickCount == 0)
                game.BeginAct();

            Transformations = game.Animations;

            MapToAnimation();

            if (tickCount == 7)
                game.EndAct();

            tickCount++;
            if (tickCount == 8) tickCount = 0;

            Invalidate();
        }
    }

    static class CreatureImageNameGetter
    {
        public static string Get(Terrain terrain) => "Terrain.png";
        public static string Get(Digger digger) => "Digger.png";
    }

    static class Client
    {
        static RestClient RestClient = new RestClient("http://localhost:50322");

        public static Guid RegisterPlayer()
        {
            return Get<Guid>("api/register");
        }

        public static List<CreatureTransformation> Turn(Guid guid, PlayerCommand playerCommand)
        {
            var request = new RestRequest("api/turn");
            request.AddQueryParameter("guid", guid);
            request.AddBody(playerCommand);

            var dto = RestClient.GetAsync<List<DtoCreatureTransformation>>(request).Result;
            
            return dto.Select(v => v.Map()).ToList();
        }

        public static DtoGameState GetGameState()
        {
            return Get<DtoGameState>("/api/game_state");
        }

        public static T Get<T>(string api)
        {
            var result = RestClient.GetJsonAsync<T>(api).Result;

            return result;
        }
    }

    public class UserPlayer : Player
    {
        public Keys PressedKey;

        public override IPlayerCommand GetCommand(IReadOnlyGameState gameState)
        {
            return new PlayerCommand() { Move = KeyToMoveCommand(PressedKey) };
        }

        DiggerMove KeyToMoveCommand(Keys key)
        {
            return key switch
            {
                Keys.Left => DiggerMove.Left,
                Keys.Right => DiggerMove.Right,
                Keys.Up => DiggerMove.Up,
                Keys.Down => DiggerMove.Down,
                _ => DiggerMove.None,
            };
        }
    }
}