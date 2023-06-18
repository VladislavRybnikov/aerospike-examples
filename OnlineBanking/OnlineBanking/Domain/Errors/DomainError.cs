using System.Runtime.CompilerServices;

namespace OnlineBanking.Domain;

public record DomainError(DomainErrorType Type, string Message, Dictionary<string, object?>? Metadata = null)
{
    protected class MetadataBuilder
    {
        private Dictionary<string, object?>? _metadata;

        public MetadataBuilder Add(object? value, [CallerArgumentExpression(nameof(value))] string? key = null)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(key);
            AddKeyValue(key, value);
            return this;
        }

        public MetadataBuilder AddIfNotNull(object? value, [CallerArgumentExpression(nameof(value))] string? key = null)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(key);
            if (value != null) AddKeyValue(key, value);
            return this;
        }

        public MetadataBuilder AddIfNotEmpty(string? value, [CallerArgumentExpression(nameof(value))] string? key = null)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(key);
            if (!string.IsNullOrWhiteSpace(value)) AddKeyValue(key, value);
            return this;
        }

        private MetadataBuilder AddKeyValue(string key, object? value)
        {
            _metadata ??= new Dictionary<string, object?>();
            _metadata.TryAdd(key, value);

            return this;
        }

        public Dictionary<string, object?>? Build() => _metadata;
    }
}
