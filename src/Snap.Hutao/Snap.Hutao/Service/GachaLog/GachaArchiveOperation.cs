﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Model.Entity;
using System.Collections.ObjectModel;

namespace Snap.Hutao.Service.GachaLog;

/// <summary>
/// 祈愿存档初始化上下文
/// </summary>
internal static class GachaArchiveOperation
{
    public static void GetOrAdd(IGachaLogDbService gachaLogDbService, ITaskContext taskContext, string uid, ObservableCollection<GachaArchive> archives, [NotNull] out GachaArchive? archive)
    {
        archive = archives.SingleOrDefault(a => a.Uid == uid);

        if (archive is null)
        {
            GachaArchive created = GachaArchive.From(uid);
            gachaLogDbService.AddGachaArchive(created);
            taskContext.InvokeOnMainThread(() => archives.Add(created));
            archive = created;
        }
    }
}