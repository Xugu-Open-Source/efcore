// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

namespace EntityFrameworkCore.XuGu.Infrastructure
{
    public class XGDefaultDataTypeMappings
    {
        public XGDefaultDataTypeMappings()
        {
        }

        protected XGDefaultDataTypeMappings(XGDefaultDataTypeMappings copyFrom)
        {
            ClrBoolean = copyFrom.ClrBoolean;
            ClrDateTime = copyFrom.ClrDateTime;
            ClrDateTimeOffset = copyFrom.ClrDateTimeOffset;
            ClrTimeSpan = copyFrom.ClrTimeSpan;
        }

        public XGBooleanType ClrBoolean { get; private set; }
        public XGDateTimeType ClrDateTime { get; private set; }
        public XGDateTimeType ClrDateTimeOffset { get; private set; }
        public XGTimeSpanType ClrTimeSpan { get; private set; }

        public XGDefaultDataTypeMappings WithClrBoolean(XGBooleanType XGBooleanType)
        {
            var clone = Clone();
            clone.ClrBoolean = XGBooleanType;
            return clone;
        }

        public XGDefaultDataTypeMappings WithClrDateTime(XGDateTimeType XGDateTimeType)
        {
            var clone = Clone();
            clone.ClrDateTime = XGDateTimeType;
            return clone;
        }

        public XGDefaultDataTypeMappings WithClrDateTimeOffset(XGDateTimeType XGDateTimeType)
        {
            var clone = Clone();
            clone.ClrDateTimeOffset = XGDateTimeType;
            return clone;
        }

        public XGDefaultDataTypeMappings WithClrTimeSpan(XGTimeSpanType XGTimeSpanType)
        {
            var clone = Clone();
            clone.ClrTimeSpan = XGTimeSpanType;
            return clone;
        }

        protected virtual XGDefaultDataTypeMappings Clone() => new XGDefaultDataTypeMappings(this);

        protected bool Equals(XGDefaultDataTypeMappings other)
        {
            return ClrBoolean == other.ClrBoolean &&
                   ClrDateTime == other.ClrDateTime &&
                   ClrDateTimeOffset == other.ClrDateTimeOffset &&
                   ClrTimeSpan == other.ClrTimeSpan;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((XGDefaultDataTypeMappings)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)ClrBoolean;
                hashCode = (hashCode * 397) ^ (int)ClrDateTime;
                hashCode = (hashCode * 397) ^ (int)ClrDateTimeOffset;
                hashCode = (hashCode * 397) ^ (int)ClrTimeSpan;
                return hashCode;
            }
        }
    }

    public enum XGBooleanType
    {
        /// <summary>
        /// TODO
        /// </summary>
        None = -1, // TODO: Remove in EF Core 5; see XGTypeMappingTest.Bool_with_XGBooleanType_None_maps_to_null()

        /// <summary>
        /// TODO
        /// </summary>
        Default = 0,

        /// <summary>
        /// TODO
        /// </summary>
        TinyInt1 = 1,

        /// <summary>
        /// TODO
        /// </summary>
        Bit1 = 2
    }

    public enum XGDateTimeType
    {
        /// <summary>
        /// TODO
        /// </summary>
        Default = 0,

        /// <summary>
        /// TODO
        /// </summary>
        DateTime = 1,

        /// <summary>
        /// TODO
        /// </summary>
        DateTime6 = 2,

        /// <summary>
        /// TODO
        /// </summary>
        Timestamp6 = 3,

        /// <summary>
        /// TODO
        /// </summary>
        Timestamp = 4,
    }

    public enum XGTimeSpanType
    {
        /// <summary>
        /// TODO
        /// </summary>
        Default = 0,

        /// <summary>
        /// TODO
        /// </summary>
        Time = 1,

        /// <summary>
        /// TODO
        /// </summary>
        Time6 = 2,
    }
}
