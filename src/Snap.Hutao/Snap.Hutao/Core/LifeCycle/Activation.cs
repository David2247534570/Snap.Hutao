﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using CommunityToolkit.WinUI.Notifications;
using Microsoft.Windows.AppLifecycle;
using Snap.Hutao.Extension;
using Snap.Hutao.Service.Abstraction;
using Snap.Hutao.Service.DailyNote;
using Snap.Hutao.Service.Metadata;
using Snap.Hutao.Service.Navigation;
using System.Security.Principal;

namespace Snap.Hutao.Core.LifeCycle;

/// <summary>
/// 激活处理器
/// </summary>
internal static class Activation
{
    /// <summary>
    /// 启动游戏启动参数
    /// </summary>
    public const string LaunchGame = "LaunchGame";

    private static readonly SemaphoreSlim ActivateSemaphore = new(1);

    /// <summary>
    /// 获取是否提升了权限
    /// </summary>
    /// <returns>是否提升了权限</returns>
    public static bool GetElevated()
    {
        using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
        {
            WindowsPrincipal principal = new(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }

    /// <summary>
    /// 响应激活事件
    /// 激活事件一般不会在UI线程上触发
    /// </summary>
    /// <param name="sender">发送方</param>
    /// <param name="args">激活参数</param>
    public static void Activate(object? sender, AppActivationArguments args)
    {
        _ = sender;
        if (!ToastNotificationManagerCompat.WasCurrentProcessToastActivated())
        {
            HandleActivationAsync(args).SafeForget();
        }
    }

    /// <summary>
    /// 响应通知激活事件
    /// </summary>
    /// <param name="args">参数</param>
    public static void NotificationActivate(ToastNotificationActivatedEventArgsCompat args)
    {
        ToastArguments toastArgs = ToastArguments.Parse(args.Argument);
        _ = toastArgs;

        if (toastArgs.TryGetValue("Action", out string? action))
        {
            if (action == LaunchGame)
            {
                _ = toastArgs.TryGetValue("Uid", out string? uid);
                HandleLaunchGameActionAsync(uid).SafeForget();
            }
        }
    }

    /// <summary>
    /// 异步响应激活事件
    /// </summary>
    /// <returns>任务</returns>
    private static async Task HandleActivationAsync(AppActivationArguments args)
    {
        if (ActivateSemaphore.CurrentCount > 0)
        {
            using (await ActivateSemaphore.EnterAsync().ConfigureAwait(false))
            {
                await HandleActivationCoreAsync(args).ConfigureAwait(false);
            }
        }
    }

    private static async Task HandleActivationCoreAsync(AppActivationArguments args)
    {
        if (args.Kind == ExtendedActivationKind.Protocol)
        {
            if (args.TryGetProtocolActivatedUri(out Uri? uri))
            {
                Ioc.Default.GetRequiredService<IInfoBarService>().Information(uri.ToString());
                await HandleUrlActivationAsync(uri).ConfigureAwait(false);
            }
        }
        else if (args.Kind == ExtendedActivationKind.Launch)
        {
            if (args.TryGetLaunchActivatedArgument(out string? arguments))
            {
                switch (arguments)
                {
                    case "":
                        {
                            await WaitMainWindowAsync().ConfigureAwait(false);
                            break;
                        }

                    case LaunchGame:
                        {
                            await HandleLaunchGameActionAsync().ConfigureAwait(false);
                            break;
                        }
                }
            }
        }
    }

    private static async Task WaitMainWindowAsync()
    {
        await ThreadHelper.SwitchToMainThreadAsync();
        _ = Ioc.Default.GetRequiredService<MainWindow>();
        await Ioc.Default.GetRequiredService<IInfoBarService>().WaitInitializationAsync().ConfigureAwait(false);

        Ioc.Default
            .GetRequiredService<IMetadataService>()
            .ImplictAs<IMetadataInitializer>()?
            .InitializeInternalAsync()
            .SafeForget();
    }

    private static async Task HandleUrlActivationAsync(Uri uri)
    {
        UriBuilder builder = new(uri);

        string category = builder.Host.ToLowerInvariant();
        string action = builder.Path.ToLowerInvariant();
        string parameter = builder.Query.ToLowerInvariant();

        switch (category)
        {
            case "achievement":
                {
                    await WaitMainWindowAsync().ConfigureAwait(false);
                    await HandleAchievementActionAsync(action, parameter).ConfigureAwait(false);
                    break;
                }

            case "dailynote":
                {
                    await HandleDailyNoteActionAsync(action, parameter).ConfigureAwait(false);
                    break;
                }
        }
    }

    private static async Task HandleAchievementActionAsync(string action, string parameter)
    {
        _ = parameter;
        switch (action)
        {
            case "/import":
                {
                    await ThreadHelper.SwitchToMainThreadAsync();

                    INavigationAwaiter navigationAwaiter = new NavigationExtra("InvokeByUri");
                    await Ioc.Default
                        .GetRequiredService<INavigationService>()
                        .NavigateAsync<View.Page.AchievementPage>(navigationAwaiter, true)
                        .ConfigureAwait(false);
                    break;
                }
        }
    }

    private static async Task HandleDailyNoteActionAsync(string action, string parameter)
    {
        _ = parameter;
        switch (action)
        {
            case "/refresh":
                {
                    await Ioc.Default
                        .GetRequiredService<IDailyNoteService>()
                        .RefreshDailyNotesAsync(true)
                        .ConfigureAwait(false);
                    break;
                }
        }
    }

    private static async Task HandleLaunchGameActionAsync(string? uid = null)
    {
        await ThreadHelper.SwitchToMainThreadAsync();

        // TODO auto switch to account
        if (!MainWindow.IsPresent)
        {
            _ = Ioc.Default.GetRequiredService<LaunchGameWindow>();
        }
        else
        {
            await Ioc.Default
                .GetRequiredService<INavigationService>()
                .NavigateAsync<View.Page.LaunchGamePage>(INavigationAwaiter.Default, true).ConfigureAwait(false);
        }
    }
}