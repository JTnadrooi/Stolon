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
using AsitLib.XNA;
using MonoGame.Extended;
using static Stolon.StolonGame;

using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Math = System.Math;
using RectangleF = MonoGame.Extended.RectangleF;
using System.Diagnostics;

#nullable enable

namespace Stolon
{
    /// <summary>
    /// The representor of the board in the STOLON environment.
    /// </summary>
    public class Board : AxComponent
    {
        public Camera2D Camera { get; }
        public float Zoom { get; private set; }

        public float MaxDeltaZoom => SmoothnessModifier * 10f;
        public float ZoomIntensity => (Zoom - desiredZoom) / MaxDeltaZoom;
        public Vector2 BoardCenter => state.Tiles[state.Tiles.GetLength(0) / 2, state.Tiles.GetLength(1) / 2].BoardPosition;
        public float SmoothnessModifier => 0.003f;
        public int TurnNumber { get; private set; }
        public ref BoardState State => ref state;
        public ReadOnlyDictionary<string, SearchTarget> SearchTargets => new ReadOnlyDictionary<string, SearchTarget>(searchTargets);
        public BoardState InitialState { get; }
        public Stack<BoardState> History { get; private set; }


        private SpriteBatch boardSpriteBatch;
        private BoardState state;

        int mouseStateCoefficient;
        private float desiredZoom;
        private Vector2 desiredCameraPos;
        bool firstFrame;

        private Task? computerMoveTask;
        private const bool hasComputer = false;

        private RectangleF[] rowHitBoxes;
        private BoardState.SearchTargetCollection searchTargets;
        private const float confZoomCoefficient = 0.98f; // 0.98f

        public UniqueMoveBoardMap UniqueMoveBoardMap { get; }

        public Board(SLScene source, BoardState conf) : base(source)
        {
            Camera = new Camera2D();
            TurnNumber = 0;

            state = conf;
            boardSpriteBatch = new SpriteBatch(Instance.GraphicsDevice);
            desiredZoom = MathF.Max(0.45f, confZoomCoefficient * (4f / conf.Dimensions.X)); // does not change.
            rowHitBoxes = new RectangleF[conf.Dimensions.X];
            desiredCameraPos = BoardCenter;
            Camera.Position = desiredCameraPos;
            searchTargets = conf.WinSearchTargets;
            computerMoveTask = null!;
            firstFrame = false;
            Zoom = 1f;
            InitialState = conf.DeepCopy();
            History = new Stack<BoardState>();
            History.Push(conf.DeepCopy());

            UniqueMoveBoardMap = new UniqueMoveBoardMap();

            for (int x = 0; x < conf.Dimensions.X; x++)
            {
                Vector2 topleft = new Vector2(x * 64, 0);
                rowHitBoxes[x] = new RectangleF(topleft.X, topleft.Y, 64, 64 * conf.Dimensions.Y);
            }
        }

