using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace ObserverExample;

internal class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Notifies that a property has changed using the CallerMemberName attribute.
    /// </summary>
    /// <param name="propertyName">Automatically populated with the calling property name if not specified.</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Notifies that a property has changed using a property expression.
    /// </summary>
    protected void OnPropertyChanged<T>(Expression<Func<T>> propertyExpression)
    {
        if (PropertyChanged != null)
        {
            var propertyName = GetPropertyName(propertyExpression);
            OnPropertyChanged(propertyName);
        }
    }

    /// <summary>
    /// Gets the property name from a property expression.
    /// </summary>
    protected static string GetPropertyName<T>(Expression<Func<T>> propertyExpression)
    {
        if (propertyExpression.Body is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }

        throw new ArgumentException("The expression is not a valid property expression.", nameof(propertyExpression));
    }

    #region Debugging Aides

    [Conditional("DEBUG")]
    [DebuggerStepThrough]
    protected void VerifyPropertyName(string propertyName)
    {
        if (TypeDescriptor.GetProperties(this)[propertyName] == null)
        {
            string msg = $"Invalid property name: {propertyName}";

            if (ThrowOnInvalidPropertyName)
                throw new ArgumentException(msg, nameof(propertyName));

            Debug.Fail(msg);
        }
    }

    protected virtual bool ThrowOnInvalidPropertyName { get; } = false;

    #endregion

    #region IParentablePropertyExposer

    public Delegate[]? GetINPCSubscribers() => PropertyChanged?.GetInvocationList();

    #endregion
}
