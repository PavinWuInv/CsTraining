using System.Diagnostics;
using Assert = Xunit.Assert;
using Xunit;
using System.Collections.Generic;
using System.Threading;
using System;
using System.Linq;

using ConcurrentTaskExecution;
using static ConcurrentTaskExecution.Worker;

public class MyTaskFactory
{
    private List<int> successfulTaskDurations = new List<int> { 600, 100, 400 };

    public Runnable CreateSuccessfulTask()
    {
        var duration = successfulTaskDurations[0];
        successfulTaskDurations.RemoveAt(0);
        return new Runnable(() => Thread.Sleep(TimeSpan.FromMilliseconds(duration)));
    }

    public Runnable CreateFailingTask(int delay)
    {
        return new Runnable(() =>
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(delay));
            throw new Exception();
        });
    }

    public Runnable CreateTimeoutTask()
    {
        return new Runnable(() => Thread.Sleep(TimeSpan.FromSeconds(100)));
    }
}

public class ConcurrentTaskExecutionTest
{
    private static readonly TimeSpan DEFAULT_TIMEOUT = TimeSpan.FromSeconds(12);

    private MyTaskFactory factory;
    private Worker worker;

    public ConcurrentTaskExecutionTest()
    {
        factory = new MyTaskFactory();
        worker = new Worker();
    }

    [Fact]
    public async void TestSuccessful()
    {
        List<Runnable> tasks = new List<Runnable>();
        List<Runnable> expectedSuccess = new List<Runnable>();

        tasks.Add(factory.CreateSuccessfulTask());
        tasks.Add(factory.CreateSuccessfulTask());
        tasks.Add(factory.CreateSuccessfulTask());

        expectedSuccess.Add(tasks[1]);
        expectedSuccess.Add(tasks[2]);
        expectedSuccess.Add(tasks[0]);

        Worker.ExecutedTasks result = await worker.ExecuteAsync(tasks, DEFAULT_TIMEOUT);

        Assert.True(expectedSuccess.SequenceEqual(result.successful));
        Assert.False(result.failed.Any());
        Assert.False(result.timedOut.Any());
    }

    [Fact]
    public async void TestFailed()
    {
        List<Runnable> tasks = new List<Runnable>();
        ISet<Runnable> expectedFailed = new HashSet<Runnable>();

        expectedFailed.Add(factory.CreateFailingTask(200));
        expectedFailed.Add(factory.CreateFailingTask(100));

        tasks.AddRange(expectedFailed);

        Worker.ExecutedTasks result = await worker.ExecuteAsync(tasks, DEFAULT_TIMEOUT);

        Assert.False(result.successful.Any());
        Assert.True(expectedFailed.SetEquals(result.failed));
        Assert.False(result.timedOut.Any());
    }

    [Fact]
    public async void TestTimedOut()
    {

        List<Runnable> tasks = new List<Runnable>();
        ISet<Runnable> expectedTimedOut = new HashSet<Runnable>();

        for (int i = 0; i < 3; i++)
            expectedTimedOut.Add(factory.CreateTimeoutTask());

        tasks.AddRange(expectedTimedOut);

        Worker.ExecutedTasks result = await worker.ExecuteAsync(tasks, DEFAULT_TIMEOUT);

        Assert.False(result.successful.Any());
        Assert.False(result.failed.Any());
        Assert.True(expectedTimedOut.SetEquals(result.timedOut));
    }

    [Fact]
    public async void TestNoExcessiveWait()
    {
        List<Runnable> tasks = Enumerable.Range(0, 3).Select(i => factory.CreateSuccessfulTask()).ToList();

        int timeLimit = 700;
        Stopwatch sw = new Stopwatch();
        sw.Start();
        Worker.ExecutedTasks result = await worker.ExecuteAsync(tasks, DEFAULT_TIMEOUT);
        sw.Stop();

        Assert.Equal(tasks.Count, result.successful.Count);
        Assert.Empty(result.failed);
        Assert.Empty(result.timedOut);
        string message = string.Format("Execution time significantly exceeded time necessary for "
                                     + "execution of all tasks. "
                                     + "Execution took {0}ms, while {1}ms is more than enough.",
                                     sw.ElapsedMilliseconds, timeLimit);

        Assert.True(sw.ElapsedMilliseconds <= timeLimit, message);
    }

    [Fact]
    public async void TestBasic()
    {
        List<Runnable> tasks = new List<Runnable>();

        List<Runnable> expectedSuccess = new List<Runnable>();
        tasks.Add(factory.CreateSuccessfulTask());
        tasks.Add(factory.CreateSuccessfulTask());

        expectedSuccess.Add(tasks[1]);
        expectedSuccess.Add(tasks[0]);

        ISet<Runnable> expectedFailed = new HashSet<Runnable>();
        expectedFailed.Add(factory.CreateFailingTask(200));
        expectedFailed.Add(factory.CreateFailingTask(100));
        tasks.AddRange(expectedFailed);

        ISet<Runnable> expectedTimedOut = new HashSet<Runnable>();
        expectedTimedOut.Add(factory.CreateTimeoutTask());
        expectedTimedOut.Add(factory.CreateTimeoutTask());
        tasks.AddRange(expectedTimedOut);

        Worker.ExecutedTasks result = await worker.ExecuteAsync(tasks, DEFAULT_TIMEOUT);

        Assert.Equal(expectedSuccess, result.successful);
        Assert.Equivalent(expectedFailed, result.failed);   // Equivalent needs Xunit 2.4.2!!
        Assert.Equivalent(expectedTimedOut, result.timedOut);
    }
}
