
using Betwixt;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Linq;

using Color = Microsoft.Xna.Framework.Color;
using Math = System.Math;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

#nullable enable

namespace Stolon
{
    public struct DialogueDrawArgs
    {
        public DialogueInfo Source { get; }
        public int[] TimeMap { get; }
        public int PostTime { get; }
        public DialogueDrawArgs(DialogueInfo source, int[] timeMap, int postTime)
        {
            Source = source;
            TimeMap = timeMap;
            PostTime = postTime;
        }
        public static DialogueDrawArgs FromInfo(DialogueInfo info)
        {
            return new DialogueDrawArgs(info, new int[info.Text.Length].Select((item, i) =>
            {
                char c = info.Text[i];
                return c switch
                {
                    '.' => Textframe.CHAR_READ_MILISECONDS * 3,
                    '?' => Textframe.CHAR_READ_MILISECONDS * 3,
                    _ => Textframe.CHAR_READ_MILISECONDS,
                };
            }).ToArray(), info.PostMiliseconds);
        }
    }
    public class Textframe : GameComponent
    {
        private Queue<DialogueInfo> _dialogueQueue;
        private Rectangle _dialoguebounds;
        private DialogueInfo? _currentDialogue;
        private DialogueDrawArgs? _currentDialogueDrawArgs;
        private Point _dialogueTextPos;
        private string _toDrawDialogueText;
        private GameFont _font;

        private Point _providerTextPos;
        private float _providerTextScaleCoefficient;
        private Tweener<float>? _providerTextSizeTweener;

        private float _dialogueShowCoefficient;
        private bool _awaitingMouseDialogueHover;
        private bool _dialogueIsHidden;
        public bool DialogueIsHidden
        {
            get => _dialogueIsHidden;
            set => _dialogueIsHidden = value;
        }

        public Rectangle DialogueBounds => _dialoguebounds;
        public const int CHAR_READ_MILISECONDS = 75; // per char
        public const int POST_READ_MILISECONDS = CHAR_READ_MILISECONDS * 10; // how long the dialogue stagnates after its finished.

        private int _msSinceLastChar;
        private int _charsRead;
        private int _postTimeRead;

        private UserInterface _userInterface;

        public Textframe(UserInterface userInterface)
        {
            _dialogueQueue = new Queue<DialogueInfo>();
            _dialogueTextPos = Point.Zero;
            _currentDialogue = null;
            _currentDialogueDrawArgs = null;
            _dialogueShowCoefficient = 0f;
            _msSinceLastChar = 0;
            _charsRead = 0;
            _toDrawDialogueText = string.Empty;
            _userInterface = userInterface;
            _font = STOLON.Fonts[STOLON.MEDIUM_FONT_ID];
        }
        public void Queue(DialogueInfo[] dialogue)
        {
            for (int i = 0; i < dialogue.Length; i++)
                Queue(dialogue[i]);
        }
        public void Queue(DialogueInfo dialogue)
        {
            _dialogueQueue.Enqueue(dialogue);
            STOLON.Debug.Log("dialogue queued with text: " + dialogue.Text);
        }
        public void Next()
        {
            if (_dialogueQueue.Count == 0) throw new Exception();

            STOLON.Debug.Log(">attempting dequeuing of dialogue with text: " + _dialogueQueue.Peek().Text);
            bool providerDiffers = _currentDialogue.HasValue && _currentDialogue.Value.Provider.Name != _dialogueQueue.Peek().Provider.Name;
            if (providerDiffers) STOLON.Debug.Log("dialogue has new provider of name: " + _dialogueQueue.Peek().Provider.Name);

            _currentDialogue = _dialogueQueue.Dequeue();
            _currentDialogueDrawArgs = DialogueDrawArgs.FromInfo(_currentDialogue.Value);

            _toDrawDialogueText = string.Empty;
            _msSinceLastChar = 0;
            _charsRead = 0;
            _postTimeRead = 0;

            //initialDialogueMiliseconds = GetMilisecondsFromText(currentDialogue.Value.Text) + currentDialogue.Value.ExtraMS;
            //dialogueMilisecondsRemaining = initialDialogueMiliseconds;

            _awaitingMouseDialogueHover = true;
            if (providerDiffers || _providerTextSizeTweener == null) // initialize or refresh.
            {
                _providerTextSizeTweener = new Tweener<float>(0.0001f, 1f, 1, Ease.Sine.Out); // 0 does not work with size calculations..
                _providerTextSizeTweener.Start();
            }

            STOLON.Debug.Success();
        }

