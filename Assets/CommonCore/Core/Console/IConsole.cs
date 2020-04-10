﻿using System.Reflection;

namespace CommonCore.Console
{
    /// <summary>
    /// Interface representing a command console system
    /// </summary>
    public interface IConsole
    {
        /// <summary>
        /// Add a command to the console system
        /// </summary>
        void AddCommand(MethodInfo command, bool useClassName, string alias, string className, string description);

        /// <summary>
        /// Write a line of text to the console
        /// </summary>
        void WriteLine(string line);

        /// <summary>
        /// Write a line of text to the console (with options)
        /// </summary>
        /// <param name="line">the text to write</param>
        /// <param name="type">the kind of message we are writing</param>
        /// <param name="context">(optional) the object the line was sent from</param>
        void WriteLineEx(string line, LogLevel type, object context);
    }
}