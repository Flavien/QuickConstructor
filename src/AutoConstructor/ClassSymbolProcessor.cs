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

namespace AutoConstructor;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AutoConstructor.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public class ClassSymbolProcessor
{
    private readonly INamedTypeSymbol _classSymbol;
    private readonly ClassDeclarationSyntax _declarationSyntax;
    private readonly AutoConstructorAttribute _attribute;

    public ClassSymbolProcessor(
        INamedTypeSymbol classSymbol,
        ClassDeclarationSyntax declarationSyntax,
        AutoConstructorAttribute attribute)
    {
        _classSymbol = classSymbol;
        _declarationSyntax = declarationSyntax;
        _attribute = attribute;
    }

    public INamedTypeSymbol ClassSymbol { get => _classSymbol; }

    public ConstructorDescriptor AnalyzeType()
    {
        if (!_declarationSyntax.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword)))
        {
            throw new AutoConstructorException(Diagnostic.Create(
                DiagnosticDescriptors.ClassMustBePartial,
                _declarationSyntax.GetLocation(),
                _classSymbol.Name));
        }

        ClassMembersAnalyzer classMembersAnalyzer = new(_classSymbol, _attribute);
        IReadOnlyList<ConstructorParameter> members = classMembersAnalyzer.GetConstructorParameters();

        IReadOnlyList<ConstructorParameter> baseClassMembers = ImmutableArray
            .CreateRange(GetRecursiveClassMembers(_classSymbol.BaseType));

        ILookup<string, ConstructorParameter> lookup = members
            .ToLookup(member => member.ParameterName, StringComparer.Ordinal);

        IList<ConstructorParameter> duplicates = lookup
            .Where(nameGroup => nameGroup.Count() > 1)
            .Select(nameGroup => nameGroup.Last())
            .ToList();

        if (duplicates.Count > 0)
        {
            throw new AutoConstructorException(Diagnostic.Create(
                DiagnosticDescriptors.DuplicateConstructorParameter,
                _declarationSyntax.GetLocation(),
                duplicates[0].ParameterName,
                _classSymbol.Name));
        }

        return new ConstructorDescriptor(
            _classSymbol,
            _attribute.ConstructorAccessibility,
            constructorParameters: members,
            baseClassConstructorParameters: baseClassMembers);
    }

    private static IEnumerable<ConstructorParameter> GetRecursiveClassMembers(INamedTypeSymbol? classSymbol)
    {
        if (classSymbol != null)
        {
            AutoConstructorAttribute? attribute = classSymbol.GetAttribute<AutoConstructorAttribute>();
            if (attribute != null)
            {
                ClassMembersAnalyzer analyzer = new(classSymbol, attribute);
                IReadOnlyList<ConstructorParameter> parameters = analyzer.GetConstructorParameters();

                return GetRecursiveClassMembers(classSymbol.BaseType).Concat(parameters);
            }
        }

        return ImmutableArray<ConstructorParameter>.Empty;
    }
}
