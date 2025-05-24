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


using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Math = System.Math;
using RectangleF = MonoGame.Extended.RectangleF;
using System.Diagnostics;
using System.Xml.Linq;
using MonoGame.Extended.Tiled;

#nullable enable

namespace STOLON
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
        public float ZoomIntensity => (Zoom - _desiredZoom) / MaxDeltaZoom;
        public Vector2 BoardCenter => _state.Tiles[_state.Tiles.GetLength(0) / 2, _state.Tiles.GetLength(1) / 2].BoardPosition;
        public float SmoothnessModifier => 0.003f;
        public int TurnNumber { get; private set; }
        public ref BoardState State => ref _state;
        public ReadOnlyDictionary<string, SearchTarget> SearchTargets => new ReadOnlyDictionary<string, SearchTarget>(_searchTargets);
        public BoardState InitialState { get; }
        public Stack<BoardState> History { get; private set; }

        public bool MouseIsOnBoard => STOLON.Input.Domain == GameInputManager.MouseDomain.Board;
        public Vector2 WorldMousePos { get; private set; }

        private SpriteBatch _boardSpriteBatch;
        private BoardState _state;

        int _mouseStateCoefficient;
        private float _desiredZoom;
        private Vector2 _desiredCameraPos;
        bool _firstFrame;

        private Task? _computerMoveTask;
        private bool _locked;

        private BoardState.SearchTargetCollection _searchTargets;
        private const float CONF_ZOOM_COEFFICIENT = 0.98f; // 0.98f

        public UniqueMoveBoardMap UniqueMoveBoardMap { get; }

        public Board(BoardState conf) : base(STOLON.Environment)
        {
            Camera = new Camera2D();
            TurnNumber = 0;

            _state = conf;
            _boardSpriteBatch = new SpriteBatch(STOLON.Instance.GraphicsDevice);
            _desiredZoom = MathF.Max(0.45f, CONF_ZOOM_COEFFICIENT * (4f / conf.Dimensions.X)); // does not change.
            _desiredCameraPos = BoardCenter;
            Camera.Position = _desiredCameraPos;
            _searchTargets = conf.WinSearchTargets;
            _computerMoveTask = null!;
            _firstFrame = false;

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
            _locked = true;
        }
        public void Unlock()
        {
            _locked = false;
        }

        /// <summary>
        /// Update method. 
        /// </summary>
        /// <param name="elapsedMiliseconds"></param>
        public override void Update(int elapsedMiliseconds)
        {
            if (!_firstFrame) _firstFrame = true;

            WorldMousePos = Camera.Unproject(STOLON.Input.VirtualMousePos);

            _mouseStateCoefficient = STOLON.Input.CurrentMouse.GetMouseStateCoefficient();

            if (STOLON.Input.IsPressed(Keys.LeftShift))
            {
                if (_mouseStateCoefficient == 0) _mouseStateCoefficient = 1;
                if (STOLON.Input.IsPressed(Keys.A))
                    _desiredCameraPos.X -= 1;
                if (STOLON.Input.IsPressed(Keys.D))
                    _desiredCameraPos.X += 1;
                if (STOLON.Input.IsPressed(Keys.W))
                    _desiredCameraPos.Y -= 1;
                if (STOLON.Input.IsPressed(Keys.S))
                    _desiredCameraPos.Y += 1;
            }

            if (STOLON.Input.IsPressed(GameInputManager.MouseButton.Right)) _desiredCameraPos += (STOLON.Input.PreviousMouse.Position - STOLON.Input.CurrentMouse.Position).ToVector2();
            Zoom += (_desiredZoom - Zoom) * 0.1f + _mouseStateCoefficient * SmoothnessModifier;
            Camera.Position += (_desiredCameraPos - Camera.Position) * 0.1f + (WorldMousePos - Camera.Position) * SmoothnessModifier * Math.Abs(_mouseStateCoefficient);
            Camera.Zoom = Zoom;

            Listen();

            //if (StolonGame.Instance.UserInterface.UIElementUpdateData["restartBoard"].IsClicked)
            //{
            //    StolonGame.Instance.Environment.Overlayer.Activate("transition", null, () =>
            //            {
            //                Reset();
            //            }, "Resetting the Board..");
            //}
            //if (StolonGame.Instance.UserInterface.UIElementUpdateData["skipMove"].IsClicked) EndMove();
            //if (StolonGame.Instance.UserInterface.UIElementUpdateData["boardSearch"].IsClicked)
            //{
            //    int ret = State.SearchAny();
            //    if (ret != -1)
            //        StolonGame.Instance.Environment.Overlayer.Activate("transition", null, () =>
            //            {
            //                Reset();
            //            }, "4 Connected found for player " + GetPlayerTile(ret) + "!");

            //}
            //if (StolonGame.Instance.UserInterface.UIElementUpdateData["centerCamera"].IsClicked) desiredCameraPos = BoardCenter;
            //if (StolonGame.Instance.UserInterface.UIElementUpdateData["undoMove"].IsClicked)
            //{
            //    if (state.Players.Any(p => p.IsComputer))
            //    {
            //        StolonGame.Instance.Environment.UI.Textframe.Queue(new DialogueInfo(StolonGame.Instance.Environment, "Not valid when against AI but coming soon!"));
            //    }
            //    Undo();
            //}
            if (STOLON.Input.IsClicked(Keys.Z)) // debug keys
            {
            }
            if (STOLON.Input.IsClicked(Keys.X)) { }
            if (STOLON.Input.IsClicked(Keys.C)) { }
            //if (StolonGame.Instance.UserInterface.UIElementUpdateData["exitGame"].IsClicked) StolonGame.Instance.SLExit();

            //Instance.UserInterface.UIElements["currentPlayer"].Text = "Current: " + state.CurrentPlayer.Name + " " + GetPlayerTile(state.CurrentPlayerID);

            base.Update(elapsedMiliseconds);
        }
        public void Undo()
        {
            STOLON.Debug.Log(">attempting move undo");
            _state.Undo();
            STOLON.Debug.Success();

        }
        public void AfterMove()
        {
            STOLON.Audio.Play(STOLON.Audio.Library["select4"]);
        }
        public bool Listen()
        {
            if (_locked)
            {
                _computerMoveTask = null;
                STOLON.Environment.Overlayer.Deactivate("loading");
                return false;
            }
            if (_computerMoveTask != null && _computerMoveTask.IsCompletedSuccessfully)
            {
                STOLON.Environment.Overlayer.Deactivate("loading");
                _computerMoveTask = null;
            }
            if (State.CurrentPlayer.IsComputer)
            {
                _computerMoveTask ??= new Task(() =>
                {
                    State.CurrentPlayer.Computer!.DoMove(this);
                    AfterMove();
                });

                if (_computerMoveTask.Status == TaskStatus.Created)
                {
                    _computerMoveTask.Start();
                    STOLON.Environment.Overlayer.Activate("loading");
                }
            }
            else if (Utils.IsMouseClicked(STOLON.Input.CurrentMouse, STOLON.Input.PreviousMouse) && MouseIsOnBoard)
            {
                STOLON.Debug.Log(">attempting board alter after mouseclick");
                Move? move = null;
                for (int x = 0; x < _state.Tiles.GetLength(0); x++)
                    for (int y = 0; y < _state.Tiles.GetLength(1); y++)
                        if (_state.Tiles[x, y].HitBox.Contains(WorldMousePos) && !_state.Tiles[x, y].IsSolid())
                        {
                            move = new Move(x, y);
                            break;
                        }
                if (move.HasValue)
                {
                    History.Push(State.DeepCopy());
                    State.Alter(move!.Value, true);
                    AfterMove();
                    STOLON.Debug.Success();
                    return true;
                }
                else STOLON.Debug.Fail();
            }
            return false;
        }
        public void Reset()
        {
            STOLON.Debug.Log(">resetting board");

            _computerMoveTask = null;
            State = InitialState.DeepCopy();


            STOLON.Debug.Success();
        }
        public void EndMove()
        {
            State.GoNextPlayer();
        }
        public override void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds)
        {
            _boardSpriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: Camera.View);
            for (int x = 0; x < _state.Dimensions.X; x++)
                for (int y = 0; y < _state.Dimensions.Y; y++)
                {
                    Tile tile = _state.Tiles[x, y];
                    _boardSpriteBatch.Draw(tile.TileType.Texture, tile.BoardPosition, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    int playerid = tile.GetOccupiedByPlayerID();
                    if (playerid != -1)
                    {
                        _boardSpriteBatch.Draw(STOLON.Textures.GetReference("textures\\player" + playerid + "item_96"), tile.BoardPosition, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    }
                    else if (tile.HasAttribute<TileAttributes.TileAttributeGravDown>()) _boardSpriteBatch.DrawString(STOLON.Fonts["fonts\\smollerMono"], string.Empty, tile.BoardPosition + new Vector2(10), Color.White);
                    else if (tile.HasAttribute<TileAttributes.TileAttributeGravUp>()) _boardSpriteBatch.DrawString(STOLON.Fonts["fonts\\smollerMono"], ("^").ToString(), (tile.BoardPosition + new Vector2(10)).PixelLock(Camera), Color.White);
                    else _boardSpriteBatch.DrawString(STOLON.Fonts["fonts\\smollerMono"], ("Z").ToString(), tile.BoardPosition + new Vector2(10), Color.White);
                }
            _boardSpriteBatch.End();
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
            STOLON.Debug.Log(">ending game with " + (draw ? "a draw" : "winner: " + _state.Players[winner]));

            STOLON.Environment.Overlayer.Activate("transition", STOLON.Instance.VirtualBounds);
            STOLON.Debug.Success();
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

            PlayerBound = playerBound;
            TurnsRemaining = turnsRemaining;
            Id = id;

            STOLON.Debug.Log("searchTarget with nodes {" + Nodes.ToJoinedString(", ") + "} created.");
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

        public static TileType Void => new TileType("void", STOLON.Textures.GetReference("textures\\box_96"));
    }

}
