using System;


namespace HunieBot.Host.Interfaces
{

    /// <summary>
    ///     A simplistic logging interface.
    /// </summary>
    public interface ILogging
    {

        /// <summary>
        ///     Write out a message at the trace level logging.
        /// </summary>
        /// <param name="message">The message to write out into the log</param>
        /// <remarks>
        ///     Trace level logging is extremely noisy and used to output a lot of information.
        /// </remarks>
        void Trace(string message);

        /// <summary>
        ///     Write out a formatted message at the trace level logging.
        /// </summary>
        /// <param name="format">A formatted string that describes how <paramref name="content"/> should be written</param>
        /// <param name="content">The content to write out</param>
        /// <remarks>
        ///     Trace level logging is extremely noisy and used to output a lot of information.
        /// </remarks>
        void Trace(string format, params object[] content);

        /// <summary>
        ///     Write out a message at the debug level logging.
        /// </summary>
        /// <param name="message">The message to write out into the log</param>
        /// <remarks>
        ///     <see cref="Debug(string)"/> is not as noisy as <see cref="Trace(string)"/>. As such, it should only be utilized for outputting useful debug information.
        /// </remarks>
        void Debug(string message);

        /// <summary>
        ///     Write out a formatted message at the debug level logging.
        /// </summary>
        /// <param name="format">A formatted string that describes how <paramref name="content"/> should be written</param>
        /// <param name="content">The content to write out</param>
        /// <remarks>
        ///     <see cref="Debug(string, object[])"/> is not as noisy as <see cref="Trace(string, object[])"/>. As such, it should only be utilized for outputting useful debug information.
        /// </remarks>
        void Debug(string format, params object[] content);

        /// <summary>
        ///     Write out a message at the info level logging
        /// </summary>
        /// <param name="message">The message to write out into the log</param>
        /// <remarks>
        ///     <see cref="Info(string)"/> is less noisy than <see cref="Debug(string)"/>. <see cref="Info(string)"/> is invoked mostly to output occasional bits of information, such as the start and stop of major events inside of the application.
        /// </remarks>
        void Info(string message);

        /// <summary>
        ///     Write out a formatted message at the info level logging.
        /// </summary>
        /// <param name="format">A formatted string that describes how <paramref name="content"/> should be written</param>
        /// <param name="content">The content to write out</param>
        /// <remarks>
        ///     <see cref="Info(string, object[])"/> is less noisy than <see cref="Debug(string, object[])"/>. <see cref="Info(string, object[])"/> is invoked mostly to output occasional bits of information, such as the start and stop of major events inside of the application.
        /// </remarks>
        void Info(string format, params object[] content);

        /// <summary>
        ///     Write out a message at the fatal level logging.
        /// </summary>
        /// <param name="message">The message to write out into the log</param>
        /// <param name="error">A reference to the <see cref="Exception"/> that occurred</param>
        /// <remarks>
        ///     <see cref="Fatal(string, Exception)"/> should only be invoked when there is an <see cref="Exception"/> that cannot be handled.
        /// </remarks>
        void Fatal(string message, Exception error);

        /// <summary>
        ///     Write out a formatted message at the fatal level logging.
        /// </summary>
        /// <param name="format">A formatted string that describes how <paramref name="content"/> should be written</param>
        /// <param name="content">The content to write out</param>
        /// <param name="error">A reference to the <see cref="Exception"/> that occurred</param>
        /// <remarks>
        ///     <see cref="Fatal(string, Exception, object[])"/> should only be invoked when there is an <see cref="Exception"/> that cannot be handled.
        /// </remarks>
        void Fatal(string format, Exception error, params object[] content);

    }

}