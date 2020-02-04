//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Enums;

namespace Xarial.XCad.UI.PropertyPage.Attributes
{
    /// <summary>
    /// Provides additional options for number box control
    /// </summary>
    /// <remarks>Applied to all numeric properties (i.e. <see cref="double"/>, <see cref="int"/>)</remarks>
    public class NumberBoxOptionsAttribute : Attribute, IAttribute
    {
        public NumberBoxStyle_e Style { get; private set; }
        public NumberBoxUnitType_e Units { get; private set; } = 0;
        public double Minimum { get; private set; }
        public double Maximum { get; private set; }
        public double Increment { get; private set; }
        public double FastIncrement { get; private set; }
        public double SlowIncrement { get; private set; }
        public bool Inclusive { get; private set; }

        /// <summary>
        /// Constructor for specifying number box options
        /// </summary>
        /// <param name="style">Number box style</param>
        public NumberBoxOptionsAttribute(NumberBoxStyle_e style = NumberBoxStyle_e.None)
            : this(0, 0, 100, 5, true, 10, 1, style)
        {
        }

        /// <inheritdoc cref = "NumberBoxOptionsAttribute(NumberBoxStyle_e)" />
        /// <param name="units">Number box units</see>
        /// 0 for not using units. If units are specified corresponding current user unit system
        /// will be used and the corresponding units marks will be displayed in the number box.
        /// Regardless of the current unit system the value will be stored in system units (MKS)
        /// </param>
        /// <param name="minimum">Minimum allowed value for the number box</param>
        /// <param name="maximum">Maximum allowed value for the number box</param>
        /// <param name="increment">Default increment when up or down increment button is clicked</param>
        /// <param name="inclusive">True sets the minimum-maximum as inclusive, false sets it as exclusive</param>
        /// <param name="fastIncrement">Fast increment for mouse wheel or scroll</param>
        /// <param name="slowIncrement">Slow increment for mouse wheel or scroll</param>
        public NumberBoxOptionsAttribute(NumberBoxUnitType_e units,
            double minimum, double maximum, double increment, bool inclusive,
            double fastIncrement, double slowIncrement,
            NumberBoxStyle_e style = NumberBoxStyle_e.None)
        {
            Units = units;
            Minimum = minimum;
            Maximum = maximum;
            Increment = increment;
            Inclusive = inclusive;
            FastIncrement = fastIncrement;
            SlowIncrement = slowIncrement;
            Style = style;
        }
    }
}