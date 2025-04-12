﻿using System.ComponentModel;
using DTasks.Utils;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DTasks;

[EditorBrowsable(EditorBrowsableState.Never)]
[StructLayout(LayoutKind.Sequential)]
public readonly struct DAsyncId : IEquatable<DAsyncId>
{
    private const int ByteCount = 3 * sizeof(uint);
    private const byte ReservedBitsMask = 0b_00001111;
    private const byte ReservedBitsInvertedMask = ~ReservedBitsMask & byte.MaxValue;
    private const byte FlowIdMask = 0b_00000001;

#if DEBUG_TESTS
    private static int s_idCount = 0;
#elif !NET6_0_OR_GREATER
    private static readonly ThreadLocal<Random> s_randomLocal = new(static () => new Random());
#endif

    private readonly uint _a;
    private readonly uint _b;
    private readonly uint _c;

    private DAsyncId(uint a, uint b, uint c)
    {
        _a = a;
        _b = b;
        _c = c;
    }

    private DAsyncId(ReadOnlySpan<byte> bytes)
    {
        Debug.Assert(bytes.Length == ByteCount);

        this = Unsafe.ReadUnaligned<DAsyncId>(ref MemoryMarshal.GetReference(bytes));
    }

    internal bool IsDefault => this == default;

    internal bool IsFlowId => (_c & FlowIdMask) == FlowIdMask;

    public byte[] ToByteArray()
    {
        byte[] bytes = new byte[ByteCount];
        Unsafe.WriteUnaligned(ref bytes[0], this);

        return bytes;
    }

    public bool TryWriteBytes(Span<byte> destination)
    {
        if (ByteCount > destination.Length)
            return false;

        Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(destination), this);
        return true;
    }

    public bool Equals(DAsyncId other) =>
        _a == other._a &&
        _b == other._b &&
        _c == other._c;

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is DAsyncId other && Equals(other);

    public override int GetHashCode()
    {
        ref int head = ref Unsafe.As<DAsyncId, int>(ref Unsafe.AsRef(in this));
        return head ^ Unsafe.Add(ref head, 1) ^ Unsafe.Add(ref head, 2);
    }

    public override string ToString()
    {
        if (IsDefault)
            return "<default>";

        ref byte head = ref Unsafe.As<DAsyncId, byte>(ref Unsafe.AsRef(in this));
        ReadOnlySpan<byte> bytes = MemoryMarshal.CreateReadOnlySpan(ref head, ByteCount);

        return Convert.ToBase64String(bytes);
    }

    public static bool operator ==(DAsyncId left, DAsyncId right) => left.Equals(right);

    public static bool operator !=(DAsyncId left, DAsyncId right) => !left.Equals(right);

    internal static DAsyncId New()
    {
        Span<byte> bytes = stackalloc byte[ByteCount];
        
        Create(bytes);
        return new(bytes);
    }

    internal static DAsyncId NewFlowId()
    {
        Span<byte> bytes = stackalloc byte[ByteCount];
        
        Create(bytes);
        bytes[ByteCount - 1] |= FlowIdMask;
        return new(bytes);
    }

    private static void Create(Span<byte> bytes)
    {
        do
        {
            Randomize(bytes);
        }
        while (IsDefault(bytes));

        bytes[ByteCount - 1] &= ReservedBitsInvertedMask;

        static bool IsDefault(Span<byte> bytes)
        {
            foreach (byte b in bytes)
            {
                if (b != 0)
                    return false;
            }

            return true;
        }
    }

    private static void Randomize(Span<byte> bytes)
    {
#if DEBUG_TESTS
        s_idCount++;
        string stringId = s_idCount.ToString("0000000000000000");
        bool hasConverted = Convert.TryFromBase64String(stringId, bytes, out int bytesWritten);
        Debug.Assert(hasConverted && bytesWritten == ByteCount);
#elif NET6_0_OR_GREATER
        Random.Shared.NextBytes(bytes);
#else
        s_randomLocal.Value.NextBytes(bytes);
#endif
    }

    public static DAsyncId Parse(string value)
    {
        ThrowHelper.ThrowIfNull(value);

        if (!TryParseCore(value, out DAsyncId id))
            throw new ArgumentException($"'{value}' does not represent a valid {nameof(DAsyncId)}.", nameof(value));

        return id;
    }

    public static bool TryParse(string value, out DAsyncId id)
    {
        ThrowHelper.ThrowIfNull(value);

        return TryParseCore(value, out id);
    }

    public static DAsyncId ReadBytes(ReadOnlySpan<byte> bytes)
    {
        if (!TryReadBytesCore(bytes, out DAsyncId id))
            throw new ArgumentException($"The provided bytes do not represent a valid {nameof(DAsyncId)}.", nameof(bytes));

        return id;
    }

    public static bool TryReadBytes(ReadOnlySpan<byte> bytes, out DAsyncId id)
    {
        return TryReadBytesCore(bytes, out id);
    }

    private static bool TryParseCore(string value, out DAsyncId id)
    {
        Span<byte> bytes = stackalloc byte[ByteCount];
        if (!Convert.TryFromBase64String(value, bytes, out int bytesWritten) || bytesWritten != ByteCount)
        {
            id = default;
            return false;
        }

        return TryReadBytesCore(bytes, out id);
    }

    private static bool TryReadBytesCore(ReadOnlySpan<byte> bytes, out DAsyncId id)
    {
        if (bytes.Length != ByteCount)
        {
            id = default;
            return false;
        }

        id = new DAsyncId(bytes);
        return true;
    }
}
