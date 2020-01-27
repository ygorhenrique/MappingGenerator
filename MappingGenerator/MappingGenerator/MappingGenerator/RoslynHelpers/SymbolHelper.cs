﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MappingGenerator.RoslynHelpers
{
    internal static class SymbolHelper
    {
        public static bool IsConstructor(IMethodSymbol methodSymbol)
        {
            return methodSymbol.MethodKind == MethodKind.Constructor;
        }

        public static IEnumerable<ITypeSymbol> UnwrapGeneric(this ITypeSymbol typeSymbol)
        {
            if (typeSymbol.TypeKind == TypeKind.TypeParameter && typeSymbol is ITypeParameterSymbol namedType && namedType.Kind != SymbolKind.ErrorType)
            {
                return namedType.ConstraintTypes;
            }
            return new []{typeSymbol};
        }

        public static bool CanBeSetPublicly(this IPropertySymbol property, bool isInternalAccessible)
        {
            if (IsDeclaredOutsideTheSourcecode(property))
            {
                return property.IsReadOnly == false && 
                       property.SetMethod != null &&
                       property.SetMethod.DeclaredAccessibility == Accessibility.Public;
            }

            var propertyDeclaration = property.DeclaringSyntaxReferences.Select(x => x.GetSyntax()).OfType<PropertyDeclarationSyntax>().FirstOrDefault();
            if (propertyDeclaration?.AccessorList == null)
            {
                return false;
            }

            return HasPublicSetter(propertyDeclaration, isInternalAccessible);
        }

        public static bool IsDeclaredOutsideTheSourcecode(IPropertySymbol property)
        {
            return property.DeclaringSyntaxReferences.Length == 0;
        }

        public static bool HasPrivateSetter(PropertyDeclarationSyntax propertyDeclaration)
        {
            return propertyDeclaration.AccessorList.Accessors.Any(x =>x.Keyword.Kind() == SyntaxKind.SetKeyword && x.Modifiers.Any(m => m.Kind() == SyntaxKind.PrivateKeyword));
        } 
        
        public static bool HasPublicSetter(PropertyDeclarationSyntax propertyDeclaration, bool isInternalAccessible)
        {
            return propertyDeclaration.AccessorList.Accessors.Any(x =>
            {
                if (x.Keyword.Kind() == SyntaxKind.SetKeyword)
                {
                    return x.Modifiers.Count == 0 || x.Modifiers.Any(m => AllowsForPublic(m, isInternalAccessible));
                }
                return false;
            });
        }

        private static bool AllowsForPublic(SyntaxToken accessor, bool isInternalAccessible)
        {
            switch (accessor.Kind())
            {
                case SyntaxKind.PrivateKeyword:
                case SyntaxKind.ProtectedKeyword:
                    return false;
                case SyntaxKind.InternalKeyword when isInternalAccessible:
                    return true;
                case SyntaxKind.PublicKeyword:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsNullable(ITypeSymbol type, out ITypeSymbol underlyingType)
        {
            if (type.TypeKind == TypeKind.Struct && type.Name == "Nullable")
            {
                underlyingType = GetUnderlyingNullableType(type);
                return true;
            }

            underlyingType = null;
            return false;
        }

        public static ITypeSymbol GetUnderlyingNullableType(ITypeSymbol type)
        {
            return ((INamedTypeSymbol) type).TypeArguments.First();
        }
    }
}