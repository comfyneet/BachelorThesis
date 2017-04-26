﻿using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RiceDoctor.InferenceEngine;

namespace RiceDoctor.Tests
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TestFactType
    {
        ScalarFact,
        IndividualFact,
        FuzzyFact
    }

    public static class TestFactTypeExtensions
    {
        public static RequestType ToRequestType(this TestFactType testFactType)
        {
            switch (testFactType)
            {
                case TestFactType.ScalarFact:
                    return RequestType.ScalarFact;
                case TestFactType.IndividualFact:
                    return RequestType.IndividualFact;
                default:
                    throw new ArgumentException(nameof(testFactType));
            }
        }
    }
}