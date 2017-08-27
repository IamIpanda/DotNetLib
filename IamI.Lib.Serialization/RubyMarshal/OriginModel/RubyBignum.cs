using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace IamI.Lib.Serialization.RubyMarshal.OriginModel
{
    //disassembled from Microsoft.Scripting.Math.RubyBignum @ Microsoft.Dynamic, Version=1.0.0.0
    [Serializable]
    public sealed class RubyBignum : RubyObject, IFormattable, IComparable, IConvertible, IEquatable<RubyBignum>
    {
        // Fields
        private const ulong Base = 0x100000000L;
        private const int bias = 0x433;
        private const int BitsPerDigit = 0x20;
        private readonly uint[] data;
        private const int DecimalScaleFactorMask = 0xff0000;
        private const int DecimalSignMask = -2147483648;
        public static readonly RubyBignum One = new RubyBignum(1, new uint[] {1});
        private readonly short sign;
        public static readonly RubyBignum Zero = new RubyBignum(0, new uint[0]);
        // Methods
        public RubyBignum(RubyBignum copy)
        {
            if (ReferenceEquals(copy, null)) throw new ArgumentNullException(nameof(copy));
            sign = copy.sign;
            data = copy.data;
            ClassName = RubySymbol.GetSymbol("Bignum");
        }

        public RubyBignum(byte[] bytes) : this(Create(bytes)) { }

        public RubyBignum(int sign, params uint[] data)
        {
            ContractUtils.RequiresNotNull(data, "data");
            ContractUtils.Requires(sign >= -1 && sign <= 1, "sign");
            var length = GetLength(data);
            ContractUtils.Requires(length == 0 || sign != 0, "sign");
            this.data = data;
            this.sign = length == 0 ? (short) 0 : (short) sign;
            ClassName = RubySymbol.GetSymbol("Bignum");
        }

        public RubyBignum Abs()
        {
            if (sign == -1) { return -this; }
            return this;
        }

        public static RubyBignum Add(RubyBignum x, RubyBignum y) { return x + y; }

        private static uint[] add0(uint[] x, int xl, uint[] y, int yl) { return xl >= yl ? InternalAdd(x, xl, y, yl) : InternalAdd(y, yl, x, xl); }

        public bool AsDecimal(out decimal ret)
        {
            if (sign == 0)
            {
                ret = 0M;
                return true;
            }
            var length = GetLength();
            if (length > 3)
            {
                ret = 0M;
                return false;
            }
            var lo = 0;
            var mid = 0;
            var hi = 0;
            if (length > 2) hi = (int) data[2];
            if (length > 1) mid = (int) data[1];
            if (length > 0) lo = (int) data[0];
            ret = new decimal(lo, mid, hi, sign < 0, 0);
            return true;
        }

        public bool AsInt32(out int ret)
        {
            ret = 0;
            if (sign == 0) return true;
            if (GetLength() > 1) return false;
            if (data[0] > 0x80000000) return false;
            if (data[0] == 0x80000000 && sign == 1) return false;
            ret = (int) data[0];
            ret *= sign;
            return true;
        }

        public bool AsInt64(out long ret)
        {
            ret = 0L;
            if (sign == 0) return true;
            if (GetLength() > 2) return false;
            if (data.Length == 1)
            {
                ret = sign * data[0];
                return true;
            }
            ulong num = (data[1] << 0x20) | data[0];
            if (num > 9223372036854775808L) return false;
            if (num == 9223372036854775808L && sign == 1) return false;
            ret = (long) num * sign;
            return true;
        }

        public bool AsUInt32(out uint ret)
        {
            ret = 0;
            if (sign == 0) return true;
            if (sign < 0) return false;
            if (GetLength() > 1) return false;
            ret = data[0];
            return true;
        }

        public bool AsUInt64(out ulong ret)
        {
            ret = 0L;
            if (sign == 0) return true;
            if (sign < 0) return false;
            if (GetLength() > 2) return false;
            ret = data[0];
            if (data.Length > 1) ret |= data[1] << 0x20;
            return true;
        }

        public static RubyBignum BitwiseAnd(RubyBignum x, RubyBignum y) { return x & y; }

        public static RubyBignum BitwiseOr(RubyBignum x, RubyBignum y) { return x | y; }

        public static int Compare(RubyBignum x, RubyBignum y)
        {
            if (ReferenceEquals(x, null)) throw new ArgumentNullException(nameof(x));
            if (ReferenceEquals(y, null)) throw new ArgumentNullException(nameof(y));
            if (x.sign == y.sign)
            {
                var length = x.GetLength();
                var num2 = y.GetLength();
                if (length == num2)
                {
                    for (var i = length - 1; i >= 0; i--)
                        if (x.data[i] != y.data[i])
                        {
                            if (x.data[i] <= y.data[i]) return -x.sign;
                            return x.sign;
                        }
                    return 0;
                }
                if (length <= num2) return -x.sign;
                return x.sign;
            }
            if (x.sign <= y.sign) return -1;
            return 1;
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;
            var objA = obj as RubyBignum;
            if (ReferenceEquals(objA, null)) throw new ArgumentException("expected integer");
            return Compare(this, objA);
        }

        private static uint[] copy(uint[] v)
        {
            var destinationArray = new uint[v.Length];
            Array.Copy(v, destinationArray, v.Length);
            return destinationArray;
        }

        public static RubyBignum Create(decimal v)
        {
            var bits = decimal.GetBits(decimal.Truncate(v));
            var num = 3;
            while (num > 0 && bits[num - 1] == 0) num--;
            if (num == 0) return Zero;
            var data = new uint[num];
            data[0] = (uint) bits[0];
            if (num > 1) data[1] = (uint) bits[1];
            if (num > 2) data[2] = (uint) bits[2];
            return new RubyBignum((bits[3] & -2147483648) != 0 ? -1 : 1, data);
        }

        public static RubyBignum Create(double v)
        {
            if (double.IsNaN(v) || double.IsInfinity(v)) throw new OverflowException();
            var bytes = BitConverter.GetBytes(v);
            var num = Mantissa(bytes);
            if (num == 0L)
            {
                int num2 = Exponent(bytes);
                if (num2 == 0) return Zero;
                var integer = Negative(bytes) ? Negate(One) : One;
                return integer << (num2 - 0x3ff);
            }
            int num3 = Exponent(bytes);
            num |= (ulong) 0x10000000000000L;
            var integer2 = Create(num);
            integer2 = num3 > 0x433 ? integer2 << (num3 - 0x433) : integer2 >> (0x433 - num3);
            if (!Negative(bytes)) return integer2;
            return integer2 * -1;
        }

        public static RubyBignum Create(int v)
        {
            if (v == 0) return Zero;
            if (v == 1) return One;
            if (v < 0) return new RubyBignum(-1, new uint[] {(uint) -v});
            return new RubyBignum(1, new uint[] {(uint) v});
        }

        public static RubyBignum Create(long v)
        {
            ulong num;
            var sign = 1;
            if (v < 0L)
            {
                num = (ulong) -v;
                sign = -1;
            }
            else num = (ulong) v;
            return new RubyBignum(sign, new uint[] {(uint) num, (uint) (num >> 0x20)});
        }

        public static RubyBignum Create(uint v)
        {
            if (v == 0) return Zero;
            if (v == 1) return One;
            return new RubyBignum(1, new uint[] {v});
        }

        public static RubyBignum Create(ulong v) { return new RubyBignum(1, new uint[] {(uint) v, (uint) (v >> 0x20)}); }

        public static RubyBignum Create(byte[] v)
        {
            ContractUtils.RequiresNotNull(v, "v");
            if (v.Length == 0) return Create(0);
            var length = v.Length;
            var num2 = length % 4;
            var num3 = length / 4 + (num2 == 0 ? 0 : 1);
            var d = new uint[num3];
            var flag = (v[length - 1] & 0x80) == 0x80;
            var flag2 = true;
            var index = 3;
            var num4 = 0;
            while (num4 < num3 - (num2 == 0 ? 0 : 1))
            {
                for (var i = 0; i < 4; i++)
                {
                    if (v[index] != 0) flag2 = false;
                    d[num4] = d[num4] << 8;
                    d[num4] |= v[index];
                    index--;
                }
                index += 8;
                num4++;
            }
            if (num2 != 0)
            {
                if (flag) d[num3 - 1] = uint.MaxValue;
                for (index = length - 1; index >= length - num2; index--)
                {
                    if (v[index] != 0) flag2 = false;
                    d[num4] = d[num4] << 8;
                    d[num4] |= v[index];
                }
            }
            if (flag2) return Zero;
            if (flag)
            {
                makeTwosComplement(d);
                return new RubyBignum(-1, d);
            }
            return new RubyBignum(1, d);
        }

        public static RubyBignum Divide(RubyBignum x, RubyBignum y) { return x / y; }

        private static void DivModUnsigned(uint[] u, uint[] v, out uint[] q, out uint[] r)
        {
            var length = GetLength(u);
            var l = GetLength(v);
            if (l <= 1)
            {
                if (l == 0) throw new DivideByZeroException();
                ulong num3 = 0L;
                var num4 = v[0];
                q = new uint[length];
                r = new uint[1];
                for (var i = length - 1; i >= 0; i--)
                {
                    num3 *= (ulong) 0x100000000L;
                    num3 += u[i];
                    var num6 = num3 / (ulong) num4;
                    num3 -= num6 * num4;
                    q[i] = (uint) num6;
                }
                r[0] = (uint) num3;
            }
            else if (length >= l)
            {
                var normalizeShift = GetNormalizeShift(v[l - 1]);
                var un = new uint[length + 1];
                var numArray2 = new uint[l];
                Normalize(u, length, un, normalizeShift);
                Normalize(v, l, numArray2, normalizeShift);
                q = new uint[length - l + 1];
                r = null;
                for (var j = length - l; j >= 0; j--)
                {
                    var num9 = (ulong) (0x100000000L * un[j + l] + un[j + l - 1]);
                    var num10 = num9 / (ulong) numArray2[l - 1];
                    num9 -= num10 * numArray2[l - 1];
                    do
                    {
                        if (num10 < 0x100000000L && num10 * numArray2[l - 2] <= num9 * (ulong) 0x100000000L + un[j + l - 2]) break;
                        num10 -= (ulong) 1L;
                        num9 += numArray2[l - 1];
                    } while (num9 < 0x100000000L);
                    var num12 = 0L;
                    var num13 = 0L;
                    var index = 0;
                    while (index < l)
                    {
                        var num14 = numArray2[index] * num10;
                        num13 = un[index + j] - (uint) num14 - num12;
                        un[index + j] = (uint) num13;
                        num14 = num14 >> 0x20;
                        num13 = num13 >> 0x20;
                        num12 = (long) num14 - num13;
                        index++;
                    }
                    num13 = un[j + l] - num12;
                    un[j + l] = (uint) num13;
                    q[j] = (uint) num10;
                    if (num13 < 0L)
                    {
                        q[j]--;
                        ulong num15 = 0L;
                        for (index = 0; index < l; index++)
                        {
                            num15 = numArray2[index] + un[j + index] + num15;
                            un[j + index] = (uint) num15;
                            num15 = num15 >> 0x20;
                        }
                        num15 += un[j + l];
                        un[j + l] = (uint) num15;
                    }
                }
                Unnormalize(un, out r, normalizeShift);
            }
            else
            {
                var numArray3 = new uint[1];
                q = numArray3;
                r = u;
            }
        }

        public static RubyBignum DivRem(RubyBignum x, RubyBignum y, out RubyBignum remainder)
        {
            if (ReferenceEquals(x, null)) throw new ArgumentNullException(nameof(x));
            if (ReferenceEquals(y, null)) throw new ArgumentNullException(nameof(y));
            DivModUnsigned(x.data, y.data, out uint[] numArray, out uint[] numArray2);
            remainder = new RubyBignum(x.sign, numArray2);
            return new RubyBignum(x.sign * y.sign, numArray);
        }

        public bool Equals(RubyBignum other)
        {
            if (ReferenceEquals(other, null)) return false;
            return this == other;
        }

        public override bool Equals(object obj) { return Equals(obj as RubyBignum); }

        private static ushort Exponent(byte[] v) { return (ushort) (((ushort) (v[7] & 0x7f) << 4) | ((ushort) (v[6] & 240) >> 4)); }

        private static uint extend(uint v, ref bool seenNonZero)
        {
            if (seenNonZero) return ~v;
            if (v == 0) return 0;
            seenNonZero = true;
            return ~v + 1;
        }

        public int GetBitCount()
        {
            if (IsZero()) return 1;
            var index = GetLength() - 1;
            var num2 = data[index];
            var num3 = index * 0x20;
            do
            {
                num2 = num2 >> 1;
                num3++;
            } while (num2 > 0);
            return num3;
        }

        public int GetByteCount() { return (GetBitCount() + 7) / 8; }

        public override int GetHashCode()
        {
            if (data.Length == 0) return 0;
            uint num = 0;
            foreach (var num2 in data) num += num2;
            var num3 = (int) num;
            if (IsNegative()) return -num3;
            return num3;
        }

        private int GetLength() { return GetLength(data); }

        private static int GetLength(uint[] data)
        {
            var index = data.Length - 1;
            while (index >= 0 && data[index] == 0) index--;
            return index + 1;
        }

        private static int GetNormalizeShift(uint value)
        {
            var num = 0;
            if ((value & 0xffff0000) == 0)
            {
                value = value << 0x10;
                num += 0x10;
            }
            if ((value & 0xff000000) == 0)
            {
                value = value << 8;
                num += 8;
            }
            if ((value & 0xf0000000) == 0)
            {
                value = value << 4;
                num += 4;
            }
            if ((value & 0xc0000000) == 0)
            {
                value = value << 2;
                num += 2;
            }
            if ((value & 0x80000000) == 0)
            {
                value = value << 1;
                num++;
            }
            return num;
        }

        private static uint getOne(bool isNeg, uint[] data, int i, ref bool seenNonZero)
        {
            if (i < data.Length)
            {
                var v = data[i];
                if (!isNeg) return v;
                return extend(v, ref seenNonZero);
            }
            if (!isNeg) return 0;
            return uint.MaxValue;
        }

        public TypeCode GetTypeCode() { return TypeCode.Object; }

        public uint GetWord(int index) { return data[index]; }

        public int GetWordCount() { return IsZero() ? 1 : GetLength(); }

        public uint[] GetWords()
        {
            if (sign == 0) return new uint[1];
            var length = GetLength();
            var destinationArray = new uint[length];
            Array.Copy(data, destinationArray, length);
            return destinationArray;
        }

        private static uint[] InternalAdd(uint[] x, int xl, uint[] y, int yl)
        {
            var v = new uint[xl];
            ulong num2 = 0L;
            var index = 0;
            while (index < yl)
            {
                num2 = num2 + x[index] + y[index];
                v[index] = (uint) num2;
                num2 = num2 >> 0x20;
                index++;
            }
            while (index < xl && num2 != 0L)
            {
                num2 += x[index];
                v[index] = (uint) num2;
                num2 = num2 >> 0x20;
                index++;
            }
            if (num2 == 0L)
            {
                while (index < xl)
                {
                    v[index] = x[index];
                    index++;
                }
                return v;
            }
            v = resize(v, xl + 1);
            v[index] = (uint) num2;
            return v;
        }

        public bool IsNegative() { return sign < 0; }

        private bool IsOdd() { return data != null && data.Length > 0 && (data[0] & 1) != 0; }

        public bool IsPositive() { return sign > 0; }

        public bool IsZero() { return sign == 0; }

        public static RubyBignum LeftShift(RubyBignum x, int shift) { return x << shift; }

        public double Log() { return Log(2.7182818284590451); }

        public double Log(double newBase)
        {
            if (IsNegative() || newBase == 1.0 || this == Zero || newBase == 0.0 && this != One) return double.NaN;
            if (newBase == double.PositiveInfinity) return !(this == One) ? double.NaN : 0.0;
            var index = GetLength() - 1;
            var num2 = -1;
            for (var i = 0x1f; i >= 0; i--)
                if ((data[index] & ((int) 1 << i)) != 0L)
                {
                    num2 = i + index * 0x20;
                    break;
                }
            long num4 = num2;
            var d = 0.0;
            var num6 = 1.0;
            var one = One;
            var num7 = num4;
            while (num7 > 0x7fffffffL)
            {
                one = one << 0x7fffffff;
                num7 -= 0x7fffffffL;
            }
            one = one << (int) num7;
            for (var j = num4; j >= 0L; j -= 1L)
            {
                if ((this & one) != Zero) d += num6;
                num6 *= 0.5;
                one = one >> 1;
            }
            return (Math.Log(d) + Math.Log(2.0) * num4) / Math.Log(newBase);
        }

        public double Log10() { return Log(10.0); }

        private static uint[] makeTwosComplement(uint[] d)
        {
            var index = 0;
            uint num2 = 0;
            while (index < d.Length)
            {
                num2 = ~d[index] + 1;
                d[index] = num2;
                if (num2 != 0)
                {
                    index++;
                    break;
                }
                index++;
            }
            if (num2 != 0)
            {
                while (index < d.Length)
                {
                    d[index] = ~d[index];
                    index++;
                }
                return d;
            }
            d = resize(d, d.Length + 1);
            d[d.Length - 1] = 1;
            return d;
        }

        private static ulong Mantissa(byte[] v)
        {
            var num = (uint) (v[0] | (v[1] << 8) | (v[2] << 0x10) | (v[3] << 0x18));
            var num2 = (uint) (v[4] | (v[5] << 8) | ((v[6] & 15) << 0x10));
            return num | (num2 << 0x20);
        }

        public static RubyBignum Mod(RubyBignum x, RubyBignum y) { return x % y; }

        public RubyBignum ModPow(RubyBignum power, RubyBignum mod)
        {
            if (ReferenceEquals(power, null)) throw new ArgumentNullException(nameof(power));
            if (ReferenceEquals(mod, null)) throw new ArgumentNullException(nameof(mod));
            if (power < 0) throw new ArgumentOutOfRangeException(nameof(power), "power must be >= 0");
            var integer = this;
            var integer2 = One % mod;
            while (power != Zero)
            {
                if (power.IsOdd())
                {
                    integer2 *= integer;
                    integer2 = integer2 % mod;
                }
                if (power == One) { return integer2; }
                integer = integer.Square() % mod;
                power = power >> 1;
            }
            return integer2;
        }

        public RubyBignum ModPow(int power, RubyBignum mod)
        {
            if (ReferenceEquals(mod, null)) throw new ArgumentNullException(nameof(mod));
            if (power < 0) throw new ArgumentOutOfRangeException(nameof(power), "power must be >= 0");
            var integer = this;
            var integer2 = One % mod;
            while (power != 0)
            {
                if ((power & 1) != 0)
                {
                    integer2 *= integer;
                    integer2 = integer2 % mod;
                }
                if (power == 1) return integer2;
                integer = integer.Square() % mod;
                power = power >> 1;
            }
            return integer2;
        }

        public static RubyBignum Multiply(RubyBignum x, RubyBignum y) { return x * y; }

        public static RubyBignum Negate(RubyBignum x) { return -x; }

        private static bool Negative(byte[] v) { return (v[7] & 0x80) != 0; }

        private static void Normalize(uint[] u, int l, uint[] un, int shift)
        {
            int num2;
            uint num = 0;
            if (shift > 0)
            {
                var num3 = 0x20 - shift;
                for (num2 = 0; num2 < l; num2++)
                {
                    var num4 = u[num2];
                    un[num2] = (num4 << shift) | num;
                    num = num4 >> num3;
                }
            }
            else
            {
                num2 = 0;
                while (num2 < l)
                {
                    un[num2] = u[num2];
                    num2++;
                }
            }
            while (num2 < un.Length) un[num2++] = 0;
            if (num != 0) un[l] = num;
        }

        public RubyBignum OnesComplement() { return ~this; }

        public static RubyBignum operator +(RubyBignum x, RubyBignum y)
        {
            if (ReferenceEquals(x, null)) throw new ArgumentNullException(nameof(x));
            if (ReferenceEquals(y, null)) throw new ArgumentNullException(nameof(y));
            if (x.sign == y.sign) return new RubyBignum(x.sign, add0(x.data, x.GetLength(), y.data, y.GetLength()));
            return x - new RubyBignum(-y.sign, y.data);
        }

        public static RubyBignum operator &(RubyBignum x, RubyBignum y)
        {
            if (ReferenceEquals(x, null)) throw new ArgumentNullException(nameof(x));
            if (ReferenceEquals(y, null)) throw new ArgumentNullException(nameof(y));
            var length = x.GetLength();
            var num2 = y.GetLength();
            var data = x.data;
            var numArray2 = y.data;
            var num3 = Math.Max(length, num2);
            var d = new uint[num3];
            var isNeg = x.sign == -1;
            var flag2 = y.sign == -1;
            var seenNonZero = false;
            var flag4 = false;
            for (var i = 0; i < num3; i++)
            {
                var num5 = getOne(isNeg, data, i, ref seenNonZero);
                var num6 = getOne(flag2, numArray2, i, ref flag4);
                d[i] = num5 & num6;
            }
            if (isNeg && flag2) return new RubyBignum(-1, makeTwosComplement(d));
            if (!isNeg && !flag2) return new RubyBignum(1, d);
            return new RubyBignum(1, d);
        }

        public static RubyBignum operator |(RubyBignum x, RubyBignum y)
        {
            if (ReferenceEquals(x, null)) throw new ArgumentNullException(nameof(x));
            if (ReferenceEquals(y, null)) throw new ArgumentNullException(nameof(y));
            var length = x.GetLength();
            var num2 = y.GetLength();
            var data = x.data;
            var numArray2 = y.data;
            var num3 = Math.Max(length, num2);
            var numArray3 = new uint[num3];
            var isNeg = x.sign == -1;
            var flag2 = y.sign == -1;
            var seenNonZero = false;
            var flag4 = false;
            for (var i = 0; i < num3; i++)
            {
                var num5 = getOne(isNeg, data, i, ref seenNonZero);
                var num6 = getOne(flag2, numArray2, i, ref flag4);
                numArray3[i] = num5 | num6;
            }
            if ((!isNeg || !flag2) && !isNeg && !flag2) { return new RubyBignum(1, numArray3); }
            return new RubyBignum(-1, makeTwosComplement(numArray3));
        }

        public static RubyBignum operator /(RubyBignum x, RubyBignum y) { return DivRem(x, y, out RubyBignum integer); }

        public static bool operator ==(RubyBignum x, RubyBignum y) { return Compare(x, y) == 0; }

        public static bool operator ==(RubyBignum x, double y)
        {
            if (ReferenceEquals(x, null)) throw new ArgumentNullException(nameof(x));
            if (y % 1.0 != 0.0) return false;
            return x == Create(y);
        }

        public static bool operator ==(RubyBignum x, int y) { return x == y; }

        public static bool operator ==(double x, RubyBignum y) { return y == x; }

        public static RubyBignum operator ^(RubyBignum x, RubyBignum y)
        {
            if (ReferenceEquals(x, null)) { throw new ArgumentNullException(nameof(x)); }
            if (ReferenceEquals(y, null)) { throw new ArgumentNullException(nameof(y)); }
            var length = x.GetLength();
            var num2 = y.GetLength();
            var data = x.data;
            var numArray2 = y.data;
            var num3 = Math.Max(length, num2);
            var numArray3 = new uint[num3];
            var isNeg = x.sign == -1;
            var flag2 = y.sign == -1;
            var seenNonZero = false;
            var flag4 = false;
            for (var i = 0; i < num3; i++)
            {
                var num5 = getOne(isNeg, data, i, ref seenNonZero);
                var num6 = getOne(flag2, numArray2, i, ref flag4);
                numArray3[i] = num5 ^ num6;
            }
            if (isNeg && flag2) { return new RubyBignum(1, numArray3); }
            if (!isNeg && !flag2) { return new RubyBignum(1, numArray3); }
            return new RubyBignum(-1, makeTwosComplement(numArray3));
        }

        public static explicit operator byte(RubyBignum self)
        {
            if (!self.AsInt32(out int num)) throw new OverflowException();
            return (byte) num;
        }

        public static explicit operator decimal(RubyBignum self)
        {
            if (!self.AsDecimal(out decimal num)) throw new OverflowException();
            return num;
        }

        public static explicit operator double(RubyBignum self)
        {
            if (ReferenceEquals(self, null)) throw new ArgumentNullException(nameof(self));
            return self.ToFloat64();
        }

        public static explicit operator short(RubyBignum self)
        {
            if (!self.AsInt32(out int num)) throw new OverflowException();
            return (short) num;
        }

        public static explicit operator int(RubyBignum self)
        {
            if (!self.AsInt32(out int num)) throw new OverflowException();
            return num;
        }

        public static explicit operator long(RubyBignum self)
        {
            if (!self.AsInt64(out long num)) throw new OverflowException();
            return num;
        }

        public static explicit operator sbyte(RubyBignum self)
        {
            if (!self.AsInt32(out int num)) throw new OverflowException();
            return (sbyte) num;
        }

        public static explicit operator float(RubyBignum self)
        {
            if (ReferenceEquals(self, null)) throw new ArgumentNullException(nameof(self));
            return (float) self.ToFloat64();
        }

        public static explicit operator ushort(RubyBignum self)
        {
            if (!self.AsInt32(out int num)) throw new OverflowException();
            return (ushort) num;
        }

        public static explicit operator uint(RubyBignum self)
        {
            if (!self.AsUInt32(out uint num)) throw new OverflowException();
            return num;
        }

        public static explicit operator ulong(RubyBignum self)
        {
            if (!self.AsUInt64(out ulong num)) throw new OverflowException();
            return num;
        }

        public static explicit operator RubyBignum(double self) { return Create(self); }

        public static explicit operator RubyBignum(float self) { return Create((double) self); }

        public static bool operator >(RubyBignum x, RubyBignum y) { return Compare(x, y) > 0; }

        public static bool operator >=(RubyBignum x, RubyBignum y) { return Compare(x, y) >= 0; }

        public static implicit operator RubyBignum(byte i) { return Create((uint) i); }

        public static implicit operator RubyBignum(decimal i) { return Create(i); }

        public static implicit operator RubyBignum(short i) { return Create((int) i); }

        public static implicit operator RubyBignum(int i) { return Create(i); }

        public static implicit operator RubyBignum(long i) { return Create(i); }

        public static implicit operator RubyBignum(sbyte i) { return Create((int) i); }

        public static implicit operator RubyBignum(ushort i) { return Create((uint) i); }

        public static implicit operator RubyBignum(uint i) { return Create(i); }

        public static implicit operator RubyBignum(ulong i) { return Create(i); }

        public static bool operator !=(RubyBignum x, RubyBignum y) { return Compare(x, y) != 0; }

        public static bool operator !=(RubyBignum x, double y) { return x != y; }

        public static bool operator !=(RubyBignum x, int y) { return x != y; }

        public static bool operator !=(double x, RubyBignum y) { return x != y; }

        public static RubyBignum operator <<(RubyBignum x, int shift)
        {
            if (ReferenceEquals(x, null)) throw new ArgumentNullException(nameof(x));
            if (shift == 0) return x;
            if (shift < 0) return x >> -shift;
            var num = shift / 0x20;
            var num2 = shift - num * 0x20;
            var length = x.GetLength();
            var data = x.data;
            var num4 = length + num + 1;
            var numArray2 = new uint[num4];
            if (num2 == 0) for (var i = 0; i < length; i++) numArray2[i + num] = data[i];

            else
            {
                var num6 = 0x20 - num2;
                uint num7 = 0;
                var index = 0;
                while (index < length)
                {
                    var num9 = data[index];
                    numArray2[index + num] = (num9 << num2) | num7;
                    num7 = num9 >> num6;
                    index++;
                }
                numArray2[index + num] = num7;
            }
            return new RubyBignum(x.sign, numArray2);
        }

        public static bool operator <(RubyBignum x, RubyBignum y) { return Compare(x, y) < 0; }

        public static bool operator <=(RubyBignum x, RubyBignum y) { return Compare(x, y) <= 0; }

        public static RubyBignum operator %(RubyBignum x, RubyBignum y)
        {
            DivRem(x, y, out RubyBignum integer);
            return integer;
        }

        public static RubyBignum operator *(RubyBignum x, RubyBignum y)
        {
            if (ReferenceEquals(x, null)) { throw new ArgumentNullException(nameof(x)); }
            if (ReferenceEquals(y, null)) { throw new ArgumentNullException(nameof(y)); }
            var length = x.GetLength();
            var num2 = y.GetLength();
            var num3 = length + num2;
            var data = x.data;
            var numArray2 = y.data;
            var numArray3 = new uint[num3];
            for (var i = 0; i < length; i++)
            {
                var num5 = data[i];
                var index = i;
                ulong num7 = 0L;
                for (var j = 0; j < num2; j++)
                {
                    num7 = num7 + num5 * numArray2[j] + numArray3[index];
                    numArray3[index++] = (uint) num7;
                    num7 = num7 >> 0x20;
                }
                while (num7 != 0L)
                {
                    num7 += numArray3[index];
                    numArray3[index++] = (uint) num7;
                    num7 = num7 >> 0x20;
                }
            }
            return new RubyBignum(x.sign * y.sign, numArray3);
        }

        public static RubyBignum operator ~(RubyBignum x)
        {
            if (ReferenceEquals(x, null)) throw new ArgumentNullException(nameof(x));
            return -(x + One);
        }

        public static RubyBignum operator >>(RubyBignum x, int shift)
        {
            if (ReferenceEquals(x, null)) throw new ArgumentNullException(nameof(x));
            if (shift == 0) return x;
            if (shift < 0) return x << -shift;
            var index = shift / 0x20;
            var num2 = shift - index * 0x20;
            var length = x.GetLength();
            var data = x.data;
            var num4 = length - index;
            if (num4 < 0) num4 = 0;
            var numArray2 = new uint[num4];
            if (num2 == 0) for (var i = length - 1; i >= index; i--) numArray2[i - index] = data[i];

            else
            {
                var num6 = 0x20 - num2;
                uint num7 = 0;
                for (var j = length - 1; j >= index; j--)
                {
                    var num9 = data[j];
                    numArray2[j - index] = (num9 >> num2) | num7;
                    num7 = num9 << num6;
                }
            }
            var integer = new RubyBignum(x.sign, numArray2);
            if (x.IsNegative())
            {
                for (var k = 0; k < index; k++) if (data[k] != 0) return integer - One;

                if (num2 > 0 && data[index] << (0x20 - num2) != 0) return integer - One;
            }
            return integer;
        }

        public static RubyBignum operator -(RubyBignum x, RubyBignum y)
        {
            uint[] numArray;
            var sign = Compare(x, y);
            if (sign != 0)
            {
                if (x.sign != y.sign) return new RubyBignum(sign, add0(x.data, x.GetLength(), y.data, y.GetLength()));
                switch (sign * x.sign)
                {
                    case -1:
                        numArray = sub(y.data, y.GetLength(), x.data, x.GetLength());
                        goto Label_0084;
                    case 1:
                        numArray = sub(x.data, x.GetLength(), y.data, y.GetLength());
                        goto Label_0084;
                }
            }
            return Zero;
            Label_0084:
            return new RubyBignum(sign, numArray);
        }

        public static RubyBignum operator -(RubyBignum x)
        {
            if (ReferenceEquals(x, null)) throw new ArgumentNullException(nameof(x));
            return new RubyBignum(-x.sign, x.data);
        }

        public RubyBignum Power(int exp)
        {
            if (exp == 0) { return One; }
            if (exp < 0) { throw new ArgumentOutOfRangeException(nameof(exp), "exp must be >= 0"); }
            var integer = this;
            var one = One;
            while (exp != 0)
            {
                if ((exp & 1) != 0) { one *= integer; }
                if (exp == 1) { return one; }
                integer = integer.Square();
                exp = exp >> 1;
            }
            return one;
        }

        private static uint[] resize(uint[] v, int len)
        {
            if (v.Length == len) return v;
            var numArray = new uint[len];
            var num = Math.Min(v.Length, len);
            for (var i = 0; i < num; i++) numArray[i] = v[i];
            return numArray;
        }

        public static RubyBignum RightShift(RubyBignum x, int shift) { return x >> shift; }

        public RubyBignum Square() { return this * this; }

        private static uint[] sub(uint[] x, int xl, uint[] y, int yl)
        {
            var numArray = new uint[xl];
            var flag = false;
            var index = 0;
            while (index < yl)
            {
                var maxValue = x[index];
                var num3 = y[index];
                if (flag)
                {
                    if (maxValue == 0)
                    {
                        maxValue = uint.MaxValue;
                        flag = true;
                    }
                    else
                    {
                        maxValue--;
                        flag = false;
                    }
                }
                if (num3 > maxValue) { flag = true; }
                numArray[index] = maxValue - num3;
                index++;
            }
            if (flag)
            {
                while (index < xl)
                {
                    var num4 = x[index];
                    numArray[index] = num4 - 1;
                    if (num4 != 0)
                    {
                        index++;
                        break;
                    }
                    index++;
                }
            }
            while (index < xl)
            {
                numArray[index] = x[index];
                index++;
            }
            return numArray;
        }

        public static RubyBignum Subtract(RubyBignum x, RubyBignum y) { return x - y; }

        string IFormattable.ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null) return ToString();
            switch (format[0])
            {
                case 'd':
                case 'D':
                {
                    if (format.Length <= 1) return ToString(10);
                    var num = Convert.ToInt32(format.Substring(1), CultureInfo.InvariantCulture.NumberFormat);
                    var str = ToString(10);
                    if (str.Length >= num) return str;
                    var str2 = new string('0', num - str.Length);
                    if (str[0] != '-') return str2 + str;
                    return "-" + str2 + str.Substring(1);
                }
                case 'x':
                case 'X':
                {
                    var builder = new StringBuilder(ToString(0x10));
                    if (format[0] == 'x') for (var i = 0; i < builder.Length; i++) if (builder[i] >= 'A' && builder[i] <= 'F') builder[i] = char.ToLower(builder[i], CultureInfo.InvariantCulture);
                    if (format.Length > 1)
                    {
                        var num3 = Convert.ToInt32(format.Substring(1), CultureInfo.InvariantCulture.NumberFormat);
                        if (builder.Length < num3)
                        {
                            var str3 = new string('0', num3 - builder.Length);
                            builder.Insert(builder[0] != '-' ? 0 : 1, str3);
                        }
                    }
                    return builder.ToString();
                }
            }
            throw new NotImplementedException("format not implemented");
        }

        public bool ToBoolean(IFormatProvider provider) { return this != Zero; }

        public byte ToByte(IFormatProvider provider)
        {
            if (!AsUInt32(out uint num) || (num & 18446744073709551360L) != 0L) { throw new OverflowException("big integer won't fit into byte"); }
            return (byte) num;
        }

        public byte[] ToByteArray()
        {
            uint[] data;
            byte num;
            if (sign == 0) return new byte[1];
            if (-1 == sign)
            {
                data = (uint[]) this.data.Clone();
                makeTwosComplement(data);
                num = 0xff;
            }
            else
            {
                data = this.data;
                num = 0;
            }
            var sourceArray = new byte[4 * data.Length];
            var num2 = 0;
            for (var i = 0; i < data.Length; i++)
            {
                var num3 = data[i];
                for (var j = 0; j < 4; j++)
                {
                    sourceArray[num2++] = (byte) (num3 & 0xff);
                    num3 = num3 >> 8;
                }
            }
            var index = sourceArray.Length - 1;
            while (index > 0)
            {
                if (sourceArray[index] != num) break;
                index--;
            }
            var flag = (sourceArray[index] & 0x80) != (num & 0x80);
            var destinationArray = new byte[index + 1 + (flag ? 1 : 0)];
            Array.Copy(sourceArray, destinationArray, (int) (index + 1));
            if (flag) destinationArray[destinationArray.Length - 1] = num;
            return destinationArray;
        }

        public char ToChar(IFormatProvider provider)
        {
            if (!AsInt32(out int num) || num > 0xffff || num < 0) throw new OverflowException("big integer won't fit into char");
            return (char) num;
        }

        public DateTime ToDateTime(IFormatProvider provider) { throw new NotImplementedException(); }

        public decimal ToDecimal()
        {
            if (!AsDecimal(out decimal num)) throw new OverflowException("big integer won't fit into decimal");
            return num;
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            if (!AsDecimal(out decimal num)) throw new OverflowException("big integer won't fit into decimal");
            return num;
        }

        public double ToDouble(IFormatProvider provider) { return ToFloat64(); }

        public double ToFloat64() { return double.Parse(ToString(10), CultureInfo.InvariantCulture.NumberFormat); }

        public short ToInt16(IFormatProvider provider)
        {
            if (!AsInt32(out int num) || num > 0x7fff || num < -32768) throw new OverflowException("big integer won't fit into short");
            return (short) num;
        }

        public int ToInt32()
        {
            if (!AsInt32(out int num)) throw new OverflowException("big integer won't fit into int");
            return num;
        }

        public int ToInt32(IFormatProvider provider)
        {
            if (!AsInt32(out int num)) throw new OverflowException("big integer won't fit into int");
            return num;
        }

        public long ToInt64()
        {
            if (!AsInt64(out long num)) throw new OverflowException("big integer won't fit into long");
            return num;
        }

        public long ToInt64(IFormatProvider provider)
        {
            if (!AsInt64(out long num)) throw new OverflowException("big integer won't fit into long");
            return num;
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            if (!AsInt32(out int num) || num > 0x7f || num < -128) throw new OverflowException("big integer won't fit into sbyte");
            return (sbyte) num;
        }

        public float ToSingle(IFormatProvider provider) { return (float) ToDouble(provider); }

        public override string ToString() { return ToString(10); }

        public string ToString(IFormatProvider provider) { return ToString(); }

        public string ToString(int radix) { return MathUtils.RubyBignumToString(copy(data), sign, radix); }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType != typeof(RubyBignum)) throw new NotImplementedException();
            return this;
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            if (!AsUInt32(out uint num) || num > 0xffff) throw new OverflowException("big integer won't fit into ushort");
            return (ushort) num;
        }

        public uint ToUInt32()
        {
            if (!AsUInt32(out uint num)) throw new OverflowException("big integer won't fit into uint");
            return num;
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            if (!AsUInt32(out uint num)) throw new OverflowException("big integer won't fit into uint");
            return num;
        }

        public ulong ToUInt64()
        {
            if (!AsUInt64(out ulong num)) throw new OverflowException("big integer won't fit into ulong");
            return num;
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            if (!AsUInt64(out ulong num)) throw new OverflowException("big integer won't fit into ulong");
            return num;
        }

        public bool TryToFloat64(out double result) { return double.TryParse(ToString(10), NumberStyles.Number, CultureInfo.InvariantCulture.NumberFormat, out result); }

        private static void Unnormalize(uint[] un, out uint[] r, int shift)
        {
            var length = GetLength(un);
            r = new uint[length];
            if (shift > 0)
            {
                var num2 = 0x20 - shift;
                uint num3 = 0;
                for (var i = length - 1; i >= 0; i--)
                {
                    var num5 = un[i];
                    r[i] = (num5 >> shift) | num3;
                    num3 = num5 << num2;
                }
            }
            else for (var j = 0; j < length; j++) r[j] = un[j];
        }

        public static RubyBignum Xor(RubyBignum x, RubyBignum y) { return x ^ y; }

        // Properties
        public short Sign => sign;

        internal static class ContractUtils
        {
            public static void Requires(bool precondition, string paramName)
            {
                if (!precondition) throw new ArgumentException("", paramName);
            }

            public static void RequiresNotNull(object value, string paramName)
            {
                if (value == null) throw new ArgumentNullException(paramName);
            }
        }

        private static class MathUtils
        {
            // Fields
            private static readonly double[] _RoundPowersOfTens = {1.0, 10.0, 100.0, 1000.0, 10000.0, 100000.0, 1000000.0, 10000000.0, 100000000.0, 1000000000.0, 10000000000, 100000000000, 1000000000000, 10000000000000, 100000000000000, 1E+15};

            private const int BitsPerDigit = 0x20;

            private static readonly uint[] groupRadixValues =
            {
                0, 0, 0x80000000, 0xcfd41b91, 0x40000000, 0x48c27395, 0x81bf1000, 0x75db9c97, 0x40000000, 0xcfd41b91, 0x3b9aca00, 0x8c8b6d2b, 0x19a10000, 0x309f1021, 0x57f6c100, 0x98c29b81, 0x10000000, 0x18754571, 0x247dbc80, 0x3547667b, 0x4c4b4000,
                0x6b5a6e1d, 0x94ace180, 0xcaf18367, 0xb640000, 0xe8d4a51, 0x1269ae40, 0x17179149, 0x1cb91000, 0x23744899, 0x2b73a840, 0x34e63b41, 0x40000000, 0x4cfa3cc1, 0x5c13d840, 0x6d91b519, 0x81bf1000
            };

            private static readonly uint[] maxCharsPerDigit = {0, 0, 0x1f, 20, 15, 13, 12, 11, 10, 10, 9, 9, 8, 8, 8, 8, 7, 7, 7, 7, 7, 7, 7, 7, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6};

            // Methods
            private static void AppendRadix(uint rem, uint radix, char[] tmp, StringBuilder buf, bool leadingZeros)
            {
                var length = tmp.Length;
                var startIndex = length;
                while (startIndex > 0 && (leadingZeros || rem != 0))
                {
                    var num3 = rem % radix;
                    rem /= radix;
                    tmp[--startIndex] = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"[(int) num3];
                }
                if (leadingZeros) buf.Append(tmp);
                else buf.Append(tmp, startIndex, length - startIndex);
            }

            internal static string RubyBignumToString(uint[] d, int sign, int radix)
            {
                var length = d.Length;
                if (length != 0)
                {
                    var list = new List<uint>();
                    var num2 = groupRadixValues[radix];
                    while (length > 0)
                    {
                        var item = div(d, ref length, num2);
                        list.Add(item);
                    }
                    var buf = new StringBuilder();
                    if (sign == -1) buf.Append("-");
                    var num4 = list.Count - 1;
                    var tmp = new char[maxCharsPerDigit[radix]];
                    AppendRadix(list[num4--], (uint) radix, tmp, buf, false);
                    while (num4 >= 0) AppendRadix(list[num4--], (uint) radix, tmp, buf, true);
                    if (buf.Length != 0) return buf.ToString();
                }
                return "0";
            }

            private static uint div(uint[] n, ref int nl, uint d)
            {
                ulong num = 0L;
                var index = nl;
                var flag = false;
                while (--index >= 0)
                {
                    num = num << 0x20;
                    num |= n[index];
                    var num3 = (uint) (num / (ulong) d);
                    n[index] = num3;
                    if (num3 == 0)
                    {
                        if (!flag) nl--;
                    }
                    else flag = true;
                    num = num % (ulong) d;
                }
                return (uint) num;
            }

            public static int FloorDivideUnchecked(int x, int y)
            {
                var num = x / y;
                if (x >= 0)
                {
                    if (y > 0) return num;
                    if (x % y == 0) return num;
                    return num - 1;
                }
                if (y <= 0) return num;
                if (x % y == 0) return num;
                return num - 1;
            }

            public static long FloorDivideUnchecked(long x, long y)
            {
                var num = x / y;
                if (x >= 0L)
                {
                    if (y > 0L) return num;
                    if (x % y == 0L) return num;
                    return num - 1L;
                }
                if (y <= 0L) return num;
                if (x % y == 0L) return num;
                return num - 1L;
            }

            public static int FloorRemainder(int x, int y)
            {
                if (y == -1) return 0;
                var num = x % y;
                if (x >= 0)
                {
                    if (y > 0) return num;
                    if (num == 0) return 0;
                    return num + y;
                }
                if (y <= 0) return num;
                if (num == 0) return 0;
                return num + y;
            }

            public static long FloorRemainder(long x, long y)
            {
                if (y == -1L) return 0L;
                var num = x % y;
                if (x >= 0L)
                {
                    if (y > 0L) return num;
                    if (num == 0L) return 0L;
                    return num + y;
                }
                if (y <= 0L) return num;
                if (num == 0L) return 0L;
                return num + y;
            }

            private static double GetPowerOf10(int precision)
            {
                if (precision >= 0x10) return Math.Pow(10.0, (double) precision);
                return _RoundPowersOfTens[precision];
            }

            private static uint GetWord(byte[] bytes, int start, int end)
            {
                uint num = 0;
                var num2 = end - start;
                var num3 = 0;
                if (num2 > 0x20) num2 = 0x20;
                start /= 8;
                while (num2 > 0)
                {
                    uint num4 = bytes[start];
                    if (num2 < 8) num4 &= (uint) (((int) 1 << num2) - 1);
                    num4 = num4 << num3;
                    num |= num4;
                    num2 -= 8;
                    num3 += 8;
                    start++;
                }
                return num;
            }

            public static double Hypot(double x, double y)
            {
                if (double.IsInfinity(x) || double.IsInfinity(y)) return double.PositiveInfinity;
                if (x < 0.0) x = -x;
                if (y < 0.0) y = -y;
                if (x == 0.0) return y;
                if (y == 0.0) return x;
                if (x < y)
                {
                    var num = y;
                    y = x;
                    x = num;
                }
                y /= x;
                return x * Math.Sqrt(1.0 + y * y);
            }

            public static bool IsNegativeZero(double self) { return self == 0.0 && 1.0 / self < 0.0; }

            public static double RoundAwayFromZero(double value) { return Math.Round(value, MidpointRounding.AwayFromZero); }

            public static double RoundAwayFromZero(double value, int precision)
            {
                if (precision >= 0)
                {
                    var num = GetPowerOf10(precision);
                    return RoundAwayFromZero(value * num) / num;
                }
                var num2 = GetPowerOf10(-precision);
                return RoundAwayFromZero(value / num2) * num2;
            }
        }
    }
}