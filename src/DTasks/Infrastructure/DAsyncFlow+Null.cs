﻿using DTasks.Execution;
using DTasks.Marshaling;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace DTasks.Infrastructure;

internal sealed partial class DAsyncFlow
{
    // Since _host, _marshaler, _stateManager, and _typeResolver are initialized in the entry points and defaulted only when calling Reset, the following saves the trouble of asserting they're not null whenever they are used
    private static readonly IDAsyncHost s_nullHost = new NullDAsyncHost();
    private static readonly IDAsyncMarshaler s_nullMarshaler = new NullDAsyncMarshaler();
    private static readonly IDAsyncStateManager s_nullStateManager = new NullDAsyncStateManager();
    private static readonly ITypeResolver s_nullTypeResolver = new NullTypeResolver();
    private static readonly IDistributedCancellationProvider s_nullCancellationProvider = new NullDistributedCancellationProvider();

    [Conditional("DEBUG")]
    [ExcludeFromCodeCoverage]
    private static void FailForNull(string fieldName)
    {
        Debug.Fail($"'{fieldName}' was not initialized.");
    }

    [ExcludeFromCodeCoverage]
    private sealed class NullDAsyncHost : IDAsyncHost
    {
        ITypeResolver IDAsyncHost.TypeResolver
        {
            get
            {
                FailForNull(nameof(_host));
                throw new UnreachableException();
            }
        }

        IDistributedCancellationProvider IDAsyncHost.CancellationProvider
        {
            get
            {
                FailForNull(nameof(_host));
                throw new UnreachableException();
            }
        }

        IDAsyncMarshaler IDAsyncHost.CreateMarshaler()
        {
            FailForNull(nameof(_host));
            throw new UnreachableException();
        }

        IDAsyncStateManager IDAsyncHost.CreateStateManager(IDAsyncMarshaler marshaler)
        {
            FailForNull(nameof(_host));
            throw new UnreachableException();
        }

        Task IDAsyncHost.OnSucceedAsync(CancellationToken cancellationToken)
        {
            FailForNull(nameof(_host));
            throw new UnreachableException();
        }

        Task IDAsyncHost.OnSucceedAsync<TResult>(TResult result, CancellationToken cancellationToken)
        {
            FailForNull(nameof(_host));
            throw new UnreachableException();
        }

        Task IDAsyncHost.OnFailAsync(Exception exception, CancellationToken cancellationToken)
        {
            FailForNull(nameof(_host));
            throw new UnreachableException();
        }

        Task IDAsyncHost.OnCancelAsync(OperationCanceledException exception, CancellationToken cancellationToken)
        {
            FailForNull(nameof(_host));
            throw new UnreachableException();
        }

        Task IDAsyncHost.OnYieldAsync(DAsyncId id, CancellationToken cancellationToken)
        {
            FailForNull(nameof(_host));
            throw new UnreachableException();
        }

        Task IDAsyncHost.OnDelayAsync(DAsyncId id, TimeSpan delay, CancellationToken cancellationToken)
        {
            FailForNull(nameof(_host));
            throw new UnreachableException();
        }

        //Task IDAsyncHost.CancelAsync(OperationCanceledException exception, CancellationToken cancellationToken)
        //{
        //    throw new UnreachableException();
        //}

        //Task IDAsyncHost.CallbackAsync(DAsyncId id, ISuspensionCallback callback, CancellationToken cancellationToken)
        //{
        //    FailForNull(nameof(_host));
        //    throw new UnreachableException();
        //}
    }

    [ExcludeFromCodeCoverage]
    private sealed class NullDAsyncMarshaler : IDAsyncMarshaler
    {
        bool IDAsyncMarshaler.TryMarshal<T, TAction>(in T value, scoped ref TAction action)
        {
            FailForNull(nameof(_marshaler));
            throw new UnreachableException();
        }

        bool IDAsyncMarshaler.TryUnmarshal<T, TAction>(TypeId typeId, scoped ref TAction action)
        {
            FailForNull(nameof(_marshaler));
            throw new UnreachableException();
        }
    }

    [ExcludeFromCodeCoverage]
    private sealed class NullDAsyncStateManager : IDAsyncStateManager
    {
        ValueTask IDAsyncStateManager.DehydrateAsync<TStateMachine>(DAsyncId parentId, DAsyncId id, ref TStateMachine stateMachine, ISuspensionContext suspensionContext, CancellationToken cancellationToken)
        {
            FailForNull(nameof(_stateManager));
            throw new UnreachableException();
        }

        ValueTask<DAsyncLink> IDAsyncStateManager.HydrateAsync(DAsyncId id, CancellationToken cancellationToken)
        {
            FailForNull(nameof(_stateManager));
            throw new UnreachableException();
        }

        ValueTask<DAsyncLink> IDAsyncStateManager.HydrateAsync<TResult>(DAsyncId id, TResult result, CancellationToken cancellationToken)
        {
            FailForNull(nameof(_stateManager));
            throw new UnreachableException();
        }

        ValueTask<DAsyncLink> IDAsyncStateManager.HydrateAsync(DAsyncId id, Exception exception, CancellationToken cancellationToken)
        {
            FailForNull(nameof(_stateManager));
            throw new UnreachableException();
        }

        ValueTask<DAsyncId> IDAsyncStateManager.DeleteAsync(DAsyncId id, CancellationToken cancellationToken)
        {
            FailForNull(nameof(_stateManager));
            throw new UnreachableException();
        }

        ValueTask IDAsyncStateManager.FlushAsync(CancellationToken cancellationToken)
        {
            FailForNull(nameof(_stateManager));
            throw new UnreachableException();
        }
    }

    private sealed class NullTypeResolver : ITypeResolver
    {
        Type ITypeResolver.GetType(TypeId id)
        {
            FailForNull(nameof(_typeResolver));
            throw new UnreachableException();
        }

        TypeId ITypeResolver.GetTypeId(Type type)
        {
            FailForNull(nameof(_typeResolver));
            throw new UnreachableException();
        }
    }

    private sealed class NullDistributedCancellationProvider : IDistributedCancellationProvider
    {
        void IDistributedCancellationProvider.RegisterHandler(IDistributedCancellationHandler handler)
        {
            FailForNull(nameof(_cancellationProvider));
            throw new UnreachableException();
        }

        void IDistributedCancellationProvider.UnregisterHandler(IDistributedCancellationHandler handler)
        {
            FailForNull(nameof(_cancellationProvider));
            throw new UnreachableException();
        }

        Task IDistributedCancellationProvider.CancelAsync(DCancellationId id, CancellationToken cancellationToken)
        {
            FailForNull(nameof(_cancellationProvider));
            throw new UnreachableException();
        }

        Task IDistributedCancellationProvider.CancelAsync(DCancellationId id, DateTimeOffset expirationTime, CancellationToken cancellationToken)
        {
            FailForNull(nameof(_cancellationProvider));
            throw new UnreachableException();
        }
    }
}
