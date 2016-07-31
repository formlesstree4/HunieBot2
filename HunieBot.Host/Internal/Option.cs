using System;

namespace HunieBot.Host.Internal
{

    /// <summary>
    ///     Represents something that may or may not have a value.
    /// </summary>
    /// <typeparam name="T">Any type that needs to be wrapped in <see cref="Option{T}"/></typeparam>
    /// <remarks>
    ///     <see cref="Option{T}"/> is used whenever an API call has the possibility of not returning a value. A SQL command, for instance, may not have an actual value. A PMS Reservation number may not actually exist in our system.
    /// The purpose of using <see cref="Option{T}"/> is to provide a thoughtful way to expose whether or not an API has a return value. <see cref="Option.Empty"/> is much more explicit than a null value for a reference type. <see cref="Nullable{T}"/> would be more useful, however, it is only for value types (aka types that are structures instead of classes).
    /// This is still problematic because NULL for a value type return may still be valid. <see cref="Option{T}"/>, with the ability to explicitly use <see cref="Option.Empty"/> and check <see cref="Option{T}.HasValue"/>, is much clearer and more concise.
    /// </remarks>
    public struct Option<T>
    {
        private readonly T _value;



        /// <summary>
        ///     Gets whether this <see cref="Option{T}"/> has a <see cref="Value"/>.
        /// </summary>
        public bool HasValue { get; }

        /// <summary>
        ///     Gets the Value (<typeparamref name="T"/>) of this <see cref="Option{T}"/>
        /// </summary>
        /// <exception cref="InvalidOperationException">If <see cref="HasValue"/> is false</exception>
        public T Value
        {
            get
            {
                if (!HasValue) throw new InvalidOperationException("There is no value. Please check for a value first.");
                return _value;
            }
        }



        /// <summary>
        ///     An internal constructor of <see cref="Option{T}"/> that can indicate whether or not there is a value.
        /// </summary>
        /// <param name="value">The value of <typeparamref name="T"/></param>
        /// <param name="hasValue">A <see cref="bool"/> value that indicates whether or not <paramref name="value"/> contains a value</param>
        private Option(T value, bool hasValue)
        {
            _value = value;
            HasValue = hasValue;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="Option{T}"/> with a value.
        /// </summary>
        public Option(T value) : this(value, true) { }



        /// <summary>
        ///     Converts an instance of <see cref="Option{T}"/> to its actual value.
        /// </summary>
        /// <exception cref="InvalidOperationException">If <see cref="HasValue"/> is false</exception>
        public static implicit operator T(Option<T> option)
        {
            return option.Value;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="Option{T}"/> with a value.
        /// </summary>
        public static implicit operator Option<T>(T value)
        {
            return new Option<T>(value);
        }

        /// <summary>
        ///     Implicitly converts the non-generic <see cref="Option"/> to <see cref="Empty"/>
        /// </summary>
        public static implicit operator Option<T>(Option option)
        {
            return Empty;
        }

        /// <summary>
        ///     Gets an empty <see cref="Option{T}"/>.
        /// </summary>
        public static Option<T> Empty { get; } = new Option<T>(default(T), false);

    }

    /// <summary>
    ///     Non-generic class for interacting with <see cref="Option{T}"/>. It contains assistance methods with creating instances of <see cref="Option{T}"/>.
    /// </summary>
    /// <remarks>
    ///     Please note that <see cref="Empty"/> can be cast to <see cref="Option{T}.Empty"/>. More specifically, <see cref="Option"/> may be cast to <see cref="Option{T}"/> with the conversion always resulting in <see cref="Option{T}.Empty"/>.
    /// </remarks>
    public sealed class Option
    {

        /// <summary>
        ///     Private constructor. An instance of <see cref="Option"/> should never be declared. Please use <see cref="Empty"/> or <see cref="Create{T}(T)"/> to create instances of <see cref="Option"/> and <see cref="Option{T}"/> respectively.
        /// </summary>
        private Option() { }

        /// <summary>
        /// Represents any empty <see cref="Option"/>
        /// </summary>
        public static Option Empty { get; } = new Option();

        /// <summary>
        /// Creates a new <see cref="Option{T}"/> with a value.
        /// </summary>
        /// <param name="value">Value to be passed in.</param>
        /// <returns></returns>
        public static Option<T> Create<T>(T value)
        {
            return new Option<T>(value);
        }

    }

}
