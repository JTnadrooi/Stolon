using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using AsitLib;

using MonoGame.Extended;
using static Stolon.StolonGame;

using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Math = System.Math;
using RectangleF = MonoGame.Extended.RectangleF;
using System.Diagnostics;
using System.Xml.Linq;
using MonoGame.Extended.Tiled;

#nullable enable

namespace Stolon
{
    /// <summary>
    /// The representor of the board in the STOLON environment.
    /// </summary>
    public partial class Board : GameComponent
    {
        public Camera2D Camera { get; }
        public float Zoom { get; private set; }
        public const int TILE_SIZE = 96;

        public float MaxDeltaZoom => SmoothnessModifier * 10f;
        public float ZoomIntensity => (Zoom - desiredZoom) / MaxDeltaZoom;
        public Vector2 BoardCenter => state.Tiles[state.Tiles.GetLength(0) / 2, state.Tiles.GetLength(1) / 2].BoardPosition;
        public float SmoothnessModifier => 0.003f;
        public int TurnNumber { get; private set; }
        public ref BoardState State => ref state;
        public ReadOnlyDictionary<string, SearchTarget> SearchTargets => new ReadOnlyDictionary<string, SearchTarget>(searchTargets);
        public BoardState InitialState { get; }
        public Stack<BoardState> History { get; private set; }

        public bool MouseIsOnBoard => SLMouse.Domain == SLMouse.MouseDomain.Board;
        public Vector2 WorldMousePos { get; private set; }

        private SpriteBatch boardSpriteBatch;
        private BoardState state;

        int mouseStateCoefficient;
        private float desiredZoom;
        private Vector2 desiredCameraPos;
        bool firstFrame;

        private Task? computerMoveTask;
        private bool locked;

        private BoardState.SearchTargetCollection searchTargets;
        private const float CONF_ZOOM_COEFFICIENT = 0.98f; // 0.98f

        public UniqueMoveBoardMap UniqueMoveBoardMap { get; }

        public Board(Scene source, BoardState conf) : base(source)
        {
            Camera = new Camera2D();
            TurnNumber = 0;

            state = conf;
            boardSpriteBatch = new SpriteBatch(Instance.GraphicsDevice);
            desiredZoom = MathF.Max(0.45f, CONF_ZOOM_COEFFICIENT * (4f / conf.Dimensions.X)); // does not change.
            desiredCameraPos = BoardCenter;
            Camera.Position = desiredCameraPos;
            searchTargets = conf.WinSearchTargets;
            computerMoveTask = null!;
            firstFrame = false;

            Zoom = 1f;
            InitialState = conf.DeepCopy();
            History = new Stack<BoardState>();
            History.Push(InitialState);
            UniqueMoveBoardMap = new UniqueMoveBoardMap();

            for (int x = 0; x < conf.Dimensions.X; x++)
            {
                Vector2 topleft = new Vector2(x * Board.TILE_SIZE, 0);
            }
        }

        public void Lock()
        {
            locked = true;
        }
        public void Unlock()
        {
            locked = false;
        }

