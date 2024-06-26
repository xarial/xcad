//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xarial.XCad.Documents;

namespace Xarial.XCad.SolidWorks.Documents
{
    /// <summary>
    /// SOLIDWORKS specific units
    /// </summary>
    public interface ISwUnits : IXUnits
    {
    }

    [DebuggerDisplay("{" + nameof(Length) + "} - {" + nameof(Mass) + "} - {" + nameof(Angle) + "} - {" + nameof(Time) + "}")]
    internal class SwUnits : ISwUnits
    {
        public UnitSystem_e System 
        {
            get
            {
                var system = (swUnitSystem_e)m_Document.Model.Extension.GetUserPreferenceInteger(
                    (int)swUserPreferenceIntegerValue_e.swUnitSystem,
                    (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified);

                switch (system)
                {
                    case swUnitSystem_e.swUnitSystem_CGS:
                        return UnitSystem_e.CGS;

                    case swUnitSystem_e.swUnitSystem_IPS:
                        return UnitSystem_e.IPS;

                    case swUnitSystem_e.swUnitSystem_MKS:
                        return UnitSystem_e.MKS;

                    case swUnitSystem_e.swUnitSystem_MMGS:
                        return UnitSystem_e.MMGS;

                    case swUnitSystem_e.swUnitSystem_Custom:
                        return UnitSystem_e.Custom;

                    default:
                        throw new NotSupportedException();
                }
            }
            set 
            {
                swUnitSystem_e system;

                switch (value) 
                {
                    case UnitSystem_e.MKS:
                        system = swUnitSystem_e.swUnitSystem_MKS;
                        break;

                    case UnitSystem_e.CGS:
                        system = swUnitSystem_e.swUnitSystem_CGS;
                        break;

                    case UnitSystem_e.MMGS:
                        system = swUnitSystem_e.swUnitSystem_MMGS;
                        break;

                    case UnitSystem_e.IPS:
                        system = swUnitSystem_e.swUnitSystem_IPS;
                        break;

                    case UnitSystem_e.Custom:
                        system = swUnitSystem_e.swUnitSystem_Custom;
                        break;

                    default:
                        throw new NotSupportedException();
                }

                if (!m_Document.Model.Extension.SetUserPreferenceInteger(
                    (int)swUserPreferenceIntegerValue_e.swUnitSystem,
                    (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified, (int)system)) 
                {
                    throw new Exception("Failed to change unit system");
                }
            }
        }

        public Length_e Length
        {
            get
            {
                var units = (swUnitSystem_e)m_Document.Model.Extension.GetUserPreferenceInteger(
                    (int)swUserPreferenceIntegerValue_e.swUnitSystem,
                    (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified);

                switch (units)
                {
                    case swUnitSystem_e.swUnitSystem_CGS:
                        return Length_e.Centimeters;

                    case swUnitSystem_e.swUnitSystem_IPS:
                        return Length_e.Inches;

                    case swUnitSystem_e.swUnitSystem_MKS:
                        return Length_e.Meters;

                    case swUnitSystem_e.swUnitSystem_MMGS:
                        return Length_e.Millimeters;

                    case swUnitSystem_e.swUnitSystem_Custom:

                        var lengthUnits = (swLengthUnit_e)m_Document.Model.Extension.GetUserPreferenceInteger(
                            (int)swUserPreferenceIntegerValue_e.swUnitsLinear, (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified);
                        
                        return ConvertLengthUnits(lengthUnits);

                    default:
                        throw new Exception($"Units are not supported: {units}");
                }
            }
            set 
            {
                if (System == UnitSystem_e.Custom)
                {
                    var lengthUnit = ConvertToLengthUnit(value);

                    if (!m_Document.Model.Extension.SetUserPreferenceInteger(
                           (int)swUserPreferenceIntegerValue_e.swUnitsLinear,
                           (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified, (int)lengthUnit))
                    {
                        throw new Exception("Failed to change length units");
                    }
                }
                else 
                {
                    throw new NotSupportedException("Units can only be changed in Custom units system");
                }
            }
        }

        public Length_e DualDimensionLength 
        {
            get 
            {
                var lengthUnits = (swLengthUnit_e)m_Document.Model.Extension.GetUserPreferenceInteger(
                            (int)swUserPreferenceIntegerValue_e.swUnitsDualLinear, (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified);

                return ConvertLengthUnits(lengthUnits);
            }
            set 
            {
                var lengthUnit = ConvertToLengthUnit(value);

                if (!m_Document.Model.Extension.SetUserPreferenceInteger(
                       (int)swUserPreferenceIntegerValue_e.swUnitsDualLinear,
                       (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified, (int)lengthUnit))
                {
                    throw new Exception("Failed to change dual dimension length units");
                }
            }
        }

        public Mass_e Mass
        {
            get
            {
                var units = (swUnitSystem_e)m_Document.Model.Extension.GetUserPreferenceInteger(
                    (int)swUserPreferenceIntegerValue_e.swUnitSystem,
                    (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified);

                switch (units)
                {
                    case swUnitSystem_e.swUnitSystem_CGS:
                        return Mass_e.Grams;

                    case swUnitSystem_e.swUnitSystem_IPS:
                        return Mass_e.Pounds;

                    case swUnitSystem_e.swUnitSystem_MKS:
                        return Mass_e.Kilograms;

                    case swUnitSystem_e.swUnitSystem_MMGS:
                        return Mass_e.Grams;

                    case swUnitSystem_e.swUnitSystem_Custom:

                        var massUnits = (swUnitsMassPropMass_e)m_Document.Model.Extension.GetUserPreferenceInteger(
                            (int)swUserPreferenceIntegerValue_e.swUnitsMassPropMass, (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified);

                        switch (massUnits)
                        {
                            case swUnitsMassPropMass_e.swUnitsMassPropMass_Grams:
                                return Mass_e.Grams;

                            case swUnitsMassPropMass_e.swUnitsMassPropMass_Kilograms:
                                return Mass_e.Kilograms;

                            case swUnitsMassPropMass_e.swUnitsMassPropMass_Milligrams:
                                return Mass_e.Milligrams;

                            case swUnitsMassPropMass_e.swUnitsMassPropMass_Pounds:
                                return Mass_e.Pounds;

                            default:
                                throw new Exception($"Specified custom mass unit is not supported: {massUnits}");
                        }

                    default:
                        throw new Exception($"Units are not supported: {units}");
                }
            }
            set
            {
                if (System == UnitSystem_e.Custom)
                {
                    swUnitsMassPropMass_e massUnit;

                    switch (value)
                    {
                        case Mass_e.Grams:
                            massUnit = swUnitsMassPropMass_e.swUnitsMassPropMass_Grams;
                            break;

                        case Mass_e.Kilograms:
                            massUnit = swUnitsMassPropMass_e.swUnitsMassPropMass_Kilograms;
                            break;

                        case Mass_e.Milligrams:
                            massUnit = swUnitsMassPropMass_e.swUnitsMassPropMass_Milligrams;
                            break;

                        case Mass_e.Pounds:
                            massUnit = swUnitsMassPropMass_e.swUnitsMassPropMass_Pounds;
                            break;

                        default:
                            throw new NotSupportedException();
                    }

                    if (!m_Document.Model.Extension.SetUserPreferenceInteger(
                           (int)swUserPreferenceIntegerValue_e.swUnitsMassPropMass,
                           (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified, (int)massUnit)) 
                    {
                        throw new Exception("Failed to change mass units");
                    }
                }
                else
                {
                    throw new NotSupportedException("Units can only be changed in Custom units system");
                }
            }
        }

        public Angle_e Angle
        {
            get
            {
                var angularUnits = (swAngleUnit_e)m_Document.Model.Extension.GetUserPreferenceInteger(
                            (int)swUserPreferenceIntegerValue_e.swUnitsAngular, (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified);

                switch (angularUnits)
                {
                    case swAngleUnit_e.swDEGREES:
                        return Angle_e.Degrees;
                    
                    case swAngleUnit_e.swDEG_MIN:
                        return Angle_e.DegreesMinutes;

                    case swAngleUnit_e.swDEG_MIN_SEC:
                        return Angle_e.DegreesMinutesSeconds;

                    case swAngleUnit_e.swRADIANS:
                        return Angle_e.Radians;

                    default:
                        throw new Exception($"Specified custom angular unit is not supported: {angularUnits}");
                }
            }
            set
            {
                swAngleUnit_e angularUnits;

                switch (value)
                {
                    case Angle_e.Degrees:
                        angularUnits = swAngleUnit_e.swDEGREES;
                        break;

                    case Angle_e.DegreesMinutes:
                        angularUnits = swAngleUnit_e.swDEG_MIN;
                        break;

                    case Angle_e.DegreesMinutesSeconds:
                        angularUnits = swAngleUnit_e.swDEG_MIN_SEC;
                        break;

                    case Angle_e.Radians:
                        angularUnits = swAngleUnit_e.swRADIANS;
                        break;

                    default:
                        throw new NotSupportedException();
                }

                if (!m_Document.Model.Extension.SetUserPreferenceInteger(
                       (int)swUserPreferenceIntegerValue_e.swUnitsAngular,
                       (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified, (int)angularUnits)) 
                {
                    throw new Exception("Failed to change angle units");
                }
            }
        }

        public Time_e Time
        {
            get
            {
                var units = (swUnitSystem_e)m_Document.Model.Extension.GetUserPreferenceInteger(
                    (int)swUserPreferenceIntegerValue_e.swUnitSystem,
                    (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified);

                switch (units)
                {
                    case swUnitSystem_e.swUnitSystem_CGS:
                    case swUnitSystem_e.swUnitSystem_IPS:
                    case swUnitSystem_e.swUnitSystem_MKS:
                    case swUnitSystem_e.swUnitSystem_MMGS:
                        return Time_e.Seconds;

                    case swUnitSystem_e.swUnitSystem_Custom:

                        var timeUnits = (swUnitsTimeUnit_e)m_Document.Model.Extension.GetUserPreferenceInteger(
                            (int)swUserPreferenceIntegerValue_e.swUnitsTimeUnits, (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified);

                        switch (timeUnits)
                        {
                            case swUnitsTimeUnit_e.swUnitsTimeUnit_Hour:
                                return Time_e.Hours;

                            case swUnitsTimeUnit_e.swUnitsTimeUnit_Microsecond:
                                return Time_e.Microseconds;

                            case swUnitsTimeUnit_e.swUnitsTimeUnit_Millisecond:
                                return Time_e.Milliseconds;

                            case swUnitsTimeUnit_e.swUnitsTimeUnit_Minute:
                                return Time_e.Minutes;

                            case swUnitsTimeUnit_e.swUnitsTimeUnit_Nanosecond:
                                return Time_e.Nanoseconds;

                            case swUnitsTimeUnit_e.swUnitsTimeUnit_Second:
                                return Time_e.Seconds;

                            default:
                                throw new Exception($"Specified custom time unit is not supported: {timeUnits}");
                        }

                    default:
                        throw new Exception($"Units are not supported: {units}");
                }
            }
            set
            {
                if (System == UnitSystem_e.Custom)
                {
                    swUnitsTimeUnit_e timeUnit;

                    switch (value)
                    {
                        case Time_e.Hours:
                            timeUnit = swUnitsTimeUnit_e.swUnitsTimeUnit_Hour;
                            break;

                        case Time_e.Microseconds:
                            timeUnit = swUnitsTimeUnit_e.swUnitsTimeUnit_Microsecond;
                            break;

                        case Time_e.Milliseconds:
                            timeUnit = swUnitsTimeUnit_e.swUnitsTimeUnit_Millisecond;
                            break;

                        case Time_e.Minutes:
                            timeUnit = swUnitsTimeUnit_e.swUnitsTimeUnit_Minute;
                            break;

                        case Time_e.Nanoseconds:
                            timeUnit = swUnitsTimeUnit_e.swUnitsTimeUnit_Nanosecond;
                            break;

                        case Time_e.Seconds:
                            timeUnit = swUnitsTimeUnit_e.swUnitsTimeUnit_Second;
                            break;

                        default:
                            throw new NotSupportedException();
                    }

                    if (!m_Document.Model.Extension.SetUserPreferenceInteger(
                           (int)swUserPreferenceIntegerValue_e.swUnitsTimeUnits,
                           (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified, (int)timeUnit)) 
                    {
                        throw new Exception("Failed to change angle units");
                    }
                }
                else
                {
                    throw new NotSupportedException("Units can only be changed in Custom units system");
                }
            }
        }

        public int? LengthDecimalPlaces
        {
            get
            {
                if (m_Document.Model.Extension.GetUserPreferenceInteger(
                    (int)swUserPreferenceIntegerValue_e.swUnitsLinearDecimalDisplay,
                    (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified) == (int)swFractionDisplay_e.swDECIMAL)
                {
                    return m_Document.Model.Extension.GetUserPreferenceInteger(
                        (int)swUserPreferenceIntegerValue_e.swUnitsLinearDecimalPlaces,
                        (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified);
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value.HasValue)
                {
                    if (m_Document.Model.Extension.SetUserPreferenceInteger(
                        (int)swUserPreferenceIntegerValue_e.swUnitsLinearDecimalDisplay,
                        (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified, (int)swFractionDisplay_e.swDECIMAL))
                    {
                        if (!m_Document.Model.Extension.SetUserPreferenceInteger(
                            (int)swUserPreferenceIntegerValue_e.swUnitsLinearDecimalPlaces,
                            (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified, value.Value))
                        {
                            throw new Exception("Failed to change length decimal places");
                        }
                    }
                    else
                    {
                        throw new Exception("Failed to change length display");
                    }
                }
                else
                {
                    if (m_Document.Model.Extension.SetUserPreferenceInteger(
                        (int)swUserPreferenceIntegerValue_e.swUnitsLinearDecimalDisplay,
                        (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified, (int)swFractionDisplay_e.swNONE))
                    {
                        throw new Exception("Failed to set length display to none");
                    }
                }
            }
        }

        public int? DualDimensionLengthDecimalPlaces
        {
            get
            {
                if (m_Document.Model.Extension.GetUserPreferenceInteger(
                    (int)swUserPreferenceIntegerValue_e.swUnitsDualLinearDecimalDisplay,
                    (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified) == (int)swFractionDisplay_e.swDECIMAL)
                {
                    return m_Document.Model.Extension.GetUserPreferenceInteger(
                        (int)swUserPreferenceIntegerValue_e.swUnitsDualLinearDecimalPlaces,
                        (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified);
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value.HasValue)
                {
                    if (m_Document.Model.Extension.SetUserPreferenceInteger(
                        (int)swUserPreferenceIntegerValue_e.swUnitsDualLinearDecimalDisplay,
                        (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified, (int)swFractionDisplay_e.swDECIMAL))
                    {
                        if (!m_Document.Model.Extension.SetUserPreferenceInteger(
                            (int)swUserPreferenceIntegerValue_e.swUnitsDualLinearDecimalPlaces,
                            (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified, value.Value))
                        {
                            throw new Exception("Failed to change dual dimension length decimal places");
                        }
                    }
                    else
                    {
                        throw new Exception("Failed to change dual dimension length display");
                    }
                }
                else
                {
                    if (m_Document.Model.Extension.SetUserPreferenceInteger(
                        (int)swUserPreferenceIntegerValue_e.swUnitsDualLinearDecimalDisplay,
                        (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified, (int)swFractionDisplay_e.swNONE))
                    {
                        throw new Exception("Failed to set dual dimension length display to none");
                    }
                }
            }
        }
        
        public int MassDecimalPlaces
        {
            get => m_Document.Model.Extension.GetUserPreferenceInteger(
                (int)swUserPreferenceIntegerValue_e.swUnitsMassPropDecimalPlaces,
                (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified);
            set 
            {
                if (m_Document.Model.Extension.SetUserPreferenceInteger(
                    (int)swUserPreferenceIntegerValue_e.swUnitsMassPropDecimalPlaces,
                    (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified, value)) 
                {
                    throw new Exception("Failed to change mass decimal places");
                }
            }
        }

        public int AngleDecimalPlaces
        {
            get => m_Document.Model.Extension.GetUserPreferenceInteger(
                (int)swUserPreferenceIntegerValue_e.swUnitsAngularDecimalPlaces,
                (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified);
            set 
            {
                if (!m_Document.Model.Extension.SetUserPreferenceInteger(
                    (int)swUserPreferenceIntegerValue_e.swUnitsAngularDecimalPlaces,
                    (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified, value)) 
                {
                    throw new Exception("Failed to change angle decimal places");
                }
            }
        }

        public int TimeDecimalPlaces
        {
            get => m_Document.Model.Extension.GetUserPreferenceInteger(
                (int)swUserPreferenceIntegerValue_e.swUnitsTimeDecimalPlaces,
                (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified);
            set 
            {
                if (!m_Document.Model.Extension.SetUserPreferenceInteger(
                    (int)swUserPreferenceIntegerValue_e.swUnitsTimeDecimalPlaces,
                    (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified, value)) 
                {
                    throw new Exception("Failed to change time decimal places");
                }
            }
        }

        public int? LengthFractions
        {
            get
            {
                if (m_Document.Model.Extension.GetUserPreferenceInteger(
                    (int)swUserPreferenceIntegerValue_e.swUnitsLinearDecimalDisplay,
                    (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified) == (int)swFractionDisplay_e.swFRACTION)
                {
                    return m_Document.Model.Extension.GetUserPreferenceInteger(
                    (int)swUserPreferenceIntegerValue_e.swUnitsLinearFractionDenominator,
                    (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified);
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value.HasValue)
                {
                    if (m_Document.Model.Extension.SetUserPreferenceInteger(
                        (int)swUserPreferenceIntegerValue_e.swUnitsLinearDecimalDisplay,
                        (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified, (int)swFractionDisplay_e.swFRACTION))
                    {
                        if (!m_Document.Model.Extension.SetUserPreferenceInteger(
                            (int)swUserPreferenceIntegerValue_e.swUnitsLinearFractionDenominator,
                            (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified, value.Value))
                        {
                            throw new Exception("Failed to change length fractions");
                        }
                    }
                    else
                    {
                        throw new Exception("Failed to change length display");
                    }
                }
                else
                {
                    if (m_Document.Model.Extension.SetUserPreferenceInteger(
                        (int)swUserPreferenceIntegerValue_e.swUnitsLinearDecimalDisplay,
                        (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified, (int)swFractionDisplay_e.swNONE))
                    {
                        throw new Exception("Failed to set length display to none");
                    }
                }
            }
        }

        public bool? LengthRoundToNearestFraction
        {
            get
            {
                if (m_Document.Model.Extension.GetUserPreferenceInteger(
                    (int)swUserPreferenceIntegerValue_e.swUnitsLinearDecimalDisplay,
                    (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified) == (int)swFractionDisplay_e.swFRACTION)
                {
                    return m_Document.Model.Extension.GetUserPreferenceToggle(
                        (int)swUserPreferenceToggle_e.swUnitsLinearRoundToNearestFraction,
                        (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified);
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value.HasValue)
                {
                    if (m_Document.Model.Extension.SetUserPreferenceInteger(
                        (int)swUserPreferenceIntegerValue_e.swUnitsLinearDecimalDisplay,
                        (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified, (int)swFractionDisplay_e.swFRACTION))
                    {
                        if (!m_Document.Model.Extension.SetUserPreferenceToggle(
                            (int)swUserPreferenceToggle_e.swUnitsLinearRoundToNearestFraction,
                            (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified, value.Value))
                        {
                            throw new Exception("Failed to change length round to nearest fraction");
                        }
                    }
                    else
                    {
                        throw new Exception("Failed to change length round to nearest fraction");
                    }
                }
                else
                {
                    throw new NotSupportedException("Failed to set length round to nearest fraction");
                }
            }
        }

        public bool? LengthConvertFeetAndInchesFormat
        {
            get
            {
                if (m_Document.Model.Extension.GetUserPreferenceInteger(
                    (int)swUserPreferenceIntegerValue_e.swUnitsLinear,
                    (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified) == (int)swLengthUnit_e.swFEETINCHES)
                {
                    return m_Document.Model.Extension.GetUserPreferenceToggle(
                        (int)swUserPreferenceToggle_e.swUnitsLinearFeetAndInchesFormat,
                        (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified);
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value.HasValue)
                {
                    if (m_Document.Model.Extension.GetUserPreferenceInteger(
                        (int)swUserPreferenceIntegerValue_e.swUnitsLinear,
                        (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified) == (int)swLengthUnit_e.swFEETINCHES)
                    {
                        if (!m_Document.Model.Extension.SetUserPreferenceToggle(
                            (int)swUserPreferenceToggle_e.swUnitsLinearFeetAndInchesFormat,
                            (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified, value.Value))
                        {
                            throw new Exception("Failed to change length convert feet & inches format");
                        }
                    }
                    else
                    {
                        throw new Exception("Failed to change length convert feet & inches format");
                    }
                }
                else
                {
                    throw new NotSupportedException("Failed to set length convert feet & inches format");
                }
            }
        }

        public int? DualDimensionLengthFractions
        {
            get
            {
                if (m_Document.Model.Extension.GetUserPreferenceInteger(
                    (int)swUserPreferenceIntegerValue_e.swUnitsDualLinearDecimalDisplay,
                    (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified) == (int)swFractionDisplay_e.swFRACTION)
                {
                    return m_Document.Model.Extension.GetUserPreferenceInteger(
                        (int)swUserPreferenceIntegerValue_e.swUnitsDualLinearFractionDenominator,
                        (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified);
                }
                else 
                {
                    return null;
                }
            }
            set
            {
                if (value.HasValue)
                {
                    if (m_Document.Model.Extension.SetUserPreferenceInteger(
                        (int)swUserPreferenceIntegerValue_e.swUnitsDualLinearDecimalDisplay,
                        (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified, (int)swFractionDisplay_e.swFRACTION))
                    {
                        if (!m_Document.Model.Extension.SetUserPreferenceInteger(
                            (int)swUserPreferenceIntegerValue_e.swUnitsDualLinearFractionDenominator,
                            (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified, value.Value))
                        {
                            throw new Exception("Failed to change dual dimension length fractions");
                        }
                    }
                    else
                    {
                        throw new Exception("Failed to change dual dimension length display");
                    }
                }
                else 
                {
                    if (m_Document.Model.Extension.SetUserPreferenceInteger(
                        (int)swUserPreferenceIntegerValue_e.swUnitsDualLinearDecimalDisplay,
                        (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified, (int)swFractionDisplay_e.swNONE))
                    {
                        throw new Exception("Failed to set dual dimension length display to none");
                    }
                }
            }
        }

        public bool? DualDimensionLengthRoundToNearestFraction
        {
            get
            {
                if (m_Document.Model.Extension.GetUserPreferenceInteger(
                    (int)swUserPreferenceIntegerValue_e.swUnitsDualLinearDecimalDisplay,
                    (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified) == (int)swFractionDisplay_e.swFRACTION)
                {
                    return m_Document.Model.Extension.GetUserPreferenceToggle(
                        (int)swUserPreferenceToggle_e.swUnitsDualLinearRoundToNearestFraction,
                        (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified);
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value.HasValue)
                {
                    if (m_Document.Model.Extension.SetUserPreferenceInteger(
                        (int)swUserPreferenceIntegerValue_e.swUnitsDualLinearDecimalDisplay,
                        (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified, (int)swFractionDisplay_e.swFRACTION))
                    {
                        if (!m_Document.Model.Extension.SetUserPreferenceToggle(
                            (int)swUserPreferenceToggle_e.swUnitsDualLinearRoundToNearestFraction,
                            (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified, value.Value))
                        {
                            throw new Exception("Failed to change dual dimension length round to nearest fraction");
                        }
                    }
                    else
                    {
                        throw new Exception("Failed to change dual dimension length round to nearest fraction");
                    }
                }
                else
                {
                    throw new NotSupportedException("Failed to set dual dimension length round to nearest fraction");
                }
            }
        }

        public bool? DualDimensionLengthConvertFeetAndInchesFormat
        {
            get
            {
                if (m_Document.Model.Extension.GetUserPreferenceInteger(
                    (int)swUserPreferenceIntegerValue_e.swUnitsDualLinear,
                    (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified) == (int)swLengthUnit_e.swFEETINCHES)
                {
                    return m_Document.Model.Extension.GetUserPreferenceToggle(
                        (int)swUserPreferenceToggle_e.swUnitsDualLinearFeetAndInchesFormat,
                        (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified);
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value.HasValue)
                {
                    if (m_Document.Model.Extension.GetUserPreferenceInteger(
                        (int)swUserPreferenceIntegerValue_e.swUnitsDualLinear,
                        (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified) == (int)swLengthUnit_e.swFEETINCHES)
                    {
                        if (!m_Document.Model.Extension.SetUserPreferenceToggle(
                            (int)swUserPreferenceToggle_e.swUnitsDualLinearFeetAndInchesFormat,
                            (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified, value.Value))
                        {
                            throw new Exception("Failed to change dual dimension length convert feet & inches format");
                        }
                    }
                    else
                    {
                        throw new Exception("Failed to change dual dimension length convert feet & inches format");
                    }
                }
                else
                {
                    throw new NotSupportedException("Failed to set dual dimension length convert feet & inches format");
                }
            }
        }

        private readonly ISwDocument m_Document;

        internal SwUnits(ISwDocument document) 
        {
            m_Document = document;
        }

        private swLengthUnit_e ConvertToLengthUnit(Length_e value)
        {
            switch (value)
            {
                case Length_e.Angstroms:
                    return swLengthUnit_e.swANGSTROM;
                case Length_e.Centimeters:
                    return swLengthUnit_e.swCM;
                case Length_e.Feet:
                    return swLengthUnit_e.swFEET;
                case Length_e.FeetInches:
                    return swLengthUnit_e.swFEETINCHES;
                case Length_e.Inches:
                    return swLengthUnit_e.swINCHES;
                case Length_e.Meters:
                    return swLengthUnit_e.swMETER;
                case Length_e.Microns:
                    return swLengthUnit_e.swMICRON;
                case Length_e.Mils:
                    return swLengthUnit_e.swMIL;
                case Length_e.Millimeters:
                    return swLengthUnit_e.swMM;
                case Length_e.Nanometers:
                    return swLengthUnit_e.swNANOMETER;
                case Length_e.Microinches:
                    return swLengthUnit_e.swUIN;
                default:
                    throw new NotSupportedException();
            }
        }

        private Length_e ConvertLengthUnits(swLengthUnit_e lengthUnits)
        {
            switch (lengthUnits)
            {
                case swLengthUnit_e.swANGSTROM:
                    return Length_e.Angstroms;
                case swLengthUnit_e.swCM:
                    return Length_e.Centimeters;
                case swLengthUnit_e.swFEET:
                    return Length_e.Feet;
                case swLengthUnit_e.swFEETINCHES:
                    return Length_e.FeetInches;
                case swLengthUnit_e.swINCHES:
                    return Length_e.Inches;
                case swLengthUnit_e.swMETER:
                    return Length_e.Meters;
                case swLengthUnit_e.swMICRON:
                    return Length_e.Microns;
                case swLengthUnit_e.swMIL:
                    return Length_e.Mils;
                case swLengthUnit_e.swMM:
                    return Length_e.Millimeters;
                case swLengthUnit_e.swNANOMETER:
                    return Length_e.Nanometers;
                case swLengthUnit_e.swUIN:
                    return Length_e.Microinches;
                default:
                    throw new Exception($"Specified custom length unit is not supported: {lengthUnits}");
            }
        }
    }
}
