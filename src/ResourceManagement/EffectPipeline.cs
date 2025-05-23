using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;

namespace STOLON
{
    public interface IEffect
    {
        public Effect Shader { get; }
        public void SetParameters(Texture2D source);
    }

    public class EffectPipeline : IDisposable
    {
        private readonly GraphicsDevice _graphics;
        private readonly SpriteBatch _spriteBatch;
        private readonly RenderTarget2D _sceneTarget;
        private readonly List<IEffect> _effects;
        private readonly RenderTarget2D _rt1;
        private readonly RenderTarget2D _rt2;

        public EffectPipeline(GraphicsDevice graphics, SpriteBatch sb, int w, int h)
        {
            _graphics = graphics;
            _spriteBatch = sb;
            _rt1 = new RenderTarget2D(graphics, w, h);
            _rt2 = new RenderTarget2D(graphics, w, h);
            _sceneTarget = _rt1;
            _effects = new List<IEffect>();
        }

        public void AddEffect(IEffect effect) => _effects.Add(effect);

        /// <summary>
        /// Call this at the top of Draw: all subsequent sprite-draws get routed into
        /// an offscreen buffer instead of the backbuffer.
        /// </summary>
        public void BeginScene()
        {
            _graphics.SetRenderTarget(_sceneTarget);
            _graphics.Clear(Color.LightSeaGreen);
        }

        /// <summary>
        /// Call this at the end of Draw: applies each effect in order, then
        /// presents the final result to the screen.
        /// </summary>
        public void EndScene(Matrix? transform = null)
        {
            RenderTarget2D src = _sceneTarget;
            RenderTarget2D dst = _rt2;

            foreach (IEffect effect in _effects)
            {
                // pass src into the effect
                effect.SetParameters(src);

                _graphics.SetRenderTarget(dst);
                _graphics.Clear(Color.LightSeaGreen);

                _spriteBatch.Begin(
                    SpriteSortMode.Deferred,
                    BlendState.Opaque,
                    SamplerState.PointClamp,
                    DepthStencilState.None,
                    RasterizerState.CullCounterClockwise,
                    effect.Shader
                );
                _spriteBatch.Draw(src, Vector2.Zero, Color.White);
                _spriteBatch.End();

                // ping-pong
                var tmp = src;
                src = dst;
                dst = tmp;
            }

            // finally, draw `src` (which now holds the last pass) to the backbuffer
            _graphics.SetRenderTarget(null);
            _spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                DepthStencilState.None,
                RasterizerState.CullCounterClockwise,
                transformMatrix: transform
            );
            _spriteBatch.Draw(src, Vector2.Zero, Color.White);
            _spriteBatch.End();
        }


        public void Dispose()
        {
            _rt1.Dispose();
            _rt2.Dispose();
            foreach (IEffect post in _effects) (post as IDisposable)?.Dispose();
        }
    }

    public class ReplaceColorEffect : IEffect
    {
        public Effect Shader { get; }

        public Color Target1 { get; set; }
        public Color Replace1 { get; set; }
        public Color Target2 { get; set; }
        public Color Replace2 { get; set; }

        public ReplaceColorEffect(Effect effect) => Shader = effect;

        public void SetParameters(Texture2D source)
        {
            Shader.Parameters["InputTexture"].SetValue(source);
            Shader.Parameters["dcolor1"].SetValue(Target1.ToVector4());
            Shader.Parameters["color1"].SetValue(Replace1.ToVector4());
            Shader.Parameters["dcolor2"].SetValue(Target2.ToVector4());
            Shader.Parameters["color2"].SetValue(Replace2.ToVector4());
        }
    }
}