        /// <summary>
        /// Update method. 
        /// </summary>
        /// <param name="elapsedMiliseconds"></param>
        public override void Update(int elapsedMiliseconds)
        {
            if (!firstFrame) firstFrame = true;

            WorldMousePos = Camera.Unproject(SLMouse.VirualPosition);

            mouseStateCoefficient = SLMouse.CurrentState.GetMouseStateCoefficient();

            if (SLKeyboard.IsPressed(Keys.LeftShift))
            {
                if (mouseStateCoefficient == 0) mouseStateCoefficient = 1;
                if (SLKeyboard.IsPressed(Keys.A))
                    desiredCameraPos.X -= 1;
                if (SLKeyboard.IsPressed(Keys.D))
                    desiredCameraPos.X += 1;
                if (SLKeyboard.IsPressed(Keys.W))
                    desiredCameraPos.Y -= 1;
                if (SLKeyboard.IsPressed(Keys.S))
                    desiredCameraPos.Y += 1;
            }

            if (SLMouse.IsPressed(SLMouse.MouseButton.Right)) desiredCameraPos += (SLMouse.PreviousState.Position - SLMouse.CurrentState.Position).ToVector2();
            Zoom += (desiredZoom - Zoom) * 0.1f + mouseStateCoefficient * SmoothnessModifier;
            Camera.Position += (desiredCameraPos - Camera.Position) * 0.1f + (WorldMousePos - Camera.Position) * SmoothnessModifier * Math.Abs(mouseStateCoefficient);
            Camera.Zoom = Zoom;

            Listen();

            //if (Instance.UserInterface.UIElementUpdateData["restartBoard"].IsClicked)
            //{
            //    Instance.Environment.Overlayer.Activate("transition", null, () =>
            //            {
            //                Reset();
            //            }, "Resetting the Board..");
            //}
            //if (Instance.UserInterface.UIElementUpdateData["skipMove"].IsClicked) EndMove();
            //if (Instance.UserInterface.UIElementUpdateData["boardSearch"].IsClicked)
            //{
            //    int ret = State.SearchAny();
            //    if (ret != -1)
            //        Instance.Environment.Overlayer.Activate("transition", null, () =>
            //            {
            //                Reset();
            //            }, "4 Connected found for player " + GetPlayerTile(ret) + "!");

            //}
            //if (Instance.UserInterface.UIElementUpdateData["centerCamera"].IsClicked) desiredCameraPos = BoardCenter;
            //if (Instance.UserInterface.UIElementUpdateData["undoMove"].IsClicked)
            //{
            //    if (state.Players.Any(p => p.IsComputer))
            //    {
            //        Instance.Environment.UI.Textframe.Queue(new DialogueInfo(Instance.Environment, "Not valid when against AI but coming soon!"));
            //    }
            //    Undo();
            //}
            if (SLKeyboard.IsClicked(Keys.Z)) // debug keys
            {
            }
            if (SLKeyboard.IsClicked(Keys.X)) { }
            if (SLKeyboard.IsClicked(Keys.C)) { }
            //if (Instance.UserInterface.UIElementUpdateData["exitGame"].IsClicked) Instance.SLExit();

            //Instance.UserInterface.UIElements["currentPlayer"].Text = "Current: " + state.CurrentPlayer.Name + " " + GetPlayerTile(state.CurrentPlayerID);

            base.Update(elapsedMiliseconds);
        }
        public void Undo()
        {
            Instance.DebugStream.Log(">attempting move undo");
            state.Undo();
            Instance.DebugStream.Success();

        }
        public void AfterMove()
        {
            AudioEngine.Audio.Play(AudioEngine.AudioLibrary["select4"]);
        }
        public bool Listen()
        {
            if (locked)
            {
                computerMoveTask = null;
                Instance.Environment.Overlayer.Deactivate("loading");
                return false;
            }
            if (computerMoveTask != null && computerMoveTask.IsCompletedSuccessfully)
            {
                Instance.Environment.Overlayer.Deactivate("loading");
                computerMoveTask = null;
            }
            if (State.CurrentPlayer.IsComputer)
            {
                computerMoveTask ??= new Task(() =>
                {
                    State.CurrentPlayer.Computer!.DoMove(this);
                    AfterMove();
                });

                if (computerMoveTask.Status == TaskStatus.Created)
                {
                    computerMoveTask.Start();
                    Instance.Environment.Overlayer.Activate("loading");
                }
            }
            else if (StolonStatic.IsMouseClicked(SLMouse.CurrentState, SLMouse.PreviousState) && MouseIsOnBoard)
            {
                Instance.DebugStream.Log(">attempting board alter after mouseclick");
                Move? move = null;
                for (int x = 0; x < state.Tiles.GetLength(0); x++)
                    for (int y = 0; y < state.Tiles.GetLength(1); y++)
                        if (state.Tiles[x, y].HitBox.Contains(WorldMousePos) && !state.Tiles[x, y].IsSolid())
                        {
                            move = new Move(x, y);
                            break;
                        }
                if (move.HasValue)
                {
                    History.Push(State.DeepCopy());
                    State.Alter(move!.Value, true);
                    AfterMove();
                    Instance.DebugStream.Success();
                    return true;
                }
                else Instance.DebugStream.Fail();
            }
            return false;
        }
        public void Reset()
        {
            Instance.DebugStream.Log(">resetting board");

            computerMoveTask = null;
            State = InitialState.DeepCopy();


            Instance.DebugStream.Success();
        }
        public void EndMove()
        {
            State.GoNextPlayer();
        }
        public override void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds)
        {
            boardSpriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: Camera.View);
            for (int x = 0; x < state.Dimensions.X; x++)
                for (int y = 0; y < state.Dimensions.Y; y++)
                {
                    Tile tile = state.Tiles[x, y];
                    boardSpriteBatch.Draw(tile.TileType.Texture, tile.BoardPosition, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    int playerid = tile.GetOccupiedByPlayerID();
                    if (playerid != -1)
                    {
                        boardSpriteBatch.Draw(Instance.Textures.GetReference("textures\\player" + playerid + "item_96"), tile.BoardPosition, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    }
                    else if (tile.HasAttribute<TileAttributes.TileAttributeGravDown>()) boardSpriteBatch.DrawString(Instance.Fonts["fonts\\smollerMono"], string.Empty, tile.BoardPosition + new Vector2(10), Color.White);
                    else if (tile.HasAttribute<TileAttributes.TileAttributeGravUp>()) boardSpriteBatch.DrawString(Instance.Fonts["fonts\\smollerMono"], ("^").ToString(), (tile.BoardPosition + new Vector2(10)).PixelLock(Camera), Color.White);
                    else boardSpriteBatch.DrawString(Instance.Fonts["fonts\\smollerMono"], ("Z").ToString(), tile.BoardPosition + new Vector2(10), Color.White);
                }
            boardSpriteBatch.End();
            base.Draw(spriteBatch, elapsedMiliseconds);
        }
        public string GetPlayerTile(int playerIndex) => playerIndex switch
        {
            0 => "[o]",
            1 => "[x]",
            2 => "[.]",
            3 => "[-]",
            4 => "[v]",
            5 => "[~]",
            _ => throw new Exception()
        };
        public void EndGame(int winner)
        {
            bool draw = winner < 0;
            Instance.DebugStream.Log(">ending game with " + (draw ? "a draw" : "winner: " + state.Players[winner]));

            Instance.Environment.Overlayer.Activate("transition", Instance.VirtualBounds);
            Instance.DebugStream.Success();
        }
    }

