using System.Reflection;

namespace Just.Railway;

internal static class ReflectionHelper
{
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEqual<T>(T? left, T? right) => TypeReflectionCache<T>.IsEqualFunc(left, right);

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Compare<T>(T? left, T? right) => TypeReflectionCache<T>.CompareFunc(left, right);

    private static class TypeReflectionCache<T>
    {
        public static readonly Func<T?, T?, bool> IsEqualFunc;
        public static readonly Func<T?, T?, int> CompareFunc;

        static TypeReflectionCache()
        {
            var type = typeof(T);
            var isNullableStruct = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
            var underlyingType = isNullableStruct ? type.GenericTypeArguments.First() : type;
            var thisType = typeof(TypeReflectionCache<T>);

            var equatableType = typeof(IEquatable<>).MakeGenericType(underlyingType);
            if (equatableType.IsAssignableFrom(underlyingType))
            {
                var isEqualFunc = thisType.GetMethod(isNullableStruct ? nameof(IsEqualNullable) : nameof(IsEqual), BindingFlags.Static | BindingFlags.Public)
                    !.MakeGenericMethod(underlyingType);

                IsEqualFunc = (Func<T?, T?, bool>)Delegate.CreateDelegate(typeof(Func<T?, T?, bool>), isEqualFunc);
            }
            else
            {
                IsEqualFunc = static (left, right) => left is null ? right is null : left.Equals(right);
            }

            var comparableType = typeof(IComparable<>).MakeGenericType(underlyingType);
            if (comparableType.IsAssignableFrom(underlyingType))
            {
                var compareFunc = thisType.GetMethod(isNullableStruct ? nameof(CompareNullable) : nameof(Compare), BindingFlags.Static | BindingFlags.Public)
                    !.MakeGenericMethod(underlyingType);

                CompareFunc = (Func<T?, T?, int>)Delegate.CreateDelegate(typeof(Func<T?, T?, int>), compareFunc);
            }
            else
            {
                CompareFunc = static (left, right) => left is null
                    ? right is null ? 0 : -1
                    : right is null ? 1 : left.GetHashCode().CompareTo(right.GetHashCode());
            }
        }

    #pragma warning disable CS8604 // Possible null reference argument.
        [Pure] public static bool IsEqual<R>(R? left, R? right) where R : notnull, IEquatable<R>, T => left is null ? right is null : left.Equals(right);
        [Pure] public static bool IsEqualNullable<R>(R? left, R? right) where R : struct, IEquatable<R> => left is null ? right is null : right is not null && left.Value.Equals(right.Value);

        [Pure] public static int Compare<R>(R? left, R? right) where R : notnull, IComparable<R>, T => left is null
            ? right is null ? 0 : -1
            : right is null ? 1 : left.CompareTo(right);
        [Pure] public static int CompareNullable<R>(R? left, R? right) where R : struct, IComparable<R> => left is null
            ? right is null ? 0 : -1
            : right is null ? 1 : left.Value.CompareTo(right.Value);
    #pragma warning restore CS8604 // Possible null reference argument.
    }
}
