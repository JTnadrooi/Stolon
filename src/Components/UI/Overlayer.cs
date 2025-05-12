using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Betwixt;
using MonoGame.Extended;
using static Stolon.StolonGame;

using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Math = System.Math;
using MonoGame.Extended.Tweening;

#nullable enable

namespace Stolon
{
    public class OverlayEngine : GameComponent
    {
        private Dictionary<string, IOverlay> overlays;
        private List<string> initialized;

        public OverlayEngine() : base(StolonEnvironment.Instance)
        {
            overlays = new Dictionary<string, IOverlay>();
            initialized = new List<string>();
        }

        public void AddOverlay<TOverlay>() where TOverlay : IOverlay, new() => AddOverlay(new TOverlay());
        public void AddOverlay(IOverlay overlay)
        {
            Instance.DebugStream.Log(">adding overlay of id " + overlay.ID + ".");
            overlays.Add(overlay.ID, overlay);
            Instance.DebugStream.Success();
        }

        public void RemoveOverlay(string overlayId)
        {
            Instance.DebugStream.Log(">removing overlay of id " + overlayId + ".");
            Deactivate(overlayId);
            overlays.Remove(overlayId);
            Instance.DebugStream.Success();
        }

        public void Activate(string overlayId, params object?[] args)
        {
            
            if (!initialized.Contains(overlayId))
            {
                Instance.DebugStream.Log(">[s]activating overlay of id " + overlayId + ".");
                overlays[overlayId].Initialize(this, args);
                initialized.Add(overlayId);
                Instance.DebugStream.Success();
            }
        }

        public bool IsActive(IOverlay overlay) => IsActive(overlay.ID);
        public bool IsActive(string overlayId)
        {
            return initialized.Contains(overlayId);
        }

        public void Deactivate(string overlayId) // ensure
        {
            if (initialized.Contains(overlayId))
            {
                Instance.DebugStream.Log(">deactivating overlay of id " + overlayId + ".");
                overlays[overlayId].Reset();
                Instance.DebugStream.Success();
            }
            initialized.Remove(overlayId);
        }

        public override void Update(int elapsedMiliseconds)
        {
            IOverlay overlay;
            for (int i = 0; i < initialized.Count; i++) // for all initialized overlays
            {
                overlay = overlays[initialized[i]];
                overlay.Update(elapsedMiliseconds);
                if (overlay.Ended)
                {
                    Instance.DebugStream.Log(">deactivating and resetting ended overlay of id " + overlay.ID + ".");
                    Deactivate(overlay.ID);
                    Instance.DebugStream.Success();
                }
            }
            base.Update(elapsedMiliseconds);
        }
        public override void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds)
        {
            for (int i = 0; i < initialized.Count; i++)
            {
                overlays[initialized[i]].Draw(spriteBatch, elapsedMiliseconds);
            }
            base.Draw(spriteBatch, elapsedMiliseconds);
        }

        public static OverlayEngine Engine => Instance.Environment.Overlayer;
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

        private float rotation;
        private float rotationSpeed;
        private Vector2 pos;
        private float scale;

        public LoadOverlay()
        {
            lineTexture = Instance.Textures.GetReference("textures\\loading1");
            rotation = 0f;
            scale = 0.20f;
            rotationSpeed = 40f;

            pos = Instance.VirtualBounds.Size.ToVector2() + new Vector2(-lineTexture.Width, -lineTexture.Height) * scale;

        }

        public void Initialize(OverlayEngine overlayer, params object?[] args)
        {
            pos = (Vector2)((args.Length > 0 ? args[0] : null) ?? pos);
        }

        public void Reset()
        {

        }

        public void Update(int elapsedMiliseconds)
        {
            rotation += rotationSpeed;
        }

