// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.


using Unity.Attributes;

namespace Unity.Tests.Override
{
    public class MySimpleType
    {
        [Dependency]
        public int X { get; set; }
    }
}