    public struct SearchTarget
    {
        public bool PlayerBound { get; }
        public Point[] Nodes { get; }
        public int? TurnsRemaining { get; private set; }
        public string Id { get; }
        public Point[] InvertedNodes { get; }
        public SearchTarget(string id, Point[] nodes, bool playerBound = true, int? turnsRemaining = null)
        {
            Nodes = Point.Zero.ToSingleArray().Concat(nodes).ToArray();

            List<Point> rev = new List<Point>() { Point.Zero };
            foreach (Point node in nodes)
            {
                rev.Add(node * new Point(-1, -1));
            }
            InvertedNodes = rev.ToArray();

            //Nodes.CopyTo(InvertedNodes, 0);
            //Array.Reverse(InvertedNodes);

            PlayerBound = playerBound;
            TurnsRemaining = turnsRemaining;
            Id = id;

            Instance.DebugStream.Log("searchTarget with nodes {" + Nodes.ToJoinedString(", ") + "} created.");
        }
        public bool DecrementTurn()
        {
            if (!TurnsRemaining.HasValue) throw new Exception();
            else TurnsRemaining--;
            return TurnsRemaining == 0;
        }

    }
    /// <summary>
    /// Represent a single move.
    /// </summary>
    public struct Move
    {
        /// <summary>
        /// The origin of the <see cref="Move"/>, Y can often be infinitly large, X is limited by board width.
        /// </summary>
        public Point Origin { get; }
        /// <summary>
        /// Create a new move with set X and Y coordinates.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate</param>
        public Move(int x, int y) : this(new Point(x, y)) { }
        /// <summary>
        /// Create a new move from a <see cref="Point"/>.
        /// </summary>
        /// <param name="origin"></param>
        public Move(Point origin)
        {
            Origin = origin;
        }

        public Tile ToTile(int playerID, BoardState state) => ToTile(playerID, state.Tiles);
        public Tile ToTile(int playerID, Tile[,] tiles) => ToTile(playerID, tiles[Origin.X, Origin.Y].Attributes);
        public Tile ToTile(int playerID, HashSet<TileAttributeBase> OGattributes)
        {
            HashSet<TileAttributeBase> a = TileAttributes.GetNewPlayerAttributes(playerID);
            a.UnionWith(OGattributes);
            return new Tile(new Point(Origin.X, Origin.Y), null, a);
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return Origin == ((Move)obj!).Origin;
        }
        public static Move GetRandomMove(Move[] uniqueMoves)
        {
            Random random = new Random();
            return uniqueMoves[random.Next(0, uniqueMoves.Length)];
        }

