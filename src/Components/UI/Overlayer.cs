using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Betwixt;
using MonoGame.Extended;


using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Math = System.Math;
using MonoGame.Extended.Tweening;

#nullable enable

namespace STOLON
{
    public class OverlayEngine : GameComponent
    {
        private Dictionary<string, IOverlay> _overlays;
        private List<string> _initialized;

        public OverlayEngine() : base(GameEnvironment.Instance)
        {
            _overlays = new Dictionary<string, IOverlay>();
            _initialized = new List<string>();
        }

        public void AddOverlay<TOverlay>() where TOverlay : IOverlay, new() => AddOverlay(new TOverlay());
        public void AddOverlay(IOverlay overlay)
        {
            STOLON.Debug.Log(">adding overlay of id " + overlay.ID + ".");
            _overlays.Add(overlay.ID, overlay);
            STOLON.Debug.Success();
        }

        public void RemoveOverlay(string overlayId)
        {
            STOLON.Debug.Log(">removing overlay of id " + overlayId + ".");
            Deactivate(overlayId);
            _overlays.Remove(overlayId);
            STOLON.Debug.Success();
        }

        public void Activate(string overlayId, params object?[] args)
        {

            if (!_initialized.Contains(overlayId))
            {
                STOLON.Debug.Log(">[s]activating overlay of id " + overlayId + ".");
                _overlays[overlayId].Initialize(this, args);
                _initialized.Add(overlayId);
                STOLON.Debug.Success();
            }
        }

        public bool IsActive(IOverlay overlay) => IsActive(overlay.ID);
        public bool IsActive(string overlayId)
        {
            return _initialized.Contains(overlayId);
        }

        public void Deactivate(string overlayId) // ensure
        {
            if (_initialized.Contains(overlayId))
            {
                STOLON.Debug.Log(">deactivating overlay of id " + overlayId + ".");
                _overlays[overlayId].Reset();
                STOLON.Debug.Success();
            }
            _initialized.Remove(overlayId);
        }

        public override void Update(int elapsedMiliseconds)
        {
            IOverlay overlay;
            for (int i = 0; i < _initialized.Count; i++) // for all initialized overlays
            {
                overlay = _overlays[_initialized[i]];
                overlay.Update(elapsedMiliseconds);
                if (overlay.Ended)
                {
                    STOLON.Debug.Log(">deactivating and resetting ended overlay of id " + overlay.ID + ".");
                    Deactivate(overlay.ID);
                    STOLON.Debug.Success();
                }
            }
            base.Update(elapsedMiliseconds);
        }
        public override void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds)
        {
            for (int i = 0; i < _initialized.Count; i++)
            {
                _overlays[_initialized[i]].Draw(spriteBatch, elapsedMiliseconds);
            }
            base.Draw(spriteBatch, elapsedMiliseconds);
        }

