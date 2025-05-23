using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using AsitLib;
using AsitLib.Debug;

using MonoGame.Extended;

using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using DiscordRPC;
using DiscordRPC.Events;
using System.Collections.Frozen;
using System.Threading.Tasks;
using System.Collections;
using System.Linq;

#nullable enable

namespace Stolon
{
    /// <summary>
    /// Provides a way to "fire and forget" simple game logic.
    /// </summary>
    public class TaskHeap
    {
        public ReadOnlyDictionary<string, DynamicTask> Functions { get; }
        public ReadOnlyDictionary<string, object?> FrameCompletedTasks { get; } 

        private Dictionary<string, DynamicTask> _taskDictionary;
        private Dictionary<string, object?> _frameCompletedTasks;
        private Dictionary<string, int> _taskWaitDataCollection;
        private List<string> _allCompletedTasks;

        public TaskHeap()
        {
            _taskDictionary = new Dictionary<string, DynamicTask>();
            _frameCompletedTasks = new Dictionary<string, object?>();
            _taskWaitDataCollection = new Dictionary<string, int>();
            _allCompletedTasks = new List<string>();
            Functions = _taskDictionary.AsReadOnly();
            FrameCompletedTasks = _frameCompletedTasks.AsReadOnly();
        }

        public void Update(int elapsedMiliseconds)
        {
            _frameCompletedTasks.Clear();

            foreach (KeyValuePair<string, DynamicTask> taskKvp in _taskDictionary)
            {
                if (_taskWaitDataCollection[taskKvp.Key] < 0)
                {
                    STOLON.Debug.Log("(interupt:taskheap) runningtask with id; " + taskKvp.Key);
                    ForceRun(taskKvp.Key);
                }
                else _taskWaitDataCollection[taskKvp.Key] -= elapsedMiliseconds;
            }
        }

        public object? ForceRun(string id)
        {
            object? ret = _taskDictionary[id].Run();

            _frameCompletedTasks.Add(id, ret);
            _allCompletedTasks.Add(id);

            _taskWaitDataCollection.Remove(id);
            _taskDictionary.Remove(id);

            return ret;
        }

        public string[] GetHistory() => _allCompletedTasks.ToArray();
        public bool IsCompleted(string id) => _frameCompletedTasks.ContainsKey(id);
        public bool IsCompleted<T>(string id, out T? returned) 
        {
            if (IsCompleted(id))
            {
                returned = (T?)(FrameCompletedTasks[id]);
                return true;
            }
            else
            {
                returned = default(T?); // null does not work for reasons unknown.
                return false;
            }
        }

        public bool IsQueued(string id) => _taskDictionary.ContainsKey(id);
        /// <summary>
        /// Push a task to the <see cref="TaskHeap"/>.
        /// </summary>
        /// <param name="id">The ID of the <see cref="Task"/> on the <see cref="TaskHeap"/>.</param>
        /// <param name="dynamicTask">The <see cref="Task"/> to push.</param>
        /// <param name="waitTime">The time to wait before starting the task.</param>
        /// <param name="overwrite">If the task.</param>
        public void SafePush(string id, DynamicTask dynamicTask, int waitTime, bool overwrite = true)
        {

            if (waitTime < 0)
            {
                object? ret = dynamicTask.Run();
                STOLON.Debug.Log("insta-ran task with id: " + id);
                _frameCompletedTasks.Add(id, ret);
                _allCompletedTasks.Add(id);
                return;
            }

            if (_taskDictionary.ContainsKey(id)) 
                if (overwrite) STOLON.Debug.Log("key already known, overwriting task with id: " + id);
                else return;
            _taskWaitDataCollection[id] = waitTime;
            _taskDictionary[id] = dynamicTask;
            STOLON.Debug.Log("pushed task with id: " + id);
        }
        public void Push(string id, DynamicTask dynamicTask, int waitTime)
        {
            if (IsQueued(id)) throw new Exception();
            else SafePush(id, dynamicTask, waitTime);
        }
        public string EnsurePush(DynamicTask dynamicTask, int waitTime)
        {
            STOLON.Debug.Log(">ensuring task push.");
            string id = Enumerable.Range(0, int.MaxValue).Select(i => "__" + i).First(key => !_taskDictionary.ContainsKey(key));

            Push(id, dynamicTask, waitTime);

            STOLON.Debug.Log("task pushed with id: " + id);
            STOLON.Debug.Success();

            return id;
        }

        public static TaskHeap Instance => STOLON.Environment.TaskHeap;
    }
    /// <summary>
    /// Represents a <see cref="Func{TResult}"/> or <see cref="Action"/> with no parameters.
    /// </summary>
    public class DynamicTask
    {
        private Func<object?> func;
        /// <summary>
        /// Create a new <see cref="DynamicTask"/> from an <see cref="Action"/> <see langword="delegate"/>.
        /// </summary>
        /// <param name="action">The <see cref="Action"/> to create this <see cref="DynamicTask"/> from.</param>
        public DynamicTask(Action action) : this(() =>
        {
            action.Invoke();
            return null;
        }) { }
        /// <summary>
        /// Create a new <see cref="DynamicTask"/> from an <see cref="Func{TResult}"/> <see langword="delegate"/>.
        /// </summary>
        /// <param name="function">The <see cref="Func{TResult}"/> to create this <see cref="DynamicTask"/> from.</param>
        public DynamicTask(Func<object?> function) => func = function;
        /// <summary>
        /// Run this <see cref="DynamicTask"/> and optionally get its return value.
        /// </summary>
        /// <returns>The return value of the <see cref="DynamicTask"/>, null if the task ran was an <see cref="Action"/>.</returns>
        public object? Run() => func.Invoke();
        /// <summary>
        /// Get this <see cref="DynamicTask"/> as a <see cref="Function{T}"/>.
        /// </summary>
        /// <returns>This <see cref="DynamicTask"/> as a <see cref="Function{T}"/>.</returns>
        public Func<object?> AsFunction() => func;
        public static explicit operator Func<object?>(DynamicTask t) => t.AsFunction();
    }
}
