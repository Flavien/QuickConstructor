﻿// Copyright 2022 Flavien Charlon
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace AutoConstructor.Attributes;

using System;
using System.Diagnostics;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
[Conditional("INCLUDE_AUTO_CONSTRUCTOR_ATTRIBUTES")]
public class AutoConstructorAttribute : Attribute
{
    public bool IncludeNonReadOnlyMembers { get; set; } = true;

    public NullChecksSettings NullChecks { get; set; } = NullChecksSettings.NonNullableReferencesOnly;

    public Accessibility ConstructorAccessibility { get; set; } = Accessibility.Public;
}