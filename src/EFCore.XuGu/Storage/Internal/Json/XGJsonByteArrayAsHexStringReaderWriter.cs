// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.XuGu.Storage.Internal.Json;

public sealed class XGJsonByteArrayAsHexStringReaderWriter : JsonValueReaderWriter<byte[]>
{
    public static XGJsonByteArrayAsHexStringReaderWriter Instance { get; } = new();

    private XGJsonByteArrayAsHexStringReaderWriter()
    {
    }

    public override byte[] FromJsonTyped(ref Utf8JsonReaderManager manager, object existingObject = null)
        => Convert.FromHexString(manager.CurrentReader.GetString()!);

    public override void ToJsonTyped(Utf8JsonWriter writer, byte[] value)
        => writer.WriteStringValue(Convert.ToHexString(value));
}
