﻿namespace Tomataboard.Services.Extensions
{
    public static class StringExtensions
    {
        public static string LimitTo(this string data, int length)
        {
            return (data == null || data.Length < length)
              ? data
              : data.Substring(0, length);
        }
    }
}