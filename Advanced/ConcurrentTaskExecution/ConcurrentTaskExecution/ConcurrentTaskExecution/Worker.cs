using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ConcurrentTaskExecution
{
    // https://www.codewars.com/kata/5b34ca5791c746079c000067/train/csharp

    // Task with timeout
    // https://devblogs.microsoft.com/oldnewthing/20220505-00/?p=106585

    public class Worker
    {
        public class ExecutedTasks
        {
            public readonly List<Runnable> successful = new List<Runnable>();
            public readonly ISet<Runnable> failed = new HashSet<Runnable>();
            public readonly ISet<Runnable> timedOut = new HashSet<Runnable>();
        }

        public class Runnable
        {
            private readonly Action action;
            public Runnable(Action action) { this.action = action; }
            public void Run() { action(); }
        }

        public ExecutedTasks Execute(ICollection<Runnable> actions, TimeSpan timeout)
        {
            return ExecuteAsync(actions, timeout).Result;
        }

        public async Task<ExecutedTasks> ExecuteAsync(ICollection<Runnable> actions, TimeSpan timeout)
        {
            ExecutedTasks result = new ExecutedTasks();

            List<Task> taskList = new List<Task>();

            foreach (var action in actions)
            {
                var completedTokenSource = new CancellationTokenSource();
                var timeoutTokenSource = new CancellationTokenSource();

                // Wrap each task in the when any timeout
                // Note: execution continues without cancellation! => non timeout results are added as well as timeout ones.
                var task = Task.Run(() =>
                {
                    try
                    {
                        action.Run();
                        if (!timeoutTokenSource.Token.IsCancellationRequested)
                        {
                            result.successful.Add(action);
                        }
                    }
                    catch (Exception e)
                    {
                        if (!timeoutTokenSource.Token.IsCancellationRequested)
                        {
                            result.failed.Add(action);
                        }
                    }
                    completedTokenSource.Cancel();
                });

                // https://stackoverflow.com/questions/4379048/storing-a-lambda-expression-in-a-variable
                Func<Task> timeoutProvider = async () =>
                {
                    await Task.Delay(timeout);
                    timeoutTokenSource.Cancel();
                    if (!completedTokenSource.Token.IsCancellationRequested)
                    {
                        result.timedOut.Add(action);
                    }
                };

                var taskWithTimeout = Task.WhenAny(task, timeoutProvider());
                taskList.Add(taskWithTimeout);
            }

            // Hint: Task.WaitAll

            // https://stackoverflow.com/questions/17197699/awaiting-multiple-tasks-with-different-results
            // When all those tasks
            await Task.WhenAll(taskList.ToArray());

            return result;
        }
    }
}
