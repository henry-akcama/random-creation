using System;
using System.Collections.Generic;

namespace RandomCreation
{
    /// <summary>
    /// Static in-memory undo service. Max depth 10 actions.
    /// Each action stores a description and a restore lambda.
    /// Stack is never persisted — cleared on app close.
    /// </summary>
    public static class UndoService
    {
        private const int MaxDepth = 10;

        private static readonly Stack<UndoAction> _stack = new();

        /// <summary>True when there is at least one action that can be undone.</summary>
        public static bool CanUndo => _stack.Count > 0;

        /// <summary>
        /// Pushes a new undoable action onto the stack.
        /// If the stack is already at MaxDepth, the bottom action is discarded.
        /// </summary>
        public static void Push(string description, Action restore)
        {
            if (_stack.Count >= MaxDepth)
            {
                // Stack is LIFO — to remove the oldest we need to rebuild it
                var items = _stack.ToArray();  // newest first
                _stack.Clear();
                // Re-push all except the last (oldest)
                for (int i = items.Length - 2; i >= 0; i--)
                    _stack.Push(items[i]);
            }
            _stack.Push(new UndoAction(description, restore));
        }

        /// <summary>
        /// Pops and invokes the most recent undo action.
        /// Returns the description of the action that was undone, or null if stack was empty.
        /// </summary>
        public static string? Undo()
        {
            if (!CanUndo) return null;
            var action = _stack.Pop();
            action.Restore();
            return action.Description;
        }

        /// <summary>Clears all undo history. Called on app exit.</summary>
        public static void Clear() => _stack.Clear();

        /// <summary>Description of the next action that would be undone, or null.</summary>
        public static string? PeekDescription =>
            CanUndo ? _stack.Peek().Description : null;
    }

    /// <summary>A single undoable action.</summary>
    public class UndoAction
    {
        public string  Description { get; }
        public Action  Restore     { get; }

        public UndoAction(string description, Action restore)
        {
            Description = description;
            Restore     = restore;
        }
    }
}
