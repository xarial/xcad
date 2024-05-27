//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Document unit system
    /// </summary>
    public enum UnitSystem_e 
    {
        /// <summary>
        /// Meter, kilogram, second
        /// </summary>
        MKS,

        /// <summary>
        /// Centimeter, gram, second
        /// </summary>
        CGS,

        /// <summary>
        /// Millimeter, gram, second
        /// </summary>
        MMGS,

        /// <summary>
        /// Inch, pound, second
        /// </summary>
        IPS,

        /// <summary>
        /// Custom units
        /// </summary>
        Custom
    }

    /// <summary>
    /// Represents length unit
    /// </summary>
    public enum Length_e 
    {
        /// <summary>
        /// Å
        /// </summary>
        Angstroms,

        /// <summary>
        /// nm
        /// </summary>
        Nanometers,

        /// <summary>
        /// μ
        /// </summary>
        Microns,

        /// <summary>
        /// mm
        /// </summary>
        Millimeters,

        /// <summary>
        /// cm
        /// </summary>
        Centimeters,

        /// <summary>
        /// m
        /// </summary>
        Meters,

        /// <summary>
        /// µ"
        /// </summary>
        Microinches,

        /// <summary>
        /// mil
        /// </summary>
        Mils,

        /// <summary>
        /// "
        /// </summary>
        Inches,

        /// <summary>
        /// ft
        /// </summary>
        Feet,

        /// <summary>
        /// ft "
        /// </summary>
        FeetInches
    }

    /// <summary>
    /// Represents mass units
    /// </summary>
    public enum Mass_e
    {
        /// <summary>
        /// mg
        /// </summary>
        Milligrams,

        /// <summary>
        /// g
        /// </summary>
        Grams,

        /// <summary>
        /// kg
        /// </summary>
        Kilograms,

        /// <summary>
        /// lb
        /// </summary>
        Pounds,
    }
    
    /// <summary>
    /// Represents angle units
    /// </summary>
    public enum Angle_e
    {
        /// <summary>
        /// °
        /// </summary>
        Degrees,
        
        /// <summary>
        /// rad
        /// </summary>
        Radians,

        /// <summary>
        /// ° '
        /// </summary>
        DegreesMinutes,

        /// <summary>
        /// ° ' "
        /// </summary>
        DegreesMinutesSeconds,
    }

    /// <summary>
    /// Represents time units
    /// </summary>
    public enum Time_e
    {
        /// <summary>
        /// sec
        /// </summary>
        Seconds,

        /// <summary>
        /// msec
        /// </summary>
        Milliseconds,

        /// <summary>
        /// µsec
        /// </summary>
        Microseconds,

        /// <summary>
        /// nsec
        /// </summary>
        Nanoseconds,

        /// <summary>
        /// min
        /// </summary>
        Minutes,

        /// <summary>
        /// hr
        /// </summary>
        Hours
    }

    /// <summary>
    /// Represents the units system of the document
    /// </summary>
    public interface IXUnits
    {
        /// <summary>
        /// Units system
        /// </summary>
        UnitSystem_e System { get; set; }

        /// <summary>
        /// Acessing length units
        /// </summary>
        Length_e Length { get; set; }

        /// <summary>
        /// Acessing mass units
        /// </summary>
        Mass_e Mass { get; set; }

        /// <summary>
        /// Acessing angle units
        /// </summary>
        Angle_e Angle { get; set; }

        /// <summary>
        /// Acessing time units
        /// </summary>
        Time_e Time { get; set; }

        /// <summary>
        /// Decimal places of the length
        /// </summary>
        int LengthDecimalPlaces { get; set; }

        /// <summary>
        /// Decimal places of the mass
        /// </summary>
        int MassDecimalPlaces { get; set; }

        /// <summary>
        /// Decimal places of the angle
        /// </summary>
        int AngleDecimalPlaces { get; set; }

        /// <summary>
        /// Decimal places of the time
        /// </summary>
        int TimeDecimalPlaces { get; set; }
    }

    /// <summary>
    /// Additional methods of <see cref="IXUnits"/>
    /// </summary>
    public static class XUnitsExtension 
    {
        /// <summary>
        /// Conversion factor from system units (meters) to the specific length units
        /// </summary>
        public static Dictionary<Length_e, double> LengthConversionFactor { get; } = new Dictionary<Length_e, double>()
        {
            { Length_e.Angstroms, 1e+10 },
            { Length_e.Nanometers, 1e+9  },
            { Length_e.Microns, 1000000 },
            { Length_e.Millimeters, 1000 },
            { Length_e.Centimeters, 100 },
            { Length_e.Meters, 1 },
            { Length_e.Microinches, 39370078.740157485 },
            { Length_e.Mils, 39370.078740157 },
            { Length_e.Inches, 39.3700787402 },
            { Length_e.Feet, 3.280839895 },
            { Length_e.FeetInches, 3.280839895 }
        };

        /// <summary>
        /// Conversion factor from system units (kilograms) to the specific mass units
        /// </summary>
        public static Dictionary<Mass_e, double> MassConversionFactor { get; } = new Dictionary<Mass_e, double>()
        {
            { Mass_e.Milligrams, 1000000 },
            { Mass_e.Grams, 1000 },
            { Mass_e.Kilograms, 1 },
            { Mass_e.Pounds, 2.2046226218 }
        };

        /// <summary>
        /// Conversion factor from system units (radians) to the specific angle units
        /// </summary>
        public static Dictionary<Angle_e, double> AngleConversionFactor { get; } = new Dictionary<Angle_e, double>()
        {
            { Angle_e.Degrees, 180 / Math.PI },
            { Angle_e.DegreesMinutes, 180 / Math.PI },
            { Angle_e.DegreesMinutesSeconds, 180 / Math.PI },
            { Angle_e.Radians, 1 }
        };

        /// <summary>
        /// Conversion factor from system units (seconds) to the specific time units
        /// </summary>
        public static Dictionary<Time_e, double> TimeConversionFactor { get; } = new Dictionary<Time_e, double>()
        {
            { Time_e.Seconds, 1 },
            { Time_e.Milliseconds, 1000 },
            { Time_e.Microseconds, 1000000 },
            { Time_e.Nanoseconds, 1e+9 },
            { Time_e.Minutes, 1 / 60 },
            { Time_e.Hours, 1 / 3600 }
        };

        /// <summary>
        /// Abbreviation of the length specific unit
        /// </summary>
        public static Dictionary<Length_e, string> LengthAbbreviation { get; } = new Dictionary<Length_e, string>
        {
            { Length_e.Angstroms, "Å" },
            { Length_e.Nanometers, "nm" },
            { Length_e.Microns, "μ" },
            { Length_e.Millimeters, "mm" },
            { Length_e.Centimeters, "cm" },
            { Length_e.Meters, "m" },
            { Length_e.Microinches, "µin" },
            { Length_e.Mils, "mil" },
            { Length_e.Inches, "in" },
            { Length_e.Feet, "ft" },
            { Length_e.FeetInches, "ft in" }
        };

        /// <summary>
        /// Abbreviation of the mass specific unit
        /// </summary>
        public static Dictionary<Mass_e, string> MassAbbreviation { get; } = new Dictionary<Mass_e, string>
        {       
            { Mass_e.Milligrams, "mg" },
            { Mass_e.Grams, "g" },
            { Mass_e.Kilograms, "kg" },
            { Mass_e.Pounds, "lb" }
        };

        /// <summary>
        /// Abbreviation of the angle specific unit
        /// </summary>
        public static Dictionary<Angle_e, string> AngleAbbreviation { get; } = new Dictionary<Angle_e, string>
        {
            { Angle_e.Degrees, "°" },
            { Angle_e.DegreesMinutes, "° '" },
            { Angle_e.DegreesMinutesSeconds, "° ' \"" },
            { Angle_e.Radians, "rad" }
        };

        /// <summary>
        /// Abbreviation of the time specific unit
        /// </summary>
        public static Dictionary<Time_e, string> TimeAbbreviation { get; } = new Dictionary<Time_e, string>
        {
            { Time_e.Seconds, "sec" },
            { Time_e.Milliseconds, "msec" },
            { Time_e.Microseconds, "µsec" },
            { Time_e.Nanoseconds, "nsec" },
            { Time_e.Minutes, "min" },
            { Time_e.Hours, "hr" }
        };

        /// <summary>
        /// Gets the length conversion factor from system units (meters) to user units
        /// </summary>
        /// <param name="unit">Units</param>
        /// <returns>Conversion factor</returns>
        public static double GetLengthConversionFactor(this IXUnits unit)
            => LengthConversionFactor[unit.Length];

        /// <summary>
        /// Gets the mass conversion factor from system units (kilograms) to user units
        /// </summary>
        /// <param name="unit">Units</param>
        /// <returns>Conversion factor</returns>
        public static double GetMassConversionFactor(this IXUnits unit)
            => MassConversionFactor[unit.Mass];

        /// <summary>
        /// Gets the angle conversion factor from system units (radians) to user units
        /// </summary>
        /// <param name="unit">Units</param>
        /// <returns>Conversion factor</returns>
        public static double GetAngleConversionFactor(this IXUnits unit)
            => AngleConversionFactor[unit.Angle];

        /// <summary>
        /// Gets the time conversion factor from system units (seconds) to user units
        /// </summary>
        /// <param name="unit">Units</param>
        /// <returns>Conversion factor</returns>
        public static double GetTimeConversionFactor(this IXUnits unit)
            => TimeConversionFactor[unit.Time];

        /// <summary>
        /// Converts the length value from the user units to system units (meters)
        /// </summary>
        /// <param name="unit">Units</param>
        /// <param name="userValue">User value</param>
        /// <returns>Equivalent system value of length (meters)</returns>
        public static double ConvertLengthToSystemValue(this IXUnits unit, double userValue)
            => userValue / unit.GetLengthConversionFactor();

        /// <summary>
        /// Converts the length value from the system units (meters) to user units
        /// </summary>
        /// <param name="unit">Units</param>
        /// <param name="systemValue">System value of length (meters)</param>
        /// <returns>Equivalent user value</returns>
        public static double ConvertLengthToUserValue(this IXUnits unit, double systemValue)
            => systemValue * unit.GetLengthConversionFactor();

        /// <summary>
        /// Converts the mass value from the user unit to system units (kilograms)
        /// </summary>
        /// <param name="unit">Units</param>
        /// <param name="userValue">User value</param>
        /// <returns>Equivalent system value of mass (kilograms)</returns>
        public static double ConvertMassToSystemValue(this IXUnits unit, double userValue)
            => userValue / unit.GetMassConversionFactor();

        /// <summary>
        /// Converts the mass value from the system units (kilograms) to user units
        /// </summary>
        /// <param name="unit">Units</param>
        /// <param name="systemValue">System value of mass (kilograms)</param>
        /// <returns>Equivalent user value</returns>
        public static double ConvertMassToUserValue(this IXUnits unit, double systemValue)
            => systemValue * unit.GetMassConversionFactor();

        /// <summary>
        /// Converts the angle value from the user unit to system units (radians)
        /// </summary>
        /// <param name="unit">Units</param>
        /// <param name="userValue">User value</param>
        /// <returns>Equivalent system value of angle (radians)</returns>
        public static double ConvertAngleToSystemValue(this IXUnits unit, double userValue)
            => userValue / unit.GetAngleConversionFactor();

        /// <summary>
        /// Converts the angle value from the system units (radians) to user units
        /// </summary>
        /// <param name="unit">Units</param>
        /// <param name="systemValue">System value of angle (radians)</param>
        /// <returns>Equivalent user value</returns>
        public static double ConvertAngleToUserValue(this IXUnits unit, double systemValue)
            => systemValue * unit.GetAngleConversionFactor();

        /// <summary>
        /// Converts the time value from the user unit to system units (seconds)
        /// </summary>
        /// <param name="unit">Units</param>
        /// <param name="userValue">User value</param>
        /// <returns>Equivalent system value of time (seconds)</returns>
        public static double ConvertTimeToSystemValue(this IXUnits unit, double userValue)
            => userValue / unit.GetTimeConversionFactor();

        /// <summary>
        /// Converts the time value from the system units (seconds) to user units
        /// </summary>
        /// <param name="unit">Units</param>
        /// <param name="systemValue">System value of time (seconds)</param>
        /// <returns>Equivalent user value</returns>
        public static double ConvertTimeToUserValue(this IXUnits unit, double systemValue)
            => systemValue * unit.GetTimeConversionFactor();

        /// <summary>
        /// Gets the abbreviation (short name) of current length units
        /// </summary>
        /// <param name="unit">Units</param>
        /// <returns>Length unit abbreviation</returns>
        public static string GetLengthAbbreviation(this IXUnits unit) => LengthAbbreviation[unit.Length];


        /// <summary>
        /// Gets the abbreviation (short name) of current mass units
        /// </summary>
        /// <param name="unit">Units</param>
        /// <returns>Mass unit abbreviation</returns>
        public static string GetMassAbbreviation(this IXUnits unit) => MassAbbreviation[unit.Mass];

        /// <summary>
        /// Gets the abbreviation (short name) of current angle units
        /// </summary>
        /// <param name="unit">Units</param>
        /// <returns>Angle unit abbreviation</returns>
        public static string GetAngleAbbreviation(this IXUnits unit) => AngleAbbreviation[unit.Angle];

        /// <summary>
        /// Gets the abbreviation (short name) of current time units
        /// </summary>
        /// <param name="unit">Units</param>
        /// <returns>Time unit abbreviation</returns>
        public static string GetTimeAbbreviation(this IXUnits unit) => TimeAbbreviation[unit.Time];
    }
}
