﻿using System;
using EVEMon.Common.Extensions;
using Xunit;

namespace Tests.EVEMon.Common
{
    public static class TimeExtensionsTests
    {
        #region Helper Variables

        /// <summary>
        /// Valid time as returned by CCP.
        /// </summary>
        private static string ValidCcpDateTime => "2010-05-07 18:23:32";

        /// <summary>
        /// Invalid time, wrong format.
        /// </summary>
        private static string InvalidCcpDateTime => "18-23-32 2010:05:07";

        /// <summary>
        /// A point in time.
        /// </summary>
        private static DateTime PointInTime => new DateTime(2010, 05, 07, 18, 23, 32);

        /// <summary>
        /// Valid dot formated date/time string.
        /// </summary>
        private static string ValidDotFormattedDateTimeString => "2010.05.07 18:23:32";

        #endregion


        #region Tests

        /// <summary>
        /// Able to convert a <c>DateTime</c> to a CCPTime.
        /// </summary>
        [Fact]
        public static void ConvertDateTimeToCCPDateTime()
        {
            var result = PointInTime.DateTimeToTimeString();
            Assert.Equal(ValidCcpDateTime, result);
        }

        /// <summary>
        /// Able to convert a correctly formatted CCPDateTime to <c>DateTime</c>.
        /// </summary>
        [Fact]
        public static void ConvertValidCCPDateTimeToDateTime()
        {
            var result = ValidCcpDateTime.TimeStringToDateTime();
            Assert.Equal(PointInTime, result);
        }

        /// <summary>
        /// Handles an incorrect input string appropiately.
        /// </summary>
        [Fact]
        public static void ConvertInvalidCCPDateTimeToDateTime()
        {
            var result = InvalidCcpDateTime.TimeStringToDateTime();
            Assert.Equal(default(DateTime), result);
        }

        /// <summary>
        /// Handles an empty string by returning DateTime.MinValue.
        /// </summary>
        [Fact]
        public static void ConvertEmptyCCPDateTimeToDateTime()
        {
            var result = string.Empty.TimeStringToDateTime();
            Assert.Equal(DateTime.MinValue, result);
        }

        /// <summary>
        /// Able to convert a <c>DateTime</c> to a dot formatted date/time string.
        /// </summary>
        [Fact]
        public static void ConvertDateTimeToDotFormattedString()
        {
            var result = PointInTime.DateTimeToDotFormattedString();
            Assert.Equal(ValidDotFormattedDateTimeString, result);
        }

        /// <summary>
        /// If the time being tested is in the past expect "Done" to be returned.
        /// </summary>
        [Fact]
        public static void ToRemainingTimeShortDescriptionReturnsDone()
        {
            var result = DateTime.Now.AddHours(-1).ToRemainingTimeShortDescription(DateTimeKind.Local);
            Assert.Equal("Done", result);
        }

        /// <summary>
        /// Fact 1s is returned when there is 1 minute to go.
        /// </summary>
        [Fact]
        public static void ToRemainingTimeShortDescriptionReturnsSecond()
        {
            var future = DateTime.Now.AddSeconds(1);
            var result = future.ToRemainingTimeShortDescription(DateTimeKind.Local);
            Assert.Equal("1s", result);
        }

        /// <summary>
        /// Fact 1m is returned when there is 1 minute to go.
        /// </summary>
        [Fact]
        public static void ToRemainingTimeShortDescriptionReturnsMinute()
        {
            var future = DateTime.Now.AddMinutes(1);
            var result = future.ToRemainingTimeShortDescription(DateTimeKind.Local);
            Assert.Equal("1m", result);
        }

        /// <summary>
        /// Fact 1h is returned when there is 1 hour to go.
        /// </summary>
        [Fact]
        public static void ToRemainingTimeShortDescriptionReturnsHour()
        {
            var future = DateTime.Now.AddHours(1);
            var result = future.ToRemainingTimeShortDescription(DateTimeKind.Local);
            Assert.Equal("1h", result);
        }

        /// <summary>
        /// Fact 1d is returned when there is 1 day to go.
        /// </summary>
        [Fact]
        public static void ToRemainingTimeShortDescriptionReturnsDay()
        {
            var future = DateTime.Now.AddDays(1);
            var result = future.ToRemainingTimeShortDescription(DateTimeKind.Local);
            Assert.Equal("1d", result);
        }

        /// <summary>
        /// Fact 1m 1s is returned when there is 1 minute, 1 second to go.
        /// </summary>
        [Fact]
        public static void ToRemainingTimeShortDescriptionReturnsMinuteSecond()
        {
            var future = DateTime.Now.AddMinutes(1).AddSeconds(1);
            var result = future.ToRemainingTimeShortDescription(DateTimeKind.Local);
            Assert.Equal("1m 1s", result);
        }

        /// <summary>
        /// Fact 1h 1m 1s is returned when there is 1 hour, 1 minute, 1 second to go.
        /// </summary>
        [Fact]
        public static void ToRemainingTimeShortDescriptionReturnsHourMinuteSecond()
        {
            var future = DateTime.Now.AddHours(1).AddMinutes(1).AddSeconds(1);
            var result = future.ToRemainingTimeShortDescription(DateTimeKind.Local);
            Assert.Equal("1h 1m 1s", result);
        }

        /// <summary>
        /// Fact 1d 1h 1m 1s is returned when there is 1 day, 1 hour, 1 minute, 1 second to go.
        /// </summary>
        [Fact]
        public static void ToRemainingTimeShortDescriptionReturnsDayHourMinuteSecond()
        {
            var future = DateTime.Now.AddDays(1).AddHours(1).AddMinutes(1).AddSeconds(1);
            var result = future.ToRemainingTimeShortDescription(DateTimeKind.Local);
            Assert.Equal("1d 1h 1m 1s", result);
        }

        /// <summary>
        /// Fact 1d 1m 1s is returned when there is 1 hour, 1 minute, 1 second to go.
        /// </summary>
        [Fact]
        public static void ToRemainingTimeShortDescriptionReturnsDayMinuteSecond()
        {
            var future = DateTime.Now.AddDays(1).AddMinutes(1).AddSeconds(1);
            var result = future.ToRemainingTimeShortDescription(DateTimeKind.Local);
            Assert.Equal("1d 1m 1s", result);
        }

        /// <summary>
        /// Fact 1d 1m is returned when there is 1 hour, 1 minute to go.
        /// </summary>
        [Fact]
        public static void ToRemainingTimeShortDescriptionReturnsDayMinute()
        {
            var future = DateTime.Now.AddDays(1).AddMinutes(1);
            var result = future.ToRemainingTimeShortDescription(DateTimeKind.Local);
            Assert.Equal("1d 1m", result);
        }

        #endregion
    }
}