        public static OverlayEngine Engine => STOLON.Environment.Overlayer;
    }

    public interface IOverlay
    {
        public void Initialize(OverlayEngine overlayer, params object?[] args);
        public void Update(int elapsedMiliseconds);
        public void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds);
        public void Reset();

        public string ID { get; }
        public bool Ended { get; }
    }
    public class LoadOverlay : IOverlay
    {
        public string ID => "loading";
        public bool Ended { get; private set; }

        private Texture2D lineTexture;

        private float _rotation;
        private float _rotationSpeed;
        private Vector2 _pos;
        private float _scale;

        public LoadOverlay()
        {
            lineTexture = STOLON.Textures.GetReference("textures\\loading1");
            _rotation = 0f;
            _scale = 0.20f;
            _rotationSpeed = 40f;

            _pos = STOLON.Instance.VirtualBounds.Size.ToVector2() + new Vector2(-lineTexture.Width, -lineTexture.Height) * _scale;

        }

        public void Initialize(OverlayEngine overlayer, params object?[] args)
        {
            _pos = (Vector2)((args.Length > 0 ? args[0] : null) ?? _pos);
        }

        public void Reset()
        {

        }

        public void Update(int elapsedMiliseconds)
        {
            _rotation += _rotationSpeed;
        }

        public void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds)
        {
            spriteBatch.Draw(lineTexture, _pos, null, Color.White, _rotation / 360f, new Vector2(lineTexture.Width / 2f, lineTexture.Height / 2f), _scale, SpriteEffects.None, 0);
            //spriteBatch.DrawCircle(pos, scale * lineTexture.Width * 0.8f, 15, Color.White, 2);
        }
    }
    public class TransitionDitherOverlay : IOverlay
    {
        public string ID => "transitionDither";

        public bool Ended => _ended;

        private Texture2D _ditherTexture;
        private int _pixelsToRemovePerFrame; // Number of pixels to turn transparent each frame
        private Color[] _pixelData; // Holds the pixel data for the dither texture
        private Random _random;
        private GraphicsDevice _graphicsDevice;
        private bool _ended;
        private int _resolution;
        private int _width;
        private int _height;
        private Tweener<float> _tweener;

        public TransitionDitherOverlay(GraphicsDevice graphicsDevice, int pixelsToRemovePerFrame = 11150, int time = 2, int resolution = 2)
        {
            this._pixelsToRemovePerFrame = pixelsToRemovePerFrame / (resolution);
            this._graphicsDevice = graphicsDevice;
            this._resolution = resolution;
            _random = new Random();


            _tweener = new Tweener<float>(1, this._pixelsToRemovePerFrame, 5f, Ease.Expo.In);
            _height = STOLON.Instance.VirtualDimensions.Y / resolution;
            _width = STOLON.Instance.VirtualDimensions.X / resolution;

            _ditherTexture = null!;
            _pixelData = null!;
            ResetTexture();
        }


        public void Initialize(OverlayEngine overlayer, params object?[] args)
        {
            STOLON.Audio.Play(STOLON.Audio.Library["randomize4"]);
        }

        public void ResetTexture()
        {
            _ditherTexture = new Texture2D(_graphicsDevice, _width, _height);
            _pixelData = new Color[_width * _height];
            for (int i = 0; i < _pixelData.Length; i++) _pixelData[i] = Color.White;
            _ditherTexture.SetData(_pixelData);
        }

        public void Reset()
        {
            _tweener.Reset();
            ResetTexture();
        }

        public void Update(int elapsedMiliseconds)
        {
            int removedPixels = 0;
            int dullPixels = 0;

            _tweener.Update(elapsedMiliseconds / 1000f);

            while (removedPixels < _tweener.Value)
            {
                int index = _random.Next(_pixelData.Length);
                if (_pixelData[index] == Color.White)
                {
                    _pixelData[index] = Color.Transparent;
                    removedPixels++;
                }
                else dullPixels++;
                if (dullPixels > 100000)
                {
                    _ended = true;
                    return;
                }
            }
            _ditherTexture.SetData(_pixelData);
        }

        public void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds)
        {
            spriteBatch.Draw(_ditherTexture, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, _resolution, SpriteEffects.None, 1f);
        }
    }
    public class TransitionOverlay : IOverlay
    {
        public string ID => "transition";

        public bool Ended { get; private set; }

        private Rectangle _area;
        private OverlayEngine _overlayer;
        private Tweener<float> _tweener;
        private string _text;

        private Rectangle _drawArea;
        private float _heightCoefficient;
        private Vector2 _textPos;
        private Action _action;

        private bool _hasHitMax;

        public TransitionOverlay()
        {
            _area = Rectangle.Empty;
            _drawArea = Rectangle.Empty;
            _overlayer = null!; // I know I know
            _tweener = null!; // yupyup
            Ended = false;
            _text = string.Empty;
            _textPos = Vector2.Zero;
            _action = () => { };
        }

        public void Update(int elapsedMiliseconds)
        {
            int desiredHeight = _area.Height;
            if (!_hasHitMax && _heightCoefficient > 0.999f)
            {
                _tweener = new Tweener<float>(1f, 0f, Duration / 1000f / 2, Ease.Sine.In);
                _action();
                _tweener.Start();
                _hasHitMax = true;
            }
            Ended = _hasHitMax && _heightCoefficient < 0.001f;

            _tweener.Update(elapsedMiliseconds / 1000f);

            _heightCoefficient = _tweener.Value;

            _drawArea = new Rectangle(_area.Location, new Point(_area.Width, (int)(desiredHeight * _heightCoefficient)));
            _textPos = Centering.MiddleXY(STOLON.Fonts[STOLON.SMALL_FONT_ID].FastMeasure(_text).ToPoint(), _drawArea, new Vector2(TextSizeMod));
            _textPos = new Vector2(_textPos.X, Math.Min(_textPos.Y, _drawArea.Height - STOLON.Fonts[STOLON.SMALL_FONT_ID].Dimensions.Y * TextSizeMod));

            Centering.OnPixel(ref _textPos);
        }

        public void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds)
        {
            spriteBatch.Draw(STOLON.Textures.Pixel, _drawArea, Color.Black);
            spriteBatch.DrawRectangle(_drawArea, Color.White);
            spriteBatch.DrawString(STOLON.Fonts[STOLON.SMALL_FONT_ID], _text, _textPos, Color.White, 0f, Vector2.Zero, STOLON.Fonts[STOLON.SMALL_FONT_ID].Scale * TextSizeMod, SpriteEffects.None, 0f);
        }

        public void Initialize(OverlayEngine overlayer, params object?[] args)
        {
            Ended = false;
            _tweener = new Tweener<float>(0f, 1f, Duration / 1000f / 2, Ease.Sine.Out);
            _area = (Rectangle)((args.Length > 0 ? args[0] : null) ?? STOLON.Instance.VirtualBounds);
            _text = (string)(args[2] ?? string.Empty);
            _action = (Action)(args[1]! ?? _action);

            _tweener.Start();

            this._overlayer = overlayer;
        }

        public void Reset()
        {
            _tweener.Stop();
            _tweener.Reset();
            _hasHitMax = false;
            _heightCoefficient = 0f;
        }

        public static int Duration => 4000;
        public static float TextSizeMod => 3;
    }
}
