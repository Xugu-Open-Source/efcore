// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Design.Internal;

public class XGCSharpRuntimeAnnotationCodeGenerator : RelationalCSharpRuntimeAnnotationCodeGenerator
{
    public XGCSharpRuntimeAnnotationCodeGenerator(
        CSharpRuntimeAnnotationCodeGeneratorDependencies dependencies,
        RelationalCSharpRuntimeAnnotationCodeGeneratorDependencies relationalDependencies)
        : base(dependencies, relationalDependencies)
    {
    }

    public override bool Create(
        CoreTypeMapping typeMapping,
        CSharpRuntimeAnnotationCodeGeneratorParameters parameters,
        ValueComparer valueComparer = null,
        ValueComparer keyValueComparer = null,
        ValueComparer providerValueComparer = null)
    {
        var result = base.Create(typeMapping, parameters, valueComparer, keyValueComparer, providerValueComparer);

        if (typeMapping is IXGCSharpRuntimeAnnotationTypeMappingCodeGenerator extension)
        {
            extension.Create(parameters, Dependencies);
        }

        return result;
    }
}
