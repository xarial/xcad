//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Documents;

namespace Xarial.XCad.SolidWorks.Documents
{
    public interface ISwUnits : IXUnits
    {
    }

    internal class SwUnits : ISwUnits
    {
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

                        switch (lengthUnits)
                        {
                            case swLengthUnit_e.swANGSTROM:
                                return Length_e.Angstroms;
                            case swLengthUnit_e.swCM:
                                return Length_e.Centimeters;
                            case swLengthUnit_e.swFEET:
                            case swLengthUnit_e.swFEETINCHES:
                                return Length_e.Feet;
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

                    default:
                        throw new Exception($"Units are not supported: {units}");
                }
            }
            set => throw new NotImplementedException();
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
            set => throw new NotImplementedException();
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
                    case swAngleUnit_e.swDEG_MIN:
                    case swAngleUnit_e.swDEG_MIN_SEC:
                        return Angle_e.Degrees;

                    case swAngleUnit_e.swRADIANS:
                        return Angle_e.Radians;

                    default:
                        throw new Exception($"Specified custom angular unit is not supported: {angularUnits}");
                }
            }
            set => throw new NotImplementedException();
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
            set => throw new NotImplementedException();
        }

        private readonly ISwDocument m_Document;

        internal SwUnits(ISwDocument document) 
        {
            m_Document = document;
        }
    }
}
