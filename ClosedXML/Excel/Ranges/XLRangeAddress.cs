using ClosedXML.Extensions;
using System;
using System.Diagnostics;
using System.Linq;

namespace ClosedXML.Excel
{
    internal class XLRangeAddress : IXLRangeAddress
    {
        #region Private fields

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private XLAddress _firstAddress;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private XLAddress _lastAddress;

        #endregion Private fields

        #region Constructor

        public XLRangeAddress(XLRangeAddress rangeAddress) : this(rangeAddress.FirstAddress, rangeAddress.LastAddress)
        {
        }

        public XLRangeAddress(XLAddress firstAddress, XLAddress lastAddress)
        {
            Worksheet = firstAddress.Worksheet;
            FirstAddress = XLAddress.Create(firstAddress);
            LastAddress = XLAddress.Create(lastAddress);
        }

        public XLRangeAddress(XLWorksheet worksheet, String rangeAddress)
        {
            string addressToUse = rangeAddress.Contains("!")
                                      ? rangeAddress.Substring(rangeAddress.IndexOf("!") + 1)
                                      : rangeAddress;

            string firstPart;
            string secondPart;
            if (addressToUse.Contains(':'))
            {
                var arrRange = addressToUse.Split(':');
                firstPart = arrRange[0];
                secondPart = arrRange[1];
            }
            else
            {
                firstPart = addressToUse;
                secondPart = addressToUse;
            }

            if (XLHelper.IsValidA1Address(firstPart))
            {
                FirstAddress = XLAddress.Create(worksheet, firstPart);
                LastAddress = XLAddress.Create(worksheet, secondPart);
            }
            else
            {
                firstPart = firstPart.Replace("$", String.Empty);
                secondPart = secondPart.Replace("$", String.Empty);
                if (char.IsDigit(firstPart[0]))
                {
                    FirstAddress = XLAddress.Create(worksheet, "A" + firstPart);
                    LastAddress = XLAddress.Create(worksheet, XLHelper.MaxColumnLetter + secondPart);
                }
                else
                {
                    FirstAddress = XLAddress.Create(worksheet, firstPart + "1");
                    LastAddress = XLAddress.Create(worksheet, secondPart + XLHelper.MaxRowNumber.ToInvariantString());
                }
            }

            Worksheet = worksheet;
        }

        #endregion Constructor

        #region Public properties

        public XLWorksheet Worksheet { get; internal set; }

        public XLAddress FirstAddress
        {
            get
            {
                if (!IsValid)
                    throw new InvalidOperationException("Range is invalid.");

                return _firstAddress;
            }
            set { _firstAddress = value; }
        }

        public XLAddress LastAddress
        {
            get
            {
                if (!IsValid)
                    throw new InvalidOperationException("Range is an invalid state.");

                return _lastAddress;
            }
            set { _lastAddress = value; }
        }

        IXLWorksheet IXLRangeAddress.Worksheet
        {
            get { return Worksheet; }
        }

        IXLAddress IXLRangeAddress.FirstAddress
        {
            [DebuggerStepThrough]
            get { return FirstAddress; }
            set { FirstAddress = value as XLAddress; }
        }

        IXLAddress IXLRangeAddress.LastAddress
        {
            [DebuggerStepThrough]
            get { return LastAddress; }
            set { LastAddress = value as XLAddress; }
        }

        public bool IsValid { get; set; } = true;

        #endregion Public properties

        #region Public methods

        public String ToStringRelative()
        {
            return ToStringRelative(false);
        }

        public String ToStringFixed()
        {
            return ToStringFixed(XLReferenceStyle.A1);
        }

        public String ToStringRelative(Boolean includeSheet)
        {
            if (includeSheet)
                return String.Concat(
                    Worksheet.Name.EscapeSheetName(),
                    '!',
                    _firstAddress.ToStringRelative(),
                    ':',
                    _lastAddress.ToStringRelative());
            else
                return string.Concat(
                    _firstAddress.ToStringRelative(),
                    ":",
                    _lastAddress.ToStringRelative());
        }

        public String ToStringFixed(XLReferenceStyle referenceStyle)
        {
            return ToStringFixed(referenceStyle, false);
        }

        public String ToStringFixed(XLReferenceStyle referenceStyle, Boolean includeSheet)
        {
            if (includeSheet)
                return String.Format("{0}!{1}:{2}",
                    Worksheet.Name.EscapeSheetName(),
                    _firstAddress.ToStringFixed(referenceStyle),
                    _lastAddress.ToStringFixed(referenceStyle));

            return _firstAddress.ToStringFixed(referenceStyle) + ":" + _lastAddress.ToStringFixed(referenceStyle);
        }

        public override string ToString()
        {
            return String.Concat(_firstAddress, ':', _lastAddress);
        }

        public string ToString(XLReferenceStyle referenceStyle)
        {
            return ToString(referenceStyle, false);
        }

        public string ToString(XLReferenceStyle referenceStyle, bool includeSheet)
        {
            if (referenceStyle == XLReferenceStyle.R1C1)
                return ToStringFixed(referenceStyle, true);
            else
                return ToStringRelative(includeSheet);
        }

        public override bool Equals(object obj)
        {
            var other = (XLRangeAddress)obj;
            return Worksheet.Equals(other.Worksheet)
                   && FirstAddress.Equals(other.FirstAddress)
                   && LastAddress.Equals(other.LastAddress);
        }

        public override int GetHashCode()
        {
            return
                Worksheet.GetHashCode()
                ^ FirstAddress.GetHashCode()
                ^ LastAddress.GetHashCode();
        }

        #endregion Public methods
    }
}