        /// <summary>
        /// Update method. 
        /// </summary>
        /// <param name="elapsedMiliseconds"></param>
        public override void Update(int elapsedMiliseconds)
        {
            if (!firstFrame) firstFrame = true;

            Vector2 mousepos = SLMouse.VirualPosition;
            Vector2 worldMousePos = Camera.Unproject(mousepos);
            bool isOnBoard = SLMouse.Domain == SLMouse.MouseDomain.Board;

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
            Camera.Position += (desiredCameraPos - Camera.Position) * 0.1f + (worldMousePos - Camera.Position) * SmoothnessModifier * Math.Abs(mouseStateCoefficient);
            Camera.Zoom = Zoom;

            #region computer_old
            // if (hasComputer)
            // {
            //     if (CurrentPlayer.IsComputer)
            //     {
            //         if (computerMoveTask == null)
            //         {
            //             computerMoveTask = Task.Run(() => CurrentPlayer.Computer!.DoMove(this));
            //             Instance.Environment.Overlayer.Activate("loading");
            //             //computerMoveTask.Start();
            //         }

            //     }
            //     if (computerMoveTask != null && computerMoveTask.IsCompleted)
            //     {
            //         Instance.Environment.Overlayer.Deactiviate("loading");
            //         computerMoveTask.Dispose();
            //         computerMoveTask = null;
            //     }
            //     else if (AsitGame.IsMouseClicked(DLMouse.CurrentState, DLMouse.PreviousState) && DLMouse.Domain == DLMouse.MouseDomain.Board)
            //     {
            //         int toDropAt = -1;
            //         for (int i = 0; i < rowHitBoxes.Length; i++)
            //         {
            //             if (rowHitBoxes[i].Contains(worldMousePos.ToPoint()))
            //             {
            //                 toDropAt = i;
            //                 continue;
            //             }
            //             else
            //             {

            //             }
            //         }
            //         if (toDropAt != -1)
            //         {
            //             if (Alter(new Move(toDropAt, dimensions.Y - 1), CurrentPlayer))
            //             {
            //                 EndMove();
            //             }
            //         }
            //     }
            // }
            #endregion

            #region expectMove

            if (State.CurrentPlayer.IsComputer)
            {
                computerMoveTask ??= new Task(() => State.CurrentPlayer.Computer!.DoMove(this));
                if (computerMoveTask.Status == TaskStatus.WaitingToRun)
                {
                    computerMoveTask.Start();
                    Instance.Environment.Overlayer.Activate("loading");
                }
                if (computerMoveTask.Status == TaskStatus.RanToCompletion) Instance.Environment.Overlayer.Deactivate("loading");
            }
            else if (AsitGame.IsMouseClicked(SLMouse.CurrentState, SLMouse.PreviousState) && SLMouse.Domain == SLMouse.MouseDomain.Board)
            {
                Instance.DebugStream.WriteLine("Attempting board alter after mouseclick..");
                Move? move = null;
                for (int x = 0; x < state.Tiles.GetLength(0); x++)
                    for (int y = 0; y < state.Tiles.GetLength(1); y++)
                        if (state.Tiles[x, y].HitBox.Contains(worldMousePos) && !state.Tiles[x, y].IsSolid())
                        {
                            move = new Move(x, y);
                            break;
                        }
                if (move.HasValue)
                {
                    History.Push(State.DeepCopy());
                    State.Alter(move!.Value, true);
                    Instance.DebugStream.Succes(1);
                }
                else Instance.DebugStream.Fail(1);
            }
            #endregion


            if (Instance.UserInterface.UIElementUpdateData["restartBoard"].IsClicked)
            {
                Instance.Environment.Overlayer.Activate("transition", null, () =>
                        {
                            Reset();
                        }, "");
            }
            if (Instance.UserInterface.UIElementUpdateData["skipMove"].IsClicked) EndMove();
            if (Instance.UserInterface.UIElementUpdateData["boardSearch"].IsClicked)
            {
                int ret = State.SearchAny();
                if (ret != -1)
                    Instance.Environment.Overlayer.Activate("transition", null, () =>
                        {
                            Reset();
                        }, "4 Connected found for player " + GetPlayerTile(ret) + "!");

            }
            if (Instance.UserInterface.UIElementUpdateData["centerCamera"].IsClicked) desiredCameraPos = BoardCenter;
            if (Instance.UserInterface.UIElementUpdateData["undoMove"].IsClicked) Undo();
            if (SLKeyboard.IsClicked(Keys.Z))
            {
                Console.WriteLine(UniqueMoveBoardMap.GetAllMoves(state).ToJoinedString(", "));
            }
            if (SLKeyboard.IsClicked(Keys.X))
            {
                state.Undo();
            }
            if (SLKeyboard.IsClicked(Keys.C))
            {
                Stopwatch a = new Stopwatch();
                a.Start();
                // for (int i = 0; i < 16; i++)
                // {
                //     Console.WriteLine(i);
                //     Console.WriteLine(UniqueMoveBoardMap.GetMovePos(i));
                //     Console.WriteLine(UniqueMoveBoardMap.IsValid(i, state));
                // }

                //Console.WriteLine(GoldsilkCom.Evaluate(state, 1));
                Console.WriteLine("yooooooooooo");
                GoldsilkCom.count = 0;
                Console.WriteLine(GoldsilkCom.Search(state, UniqueMoveBoardMap));
                Console.WriteLine(GoldsilkCom.count + " counted.");
                Console.WriteLine("and it took " + a.ElapsedMilliseconds + "ms.");



                a.Stop();

                Console.WriteLine("rate: " + (a.ElapsedMilliseconds / (float)GoldsilkCom.count) + " ms/i.");


            }
            if (Instance.UserInterface.UIElementUpdateData["exitGame"].IsClicked) Instance.SLExit();

            Instance.UserInterface.UIElements["currentPlayer"].Text = "Current: " + state.CurrentPlayer.Name + " " + GetPlayerTile(state.CurrentPlayerID);

            base.Update(elapsedMiliseconds);
        }
        public void Undo()
        {
            Instance.DebugStream.WriteLine("Attempting move undo..");

            if (History.TryPop(out BoardState temp))
            {
                State = temp;
                Instance.DebugStream.Succes(1);
            }
            else Instance.DebugStream.Fail(1);

        }
        public void Reset()
        {
            Instance.DebugStream.WriteLine("Resetting the board..");
            State = InitialState.DeepCopy();
            Instance.DebugStream.Succes(1);
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
                        boardSpriteBatch.Draw(Instance.Textures.GetReference("textures\\player" + playerid + "item"), tile.BoardPosition, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    }
                    else if (tile.HasAttribute<TileAttribute.TileAttributeGravDown>()) boardSpriteBatch.DrawString(SLEnvironment.Font, string.Empty, tile.BoardPosition + new Vector2(10), Color.White);
                    else if (tile.HasAttribute<TileAttribute.TileAttributeGravUp>()) boardSpriteBatch.DrawString(SLEnvironment.Font, ("^").ToString(), (tile.BoardPosition + new Vector2(10)).PixelLock(Camera), Color.White);
                    else boardSpriteBatch.DrawString(SLEnvironment.Font, ("Z").ToString(), tile.BoardPosition + new Vector2(10), Color.White);
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
            Instance.DebugStream.WriteLine("[s]ending game with " + (draw ? "a draw" : "winner: " + state.Players[winner]));

            Instance.Environment.Overlayer.Activate("transition", Instance.VirtualBounds);
        }
    }

