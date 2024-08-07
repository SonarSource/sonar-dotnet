// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.Lightup
{
    internal delegate bool TryGetValueAccessor<T, TKey, TValue>(T instance, TKey key, out TValue value);
    internal delegate bool TryGetValueAccessor<T, TFirst, TSecond, TValue>(T instance, TFirst first, TSecond second, out TValue value); // Sonar
    internal delegate bool TryGetValueAccessor<T, TFirst, TSecond, TThird, TValue>(T instance, TFirst first, TSecond second, TThird third, out TValue value); // Sonar
}
