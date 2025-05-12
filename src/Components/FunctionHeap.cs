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
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Stolon.StolonGame;

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

        private Dictionary<string, DynamicTask> taskDictionary;
        private Dictionary<string, object?> frameCompletedTasks;
        private Dictionary<string, int> taskWaitDataCollection;
        private List<string> allCompletedTasks;

        public TaskHeap()
        {
            taskDictionary = new Dictionary<string, DynamicTask>();
            frameCompletedTasks = new Dictionary<string, object?>();
            taskWaitDataCollection = new Dictionary<string, int>();
            allCompletedTasks = new List<string>();
            Functions = taskDictionary.AsReadOnly();
            FrameCompletedTasks = frameCompletedTasks.AsReadOnly();
        }

        public void Update(int elapsedMiliseconds)
        {
            frameCompletedTasks.Clear();

            foreach (KeyValuePair<string, DynamicTask> taskKvp in taskDictionary)
            {
                if (taskWaitDataCollection[taskKvp.Key] < 0)
                {
                    Instance.DebugStream.Log("(interupt:taskheap) runningtask with id; " + taskKvp.Key);
                    ForceRun(taskKvp.Key);
                }
                else taskWaitDataCollection[taskKvp.Key] -= elapsedMiliseconds;
            }
        }

        public object? ForceRun(string id)
        {
            object? ret = taskDictionary[id].Run();

            frameCompletedTasks.Add(id, ret);
            allCompletedTasks.Add(id);

            taskWaitDataCollection.Remove(id);
            taskDictionary.Remove(id);

            return ret;
        }

        public string[] GetHistory() => allCompletedTasks.ToArray();
        public bool IsCompleted(string id) => frameCompletedTasks.ContainsKey(id);
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

        public bool IsQueued(string id) => taskDictionary.ContainsKey(id);
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
                Instance.DebugStream.Log("insta-ran task with id: " + id);
                frameCompletedTasks.Add(id, ret);
                allCompletedTasks.Add(id);
                return;
            }

            if (taskDictionary.ContainsKey(id)) 
                if (overwrite) Instance.DebugStream.Log("key already known, overwriting task with id: " + id);
                else
                {
                    //Instance.DebugStream.Log("\t\tkey already known, overwrite is dissabled, skipping push.");
                    return;
                }
            taskWaitDataCollection[id] = waitTime;
            taskDictionary[id] = dynamicTask;
            Instance.DebugStream.Log("pushed task with id: " + id);
        }
        public void Push(string id, DynamicTask dynamicTask, int waitTime)
        {
            if (IsQueued(id)) throw new Exception();
            else SafePush(id, dynamicTask, waitTime);
        }
        public string EnsurePush(DynamicTask dynamicTask, int waitTime)
        {
            Instance.DebugStream.Log(">ensuring task push.");
            string id = Enumerable.Range(0, int.MaxValue).Select(i => "__" + i).First(key => !taskDictionary.ContainsKey(key));

            Push(id, dynamicTask, waitTime);

            Instance.DebugStream.Log("task pushed with id: " + id);
            Instance.DebugStream.Success();

            return id;
        }

        public static TaskHeap Heap => Instance.Environment.TaskHeap;
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
