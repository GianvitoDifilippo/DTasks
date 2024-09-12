﻿using System.Diagnostics.CodeAnalysis;

namespace DTasks.Extensions.Microsoft.DependencyInjection.Hosting;

internal sealed class ChildDTaskScope(IServiceProvider services, ServiceResolver resolver, IRootDTaskScope root) : DTaskScope(services, resolver)
{
    public override bool TryGetReferenceToken(object reference, [NotNullWhen(true)] out object? token)
    {
        return
            base.TryGetReferenceToken(reference, out token) ||
            root.TryGetReferenceToken(reference, out token);
    }
}
