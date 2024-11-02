using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

#nullable enable

namespace Stolon
{
    /// <summary>
    /// Provides a way to make a object responsible for dialogue. 
    /// </summary>
    public interface IDialogueProvider
    {
        /// <summary>
        /// The symbolnotation of this <see cref="EntityBase"/>, example: DL for Deadline.
        /// </summary>
        public string SymbolNotation { get; }
        /// <summary>
        /// The name of this <see cref="EntityBase"/>, example: Deadline.
        /// </summary>
        public string Name { get; }
    }
    /// <summary>
    /// Represent a pushable dialogue prompt.
    /// </summary>
    public readonly struct DialogueInfo
    {
        /// <summary>
        /// The text this <see cref="DialogueInfo"/> holds.
        /// </summary>
        public string Text { get; }
        public int ExtraMS { get; }
        /// <summary>
        /// The initial <see cref="IDialogueProvider"/>.
        /// </summary>
        public IDialogueProvider Provider { get; }
        /// <summary>
        /// Create a new <see cref="DialogueInfo"/> with a set <see cref="IDialogueProvider"/> and <see cref="Text"/>.
        /// </summary>
        /// <param name="provider">The initial <see cref="IDialogueProvider"/>.</param>
        /// <param name="text">The text this <see cref="DialogueInfo"/> holds.</param>
        public DialogueInfo(IDialogueProvider provider, string text, int extraMs = 0)
        {
            Provider = provider;
            Text = text;
            ExtraMS = extraMs;
        }
        public override bool Equals([NotNullWhen(true)] object? obj) => ToString() == (obj == null ? string.Empty : obj).ToString();
        public static bool operator ==(DialogueInfo left, DialogueInfo right) => left.Equals(right);
        public static bool operator !=(DialogueInfo left, DialogueInfo right) => !(left == right);
        public override int GetHashCode() => HashCode.Combine(Text.GetHashCode(), Provider.GetHashCode());
    }
    
}