        public void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds)
        {
            spriteBatch.Draw(lineTexture, pos, null, Color.White, rotation / 360f, new Vector2(lineTexture.Width / 2f, lineTexture.Height / 2f), scale, SpriteEffects.None, 0);
            //spriteBatch.DrawCircle(pos, scale * lineTexture.Width * 0.8f, 15, Color.White, 2);
        }
    }
    public class TransitionDitherOverlay : IOverlay
    {
        public string ID => "transitionDither";

        public bool Ended => ended;

        private Texture2D ditherTexture;
        private int pixelsToRemovePerFrame; // Number of pixels to turn transparent each frame
        private Color[] pixelData; // Holds the pixel data for the dither texture
        private Random random;
        private GraphicsDevice graphicsDevice;
        private bool ended;
        private int resolution;
        private int width;
        private int height;
        private Tweener<float> tweener;

        public TransitionDitherOverlay(GraphicsDevice graphicsDevice, int pixelsToRemovePerFrame = 11150, int time = 2, int resolution = 2)
        {
            this.pixelsToRemovePerFrame = pixelsToRemovePerFrame / (resolution);
            this.graphicsDevice = graphicsDevice;
            this.resolution = resolution;
            random = new Random();


            tweener = new Tweener<float>(1, this.pixelsToRemovePerFrame, 5f, Ease.Expo.In);
            height = Instance.VirtualDimensions.Y / resolution;
            width = Instance.VirtualDimensions.X / resolution;

            ditherTexture = null!;
            pixelData = null!;
            ResetTexture();
        }


        public void Initialize(OverlayEngine overlayer, params object?[] args)
        {
            AudioEngine.Audio.Play(AudioEngine.AudioLibrary["randomize4"]);
        }

        public void ResetTexture()
        {
            ditherTexture = new Texture2D(graphicsDevice, width, height);
            pixelData = new Color[width * height];
            for (int i = 0; i < pixelData.Length; i++) pixelData[i] = Color.White;
            ditherTexture.SetData(pixelData);
        }

        public void Reset()
        {
            tweener .Reset();
            ResetTexture();
        }

        public void Update(int elapsedMiliseconds)
        {
            int removedPixels = 0;
            int dullPixels = 0;

            tweener.Update(elapsedMiliseconds / 1000f);

            while (removedPixels < tweener.Value)
            {
                int index = random.Next(pixelData.Length);
                if (pixelData[index] == Color.White)
                {
                    pixelData[index] = Color.Transparent;
                    removedPixels++;
                }
                else dullPixels++;
                if (dullPixels > 100000)
                {
                    ended = true;
                    return;
                }
            }
            ditherTexture.SetData(pixelData);
        }

        public void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds)
        {
            spriteBatch.Draw(ditherTexture, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, resolution, SpriteEffects.None, 1f);
        }
    }
    public class TransitionOverlay : IOverlay
    {
        public string ID => "transition";

        public bool Ended { get; private set; }

        private Rectangle area;
        private OverlayEngine overlayer;
        private Tweener<float> tweener;
        private string text;

        private Rectangle drawArea;
        private float heightCoefficient;
        private Vector2 textPos;
        private Action action;

        private bool hasHitMax;

        public TransitionOverlay()
        {
            area = Rectangle.Empty;
            drawArea = Rectangle.Empty;
            overlayer = null!; // I know I know
            tweener = null!; // yupyup
            Ended = false;
            text = string.Empty;
            textPos = Vector2.Zero;
            action = () => { };
        }

        public void Update(int elapsedMiliseconds)
        {
            int desiredHeight = area.Height;
            if (!hasHitMax && heightCoefficient > 0.999f)
            {
                tweener = new Tweener<float>(1f, 0f, Duration / 1000f / 2, Ease.Sine.In);
                action();
                tweener.Start();
                hasHitMax = true;
            }
            Ended = hasHitMax && heightCoefficient < 0.001f;

            tweener.Update(elapsedMiliseconds / 1000f);

            heightCoefficient = tweener.Value;

            drawArea = new Rectangle(area.Location, new Point(area.Width, (int)(desiredHeight * heightCoefficient)));
            textPos = Centering.MiddleXY(Instance.Fonts["fonts\\smollerMono"].FastMeasureString(text).ToPoint(), drawArea, new Vector2(TextSizeMod));
            textPos = new Vector2(textPos.X, Math.Min(textPos.Y, drawArea.Height - Instance.Fonts["fonts\\smollerMono"].Dimensions.Y * TextSizeMod));

            Centering.OnPixel(ref textPos);
        }

        public void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds)
        {
            spriteBatch.Draw(Instance.Textures.Pixel, drawArea, Color.Black);
            spriteBatch.DrawRectangle(drawArea, Color.White);
            spriteBatch.DrawString(Instance.Fonts["fonts\\smollerMono"], text, textPos, Color.White, 0f, Vector2.Zero, Instance.Fonts["fonts\\smollerMono"].Scale * TextSizeMod, SpriteEffects.None, 0f);
        }

        public void Initialize(OverlayEngine overlayer, params object?[] args)
        {
            Ended = false;
            tweener = new Tweener<float>(0f, 1f, Duration / 1000f / 2, Ease.Sine.Out);
            area = (Rectangle)((args.Length > 0 ? args[0] : null) ?? Instance.VirtualBounds);
            text = (string)(args[2] ?? string.Empty);
            action = (Action)(args[1]! ?? action);

            tweener.Start();

            this.overlayer = overlayer;
        }

        public void Reset()
        {
            tweener.Stop();
            tweener.Reset();
            hasHitMax = false;
            heightCoefficient = 0f;
        }

        public static int Duration => 4000;
        public static float TextSizeMod => 3;
    }
}