        public void Queue(int count, Func<string, int, string>? selector = null)
        {
            STOLON.Debug.Log(">mass queueing a stream of size: " + count);
            selector ??= new Func<string, int, string>((s, i) => s);
            //for (int i = 0; i < count; i++) Queue(new DialogueInfo(StolonEnvironment.Instance, selector.Invoke((()StolonGame.Instance.Environment.GameStateManager.Current).GetRandomSplashText(), i)));
            throw new NotImplementedException();
            STOLON.Debug.Success();
        }

        public override void Update(int elapsedMiliseconds)
        {
            Point dialogueBoxDimensions = new Point(384, 96);
            int dialogueYoffset = (int)(-10f * (_dialogueIsHidden ? _dialogueShowCoefficient : 1f));
            bool textFrameGoUp = false;
            _dialoguebounds = new Rectangle(
                (int)(STOLON.Instance.VirtualBounds.Width * 0.5f - dialogueBoxDimensions.X * 0.5f),
                (int)(STOLON.Instance.VirtualBounds.Height - (dialogueBoxDimensions.Y * _dialogueShowCoefficient) + dialogueYoffset),
                dialogueBoxDimensions.X,
                dialogueBoxDimensions.Y);

            _msSinceLastChar += elapsedMiliseconds;

            if (_dialogueQueue.Count > 0 && !_currentDialogue.HasValue) Next();

            if (_currentDialogue.HasValue) // dialogue is here!
            {
                if (_currentDialogue.Value.Text.Length == 0) throw new Exception("Text size zero.");

                if (_toDrawDialogueText == _currentDialogue.Value.Text) // if no text is left to add..
                {
                    _postTimeRead += elapsedMiliseconds; // only add postread if text is full.
                    if (_dialogueQueue.Count > 0 && _postTimeRead > _currentDialogueDrawArgs!.Value.PostTime) Next(); // ..and queue is full, go next.
                }
                else if (_msSinceLastChar > _currentDialogueDrawArgs!.Value.TimeMap[_charsRead]) // else if its time for a new char..
                {
                    _toDrawDialogueText += _currentDialogue.Value.Text[_charsRead]; // ..add said char.
                    _charsRead++; 
                    _msSinceLastChar = 0;
                }

                _providerTextSizeTweener!.Update(elapsedMiliseconds / 1000f);
                _providerTextScaleCoefficient = MathF.Min(_providerTextSizeTweener.Value, _dialogueShowCoefficient > 0.9f ? 1f : _dialogueShowCoefficient);

                _dialogueTextPos = _dialoguebounds.Location
                    + new Point((int)(_dialoguebounds.Width / 2f - _font.FastMeasure(_toDrawDialogueText).X / 2f),
                    (int)(_dialoguebounds.Height / 2f - _font.Dimensions.Y));
                 
                _providerTextPos = _dialoguebounds.Location
                    + new Point((int)(_dialoguebounds.Width / 2f - _font.FastMeasure(_currentDialogue.Value.Provider.Name).X * _providerTextScaleCoefficient / 2f), 2);
            }
            if (_awaitingMouseDialogueHover) textFrameGoUp = true;
            if (SLMouse.Domain == SLMouse.MouseDomain.Dialogue)
            {
                _awaitingMouseDialogueHover = false;
                textFrameGoUp = true;
            }
            else if (!_awaitingMouseDialogueHover)
            {
                textFrameGoUp = false;
            }

            DynamicTweening.PushSubunitary(ref _dialogueShowCoefficient, textFrameGoUp, elapsedMiliseconds, smoothness: 2);
            _dialogueShowCoefficient = Math.Clamp(_dialogueShowCoefficient, 0.1f, 1f);
        }
        public int GetMilisecondsFromText(string text)
        {
            return text.Length * CHAR_READ_MILISECONDS;
        }
        public override void Draw(SpriteBatch spriteBatch, int elapsedMiliseconds)
        {
            spriteBatch.Draw(STOLON.Textures.Pixel, _dialoguebounds, Color.Black);
            if (_currentDialogue.HasValue)
            {
                spriteBatch.DrawString(_font, _toDrawDialogueText, _dialogueTextPos.ToVector2(), Color.White, 0f, Vector2.Zero, _font.Scale, SpriteEffects.None, 0f);
                spriteBatch.DrawString(_font, _currentDialogue.Value.Provider.Name.ToUpper(), _providerTextPos.ToVector2(), Color.White, 0f, Vector2.Zero, _providerTextScaleCoefficient * _font.Scale, SpriteEffects.None, 0f);
            }
            spriteBatch.DrawRectangle(_dialoguebounds, Color.White, _userInterface.LineWidth);

            base.Draw(spriteBatch, elapsedMiliseconds);
        }
    }
}