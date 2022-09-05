﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Model.Intrinsic;

namespace Snap.Hutao.Web.Hoyolab.Hk4e.Event.GachaInfo;

/// <summary>
/// 祈愿记录物品
/// </summary>
public class GachaLogItem
{
    /// <summary>
    /// 玩家Uid
    /// </summary>
    [JsonPropertyName("uid")]
    public string Uid { get; set; } = default!;

    /// <summary>
    /// 祈愿类型
    /// </summary>
    [JsonPropertyName("gacha_type")]
    public GachaConfigType GachaType { get; set; } = default!;

    /// <summary>
    /// 总为 <see cref="string.Empty"/>
    /// </summary>
    [Obsolete("API removed this property")]
    [JsonPropertyName("item_id")]
    public string ItemId { get; set; } = default!;

    /// <summary>
    /// 个数
    /// 一般为 1
    /// </summary>
    [JsonPropertyName("count")]
    public string? Count { get; set; } = default!;

    /// <summary>
    /// 时间
    /// </summary>
    [JsonPropertyName("time")]
    [JsonConverter(typeof(DateTimeOffsetConverter))]
    public DateTimeOffset Time { get; set; }

    /// <summary>
    /// 物品名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    /// <summary>
    /// 语言
    /// </summary>
    [JsonPropertyName("lang")]
    public string Language { get; set; } = default!;

    /// <summary>
    /// 物品类型
    /// </summary>
    [JsonPropertyName("item_type")]
    public string ItemType { get; set; } = default!;

    /// <summary>
    /// 物品稀有等级
    /// </summary>
    [JsonPropertyName("rank_type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ItemQuality Rank { get; set; } = default!;

    /// <summary>
    /// Id
    /// </summary>
    [JsonPropertyName("id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long Id { get; set; } = default!;
}