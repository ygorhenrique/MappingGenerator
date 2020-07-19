using System;
using MappingGenerator.Mappings;
using MappingGenerator.Mappings.SourceFinders;
using Microsoft.CodeAnalysis;

namespace MappingGenerator.RoslynHelpers
{
    public class ObjectField : IObjectField
    {
        private readonly IFieldSymbol fieldSymbol;
        private readonly Lazy<bool> _lazyCanBeNull;

        public ObjectField(IFieldSymbol fieldSymbol)
        {
            this.fieldSymbol = fieldSymbol;
            this._lazyCanBeNull = new Lazy<bool>(fieldSymbol.CanBeNull);

        }

        public string Name => fieldSymbol.Name;

        public ITypeSymbol Type => fieldSymbol.Type;
        public bool CanBeNull => _lazyCanBeNull.Value;

        public bool CanBeSet(ITypeSymbol via, MappingContext mappingContext)
        {
            if (fieldSymbol.IsReadOnly)
            {
                return false;
            }

            return mappingContext.AccessibilityHelper.IsSymbolAccessible(fieldSymbol, via);

        }

        public bool CanBeSetInConstructor(ITypeSymbol via, MappingContext mappingContext)
        {
            return mappingContext.AccessibilityHelper.IsSymbolAccessible(fieldSymbol, via);
        }

        public bool CanBeGet(ITypeSymbol via, MappingContext mappingContext)
        {
            return mappingContext.AccessibilityHelper.IsSymbolAccessible(fieldSymbol, via);
        }
    }
}