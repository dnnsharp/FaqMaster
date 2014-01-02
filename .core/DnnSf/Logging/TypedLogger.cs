using DnnSharp.FaqMaster.Core.DnnSf.Logging.Target;
using DotNetNuke.Entities.Portals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DnnSharp.FaqMaster.Core.DnnSf.Logging
{
    public class TypedLogger<T> // TODO: implement some ILogger
        where T : class
    {
        public IList<ILogTarget<T>> Targets { get; set; }

        public eLogLevel MinLevel { get; set; }

        /// <summary>
        /// if set, it will receive current MinLevel as 2nd parameter
        /// </summary>
        public Func<T, eLogLevel, eLogLevel> FnLevel { get; set; }

        /// <summary>
        /// This data will be used whenever data parameter is passed null
        /// </summary>
        public T Data { get; set; }

        public TypedLogger()
        {
            Targets = new List<ILogTarget<T>>();
        }

        public TypedLogger(T data)
        {
            Targets = new List<ILogTarget<T>>();
            Data = data;
        }

        public void Log(eLogLevel level, T data, string message)
        {
            data = data ?? Data;
            if (GetCurrentMinLevel(data) < level)
                return;

            foreach (var target in Targets)
                target.Log(level, data, message);
        }

        public void Log(eLogLevel level, T data, string messageFormat, params object[] args)
        {
            Log(level, data, string.Format(messageFormat, args));
        }

        public void Log(eLogLevel level, T data, Exception ex)
        {
            data = data ?? Data;
            if (GetCurrentMinLevel(data) < level)
                return;

            foreach (var target in Targets)
                target.Log(level, data, ex);
        }


        #region Convenience logging functions

        public void Error(T data, string messageFormat, params object[] args)
        {
            Log(eLogLevel.Errors, data, messageFormat, args);
        }

        public void Error(string messageFormat, params object[] args)
        {
            Log(eLogLevel.Errors, null, messageFormat, args);
        }

        public void Error(T data, string message)
        {
            Log(eLogLevel.Errors, data, message);
        }

        public void Error(string message)
        {
            Log(eLogLevel.Errors, null, message);
        }

        public void Error(T data, Exception ex)
        {
            Log(eLogLevel.Errors, data, ex);
        }

        public void Error(Exception ex)
        {
            Log(eLogLevel.Errors, null, ex);
        }


        public void Info(T data, string messageFormat, params object[] args)
        {
            Log(eLogLevel.Info, data, messageFormat, args);
        }

        public void Info(string messageFormat, params object[] args)
        {
            Log(eLogLevel.Info, null, messageFormat, args);
        }

        public void Info(T data, string message)
        {
            Log(eLogLevel.Info, data, message);
        }

        public void Info(string message)
        {
            Log(eLogLevel.Info, null, message);
        }

        public void Info(T data, Exception ex)
        {
            Log(eLogLevel.Info, data, ex);
        }

        public void Info(Exception ex)
        {
            Log(eLogLevel.Info, null, ex);
        }


        public void Debug(T data, string messageFormat, params object[] args)
        {
            Log(eLogLevel.Debug, data, messageFormat, args);
        }

        public void Debug(string messageFormat, params object[] args)
        {
            Log(eLogLevel.Debug, null, messageFormat, args);
        }

        public void Debug(T data, string message)
        {
            Log(eLogLevel.Debug, data, message);
        }

        public void Debug(string message)
        {
            Log(eLogLevel.Debug, null, message);
        }

        public void Debug(T data, Exception ex)
        {
            Log(eLogLevel.Debug, data, ex);
        }

        public void Debug(Exception ex)
        {
            Log(eLogLevel.Debug, null, ex);
        }

        #endregion



        public eLogLevel GetCurrentMinLevel(T data)
        {
            if (FnLevel != null)
                return FnLevel(data, MinLevel);
            return MinLevel;
        }


        public bool IsOff(T data)
        {
            return GetCurrentMinLevel(data) == eLogLevel.Off;
        }

        public bool IsOff()
        {
            return GetCurrentMinLevel(null) == eLogLevel.Off;
        }

        public bool IsErrorEnabled(T data)
        {
            return GetCurrentMinLevel(data) == eLogLevel.Errors;
        }

        public bool IsErrorEnabled()
        {
            return GetCurrentMinLevel(null) == eLogLevel.Errors;
        }

        public bool IsInfoEnabled(T data)
        {
            return GetCurrentMinLevel(data) == eLogLevel.Info;
        }

        public bool IsInfoEnabled()
        {
            return GetCurrentMinLevel(null) == eLogLevel.Info;
        }

        public bool IsDebugEnabled(T data)
        {
            return GetCurrentMinLevel(data) == eLogLevel.Debug;
        }

        public bool IsDebugEnabled()
        {
            return GetCurrentMinLevel(null) == eLogLevel.Debug;
        }

    }
}
