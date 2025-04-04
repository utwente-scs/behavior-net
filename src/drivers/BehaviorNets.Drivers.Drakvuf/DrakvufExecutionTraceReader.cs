using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using BehaviorNets.Data;
using BehaviorNets.Drivers.Drakvuf.Reporting;
using Serilog;

namespace BehaviorNets.Drivers.Drakvuf;

public static class DrakvufExecutionTraceReader
{
    private static readonly DateTime LinuxEpoch = new(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);

    private static readonly Regex ArgumentRegex = new(@"Arg(\d+)=0x([0-9a-fA-F]+)(:.*)?");
    private static readonly Regex ArgumentStringRegex = new(@"^Arg(\d+)=0x([0-9a-fA-F]+):("".*"")$");

    public static ExecutionResult FromDirectory(string directory)
    {
        return FromFiles(
            Path.Combine(directory, "drakrun.log"),
            Path.Combine(directory, "apimon.log"),
            Path.Combine(directory, "syscall.log"),
            Path.Combine(directory, "regmon.log"),
            Path.Combine(directory, "librarymon.log")
        );
    }

    public static ExecutionResult FromFiles(string drakrunReportPath, string apiMonReportPath, string sysCallReportPath, string regMonReportPath, string libraryMonReportPath)
    {
        var drakrunReport = !string.IsNullOrEmpty(drakrunReportPath) && File.Exists(drakrunReportPath)
            ? DrakRunReport.FromFile(drakrunReportPath)
            : null;

        var apiMonReport = !string.IsNullOrEmpty(apiMonReportPath) && File.Exists(apiMonReportPath)
            ? ApiMonReport.FromFile(apiMonReportPath)
            : null;

        var sysCallReport = !string.IsNullOrEmpty(sysCallReportPath) && File.Exists(sysCallReportPath)
            ? SysCallReport.FromFile(sysCallReportPath)
            : null;

        var regMonReport = !string.IsNullOrEmpty(regMonReportPath) && File.Exists(regMonReportPath)
            ? RegMonReport.FromFile(regMonReportPath)
            : null;

        var libraryMonReport = !string.IsNullOrEmpty(libraryMonReportPath) && File.Exists(libraryMonReportPath)
            ? LibraryMonReport.FromFile(libraryMonReportPath)
            : null;

        return FromReports(drakrunReport, apiMonReport, sysCallReport, regMonReport, libraryMonReport);
    }

    public static ExecutionResult FromReports(
        DrakRunReport drakRunReport,
        ApiMonReport apiMonReport,
        SysCallReport sysCallReport,
        RegMonReport regMonReport,
        LibraryMonReport libraryMonReport)
    {
        var callEvents = new List<ExecutionEvent>();

        // Translate api mon events.
        if (apiMonReport is not null)
        {
            decimal lastTimeStamp = 0;
            foreach (var e in apiMonReport.Events)
            {
                ExecutionEvent newEvent;
                switch (e.Event)
                {
                    case "api_called":
                        lastTimeStamp = UnixTimeStampToDateTime(e.TimeStamp);
                        newEvent = new ExecutionEvent(lastTimeStamp, e.PID, e.TID, e.Method);
                        foreach (string argument in e.Arguments)
                            newEvent.Arguments.Add(InterpretArgumentString(argument));
                        if (e.ReturnValue is { } returnValue)
                            newEvent.ReturnValue = InterpretReturnValueString(returnValue);
                        break;
                        
                    case "dll_loaded":
                        newEvent = new ExecutionEvent(lastTimeStamp, e.PID, e.TID, "dll_loaded");
                        newEvent.Arguments.Add(e.DllName);
                        newEvent.Arguments.Add(ulong.Parse(e.DllBase.AsSpan(2), NumberStyles.HexNumber | NumberStyles
                            .AllowHexSpecifier));
                        break;
                        
                    default:
                        continue;
                }
                    
                callEvents.Add(newEvent);
            }
        }

        // Translate syscall events.
        if (sysCallReport is not null)
        {
            foreach (var e in sysCallReport.Events)
            {
                var unixTimeStampToDateTime = UnixTimeStampToDateTime(e.TimeStamp);
                var newEvent = new ExecutionEvent(unixTimeStampToDateTime, e.PID, e.TID, e.Method);
                foreach (var argument in e.Arguments)
                    newEvent.Arguments.Add(InterpretArgumentString(argument.Value.GetString()));
                callEvents.Add(newEvent);
            }
        }

        // Translate regmon events.
        if (regMonReport is not null)
        {
            foreach (var e in regMonReport.Events)
            {
                var unixTimeStampToDateTime = UnixTimeStampToDateTime(e.TimeStamp);
                var newEvent = new ExecutionEvent(unixTimeStampToDateTime, e.PID, e.TID, e.Method);
                newEvent.Arguments.Add(e.Key);
                newEvent.Arguments.Add(e.ValueName);
                newEvent.Arguments.Add(e.Value);
                callEvents.Add(newEvent);
            }
        }

        // Translate librarymon events.
        if (libraryMonReport is not null)
        {
            foreach (var e in libraryMonReport.Events)
            {
                var unixTimeStampToDateTime = UnixTimeStampToDateTime(e.TimeStamp);
                var newEvent = new ExecutionEvent(unixTimeStampToDateTime, e.PID, e.TID, e.Method);
                newEvent.Arguments.Add(e.ModuleName);
                newEvent.Arguments.Add(e.ModulePath);
                callEvents.Add(newEvent);
            }
        }

        // Sort by timestamp.
        callEvents.Sort((e1, e2) => e1.Time.CompareTo(e2.Time));
        var eventStream = new ExecutionEventStream(callEvents);

        return new ExecutionResult(drakRunReport?.ExtractSampleHash() ?? "<unknown>", eventStream);
    }

    private static decimal UnixTimeStampToDateTime(string unixTimeStamp) =>
        decimal.Parse(unixTimeStamp);

    private static object InterpretArgumentString(string argument)
    {
        var match = ArgumentStringRegex.Match(argument);
        if (match.Success)
            return ParseString(match.Groups[3].Value);

        match = ArgumentRegex.Match(argument);
        if (!match.Success)
            return null;

        return ulong.Parse(match.Groups[2].Value, NumberStyles.HexNumber);
    }

    private static object InterpretReturnValueString(string rawString)
    {
        return ulong.Parse(rawString[2..], NumberStyles.HexNumber);
    }

    private static string ParseString(string rawString)
    {
        var builder = new StringBuilder();

        bool escape = false;
        for (int i = 1; i < rawString.Length - 1; i++)
        {
            char c = rawString[i];

            if (c == '\\')
            {
                escape = !escape;
            }
            else if (escape)
            {
                switch (c)
                {
                    case 'n':
                        c = '\n';
                        break;
                    case 'r':
                        c = '\r';
                        break;
                    case 't':
                        c = '\t';
                        break;
                    case '"':
                        c = '"';
                        break;
                    default:
                        Log.Warning("Unrecognized escape sequence {Sequence}", $"\\{c}");
                        builder.Append('\\');
                        break;
                }

                escape = false;
            }

            if (!escape)
                builder.Append(c);
        }

        return builder.ToString();
    }
}