    public struct SearchTarget
    {
        public bool PlayerBound { get; }
        public Point[] Nodes { get; }
        public int? TurnsRemaining { get; private set; }
        public string Id { get; }
        public SearchTarget(string id, Point[] nodes, bool playerBound = true, int? turnsRemaining = null)
        {
            Nodes = nodes.Concat(Point.Zero.ToSingleArray()).ToArray();
            PlayerBound = playerBound;
            TurnsRemaining = turnsRemaining;
            Id = id;

            Instance.DebugStream.WriteLine("\tsearchTarget with nodes {" + Nodes.ToJoinedString(", ") + "} created.");
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

            if (this.HasAttribute<TileAttribute.TileAttributeGravDown>())
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
            else if (this.HasAttribute<TileAttribute.TileAttributeGravUp>())
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
        public bool HasAttribute(params TileAttributeBase[] attributes)
        {
            if (attributes.Length == 0) throw new Exception();
            for (int i = 0; i < attributes.Length; i++)
                if (Attributes.Contains(attributes[i])) return true;
            return false;
        }


        public static float BoardMultiplier => 64f;
        public static Tile[,] GetTiles(Point dimensions, bool random = false)
        {
            Tile[,] tiles = new Tile[dimensions.X, dimensions.Y];
            Random rnd = new Random();

            for (int x = 0; x < dimensions.X; x++)
            {
                for (int y = 0; y < dimensions.Y; y++)
                {
                    HashSet<TileAttributeBase> tileAttributes = new HashSet<TileAttributeBase>(TileAttribute.DefaultAttributes);

                    if (y < (int)(dimensions.Y / 2)) tileAttributes.ReplaceAttribute<TileAttribute.TileAttributeGravDown, TileAttribute.TileAttributeGravUp>();

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
    }
    public class TileType
    {
        public string Name { get; }
        public AxTexture Texture { get; }
        public TileType(string name, AxTexture texture)
        {
            Name = name;
            Texture = texture;
        }

        public static TileType Void => new TileType("void", Instance.Textures.GetReference("textures\\box2"));
    }

}