        public static Move Invalid => new Move(-1, -1);
        public static implicit operator Point(Move m) => m.Origin;
        public static explicit operator Move(Point p) => new Move(p);


        public static bool operator ==(Move left, Move right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Move left, Move right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return Origin.GetHashCode();
        }
        public override string ToString()
        {
            return Origin.ToString();
        }
    }



    public class Tile : ICloneable
    {
        public Vector2 BoardPosition => TiledPosition.ToVector2() * BoardMultiplier;
        public RectangleF HitBox => new RectangleF(BoardPosition.X, BoardPosition.Y, BoardMultiplier, BoardMultiplier);
        public Point TiledPosition { get; }
        public TileType TileType { get; set; }
        public HashSet<TileAttributeBase> Attributes { get; set; }

        public Tile(Point tiledPosition, TileType? tileType, HashSet<TileAttributeBase>? attributes = null)
        {
            TileType = tileType ?? TileType.Void;
            TiledPosition = tiledPosition;
            Attributes = attributes ?? new HashSet<TileAttributeBase>();
        }
        public Tile Simulate(BoardState board)
        {
            int x = TiledPosition.X;
            int y = TiledPosition.Y;

            Point newPos = TiledPosition;

            if (this.HasAttribute<TileAttributes.TileAttributeGravDown>())
            {
                int depth = board.Tiles.GetLength(1) - y - 1;
                for (int i = 1; i <= depth; i++)
                {
                    Tile tile = board.Tiles[x, y + i];
                    if (!tile.IsSolid() && tile.HasGravity())
                    {
                        newPos = tile.TiledPosition;
                        continue;
                    }
                }
            }
            else if (this.HasAttribute<TileAttributes.TileAttributeGravUp>())
            {
                int depth = y;
                for (int i = 1; i <= depth; i++)
                {
                    Tile tile = board.Tiles[x, y - i];
                    if (!tile.IsSolid() && tile.HasGravity())
                    {
                        newPos = tile.TiledPosition;
                        continue;
                    }
                }
            }

            return new Tile(newPos, TileType, Attributes);
        }
        public bool HasAttribute(TileAttributeBase attribute) => Attributes.Contains(attribute);
        public bool HasAttribute(params TileAttributeBase[] attributes)
        {
            if (attributes.Length == 0) throw new Exception();
            for (int i = 0; i < attributes.Length; i++)
                if (HasAttribute(attributes[i])) return true;
            return false;
        }


        public static float BoardMultiplier => Board.TILE_SIZE;
        public static Tile[,] GetTiles(Point dimensions, bool random = false)
        {
            Tile[,] tiles = new Tile[dimensions.X, dimensions.Y];
            Random rnd = new Random();

            for (int x = 0; x < dimensions.X; x++)
            {
                for (int y = 0; y < dimensions.Y; y++)
                {
                    HashSet<TileAttributeBase> tileAttributes = new HashSet<TileAttributeBase>(TileAttributes.DefaultAttributes);

                    if (y < (int)(dimensions.Y / 2)) tileAttributes.ReplaceAttribute<TileAttributes.TileAttributeGravDown, TileAttributes.TileAttributeGravUp>();

                    tiles[x, y] = new Tile(new Point(x, y), TileType.Void, tileAttributes);
                }
            }
            return tiles;
        }
        public Tile Clone() => new Tile(TiledPosition, new TileType(TileType.Name, TileType.Texture), new HashSet<TileAttributeBase>(Attributes));
        object ICloneable.Clone()
        {
            return Clone();
        }
        public override int GetHashCode()
        {
            return TiledPosition.GetHashCode();
        }
    }
    public class TileType
    {
        public string Name { get; }
        public GameTexture Texture { get; }
        public TileType(string name, GameTexture texture)
        {
            Name = name;
            Texture = texture;
        }

        public static TileType Void => new TileType("void", Instance.Textures.GetReference("textures\\box_96"));
    }

}
