﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.UI.Dispatching;

namespace Snap.Hutao.Core.Threading;

/// <summary>
/// 任务上下文
/// </summary>
[Injection(InjectAs.Singleton, typeof(ITaskContext))]
internal sealed class TaskContext : ITaskContext, ITaskContextUnsafe
{
    private readonly DispatcherQueueSynchronizationContext synchronizationContext;
    private readonly DispatcherQueue dispatcherQueue;

    /// <summary>
    /// 构造一个新的任务上下文
    /// </summary>
    public TaskContext()
    {
        dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        synchronizationContext = new(dispatcherQueue);
        SynchronizationContext.SetSynchronizationContext(synchronizationContext);
    }

    public DispatcherQueue DispatcherQueue { get => dispatcherQueue; }

    /// <inheritdoc/>
    public ThreadPoolSwitchOperation SwitchToBackgroundAsync()
    {
        return new(dispatcherQueue);
    }

    /// <inheritdoc/>
    public DispatcherQueueSwitchOperation SwitchToMainThreadAsync()
    {
        return new(dispatcherQueue);
    }

    /// <inheritdoc/>
    public void InvokeOnMainThread(Action action)
    {
        dispatcherQueue.Invoke(action);
    }

    public void BeginInvokeOnMainThread(Action action)
    {
        dispatcherQueue.TryEnqueue(() => action());
    }
}