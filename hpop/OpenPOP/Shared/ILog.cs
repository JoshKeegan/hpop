﻿namespace OpenPOP.Shared
{
	public interface ILog
	{
		/// <summary>
		/// Logs an error to the logs
		/// </summary>
		/// <param name="message">This is the string to log</param>
		void LogError(string message);

		/// <summary>
		/// Logs a debug message to the logs
		/// </summary>
		/// <param name="message">This is the debug message to log</param>
		void LogDebug(string message);
	}
}