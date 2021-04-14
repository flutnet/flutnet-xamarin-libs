// Copyright (c) 2020-2021 Novagem Solutions S.r.l.
//
// This file is part of Flutnet.
//
// Flutnet is a free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Flutnet is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with Flutnet.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Flutnet.Data;
using Flutnet.Interop.Embedding.Engine;
using Flutnet.Interop.Plugin.Common;
using Flutnet.ServiceModel;
using Flutnet.Utilities;
using Newtonsoft.Json;
using Object = Java.Lang.Object;

namespace Flutnet
{
    /// <summary>
    /// The Flutnet bridge allow the comunication between Xamarin and Flutter
    /// allowing two different FlutnetBridgeMode to help you develop your app in debug mode.
    /// </summary>
    public class FlutnetBridge : IDisposable
    {
        /// <summary>
        /// A handler for incoming method calls.
        /// </summary>
        internal class MethodCallHandler : Object, MethodChannel.IMethodCallHandler
        {
            readonly Action<MethodCall, MethodChannel.IResult> _onMethodCall;

            public MethodCallHandler(Action<MethodCall, MethodChannel.IResult> onMethodCall)
            {
                _onMethodCall = onMethodCall;
            }

            /// <summary>
            /// <para>
            /// Handles the specified method call received from Flutter.
            /// </para>
            /// <para>
            /// Handler implementations must submit a result for all incoming calls, by making a single call
            /// on the given <see cref="MethodChannel.IResult"/> callback.
            /// Failure to do so will result in lingering Flutter result handlers.
            /// The result may be submitted asynchronously.
            /// Calls to unknown or unimplemented methods should be handled using <see cref="MethodChannel.IResult.NotImplemented"/>.
            /// </para>
            /// <para>
            /// Any uncaught exception thrown by this method will be caught by the channel implementation and logged,
            /// and an error result will be sent back to Flutter.
            /// </para>
            /// <para>
            /// The handler is called on the platform thread (Android main thread).
            /// For more details see <see href="https://github.com/flutter/engine/wiki/Threading-in-the-Flutter-Engine">Threading in the Flutter Engine</see>.
            /// </para>
            /// </summary>
            /// <param name="call">A <see cref="MethodCall"/>.</param>
            /// <param name="result">A <see cref="MethodChannel.IResult"/> used for submitting the result of the call.</param>
            public void OnMethodCall(MethodCall call, MethodChannel.IResult result)
            {
                _onMethodCall?.Invoke(call, result);
            }
        }

        /// <summary>
        /// The platform side stream handler has two methods, <see cref="OnListen"/> and <see cref="OnCancel"/>, which are invoked
        /// whenever the number of listeners to the Dart stream goes from zero to one and back, respectively.
        /// This can happen multiple times. The stream handler implementation is supposed to start pouring events into the event sink
        /// when the former is called, and stop when the latter is called.
        /// In addition, it should pause when the ambient app component is not running.
        /// </summary>
        /// <seealso href="https://medium.com/flutter/flutter-platform-channels-ce7f540a104e"/>
        internal class StreamHandler : Object, EventChannel.IStreamHandler
        {
            FlutnetBridge _bridge;
            EventChannel.IEventSink _events;

            public StreamHandler(FlutnetBridge bridge)
            {
                _bridge = bridge;
            }

            /// <summary>
            /// <para>
            /// Handles a request to tear down the most recently created event stream.
            /// </para>
            /// <para>
            /// Any uncaught exception thrown by this method will be caught by the channel implementation and logged.
            /// An error result message will be sent back to Flutter.
            /// </para>
            /// <para>
            /// The channel implementation may call this method with null arguments to separate a pair of two consecutive set up requests.
            /// Such request pairs may occur during Flutter hot restart.
            /// Any uncaught exception thrown in this situation will be logged without notifying Flutter.
            /// </para>
            /// <para>
            /// Invoked when the last listener is deregistered from the Stream associated to this channel on the Flutter side.
            /// </para>
            /// </summary>
            /// <param name="arguments">Stream configuration arguments, possibly null.</param>
            public void OnCancel(Object arguments)
            {
                _events = null;
            }

            /// <summary>
            /// <para>
            /// Handles a request to tear down the most recently created event stream.
            /// </para>
            /// <para>
            /// Any uncaught exception thrown by this method will be caught by the channel implementation and logged.
            /// An error result message will be sent back to Flutter.
            /// </para>
            /// <para>
            /// The channel implementation may call this method with null arguments to separate a pair of two consecutive set up requests.
            /// Such request pairs may occur during Flutter hot restart.
            /// Any uncaught exception thrown in this situation will be logged without notifying Flutter.
            /// </para>
            /// <para>
            /// Invoked when the first listener is registered with the Stream associated to this channel on the Flutter side.
            /// </para>
            /// </summary>
            /// <param name="arguments">Stream configuration arguments, possibly null.</param>
            /// <param name="events">An <see cref="EventChannel.IEventSink"/> for emitting events to the Flutter receiver.</param>
            public void OnListen(Java.Lang.Object arguments, EventChannel.IEventSink events)
            {
                _events = events;
            }

            public EventChannel.IEventSink EventSink => _events;
        }

        readonly MethodChannel _methodChannelIncoming;
        readonly MethodCallHandler _methodCallHandlerIncoming;
        readonly MethodChannel _methodChannelTest;
        readonly MethodCallHandler _methodCallHandlerTest;
        readonly EventChannel _eventChannel;
        readonly StreamHandler _streamHandler;

        Android.Content.Context _context;
        bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlutnetBridge"/> class.
        /// </summary>
        public FlutnetBridge(FlutterEngine engine, Android.Content.Context context) : this(engine, context, FlutnetBridgeMode.PlatformChannel)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlutnetBridge"/> class
        /// specifying how platform code and Flutter code communicate.
        /// </summary>
        public FlutnetBridge(FlutterEngine engine, Android.Content.Context context, FlutnetBridgeMode mode)
        {
            // Create the named channel for communicating with Flutter module using asynchronous method calls
            // NOTE: This channel is used to RECEIVE messages/requests FROM Flutter
            _methodChannelIncoming = new MethodChannel(engine.DartExecutor.BinaryMessenger, "flutnetbridge.incoming");
            _methodCallHandlerIncoming = new MethodCallHandler(HandleMethodCall);
            _methodChannelIncoming.SetMethodCallHandler(_methodCallHandlerIncoming);

            // Create a second named channel for diagnostic use only.
            // This channel is used, for example, to test if Flutter module is running
            // embedded into a native Xamarin app or as a standalone app
            _methodChannelTest = new MethodChannel(engine.DartExecutor.BinaryMessenger, "flutnetbridge.support");
            _methodCallHandlerTest = new MethodCallHandler(HandleMethodCallTest);
            _methodChannelTest.SetMethodCallHandler(_methodCallHandlerTest);

            // Create the named channel for communicating with Flutter module using event streams
            // NOTE: This channel is used to SEND messages/notifications TO Flutter

            // An event channel is a specialized platform channel intended for the use case of exposing platform events to Flutter as a Dart stream.
            // The Flutter SDK currently has no support for the symmetrical case of exposing Dart streams to platform code, though that could be built, if the need arises.
            // see: https://medium.com/flutter/flutter-platform-channels-ce7f540a104e

            _streamHandler = new StreamHandler(this);
            _eventChannel = new EventChannel(engine.DartExecutor.BinaryMessenger, "flutnetbridge.outgoing");
            _eventChannel.SetStreamHandler(_streamHandler);

            _context = context;
            Mode = mode;

            FlutnetRuntime.OnPlatformEvent += FlutnetRuntimeOnPlatformEvent;

            if (Mode == FlutnetBridgeMode.WebSocket) 
                _context.StartService(new Android.Content.Intent(_context, typeof(FlutnetWebSocketService)));
        }

        /// <summary>
        /// Releases all resources used by this object.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            FlutnetRuntime.OnPlatformEvent -= FlutnetRuntimeOnPlatformEvent;

            _methodChannelIncoming.Dispose();
            _methodCallHandlerIncoming.Dispose();
            _methodChannelTest.Dispose();
            _methodCallHandlerTest.Dispose();
            _eventChannel.Dispose();
            _streamHandler.Dispose();

            if (Mode == FlutnetBridgeMode.WebSocket)
                _context.StopService(new Android.Content.Intent(_context, typeof(FlutnetWebSocketService)));

            _disposed = true;
        }

        /// <summary>
        /// Specifies how platform code and Flutter code communicate.
        /// </summary>
        public FlutnetBridgeMode Mode { get; }

        private void HandleMethodCallTest(MethodCall call, MethodChannel.IResult result)
        {
            if (call.Method == "FlutnetBridgeMode")
            {
                switch (Mode)
                {
                    case FlutnetBridgeMode.PlatformChannel:
                        result.Success("PlatformChannel");
                        break;
                    case FlutnetBridgeMode.WebSocket:
                        result.Success("WebSocket");
                        break;
                }
            }
            else
            {
                // Right now this handler is called just once at application startup
                // when Flutter module tries to detect if it's running
                // embedded into a native Xamarin app or as a standalone app
                result.Success("ok");
            }
        }

        private void HandleMethodCall(MethodCall call, MethodChannel.IResult result)
        {
            // Return an error if Flutter is invoking method calls through method channel
            // when bridge is configured for WebSocket communication
            if (Mode == FlutnetBridgeMode.WebSocket)
            {
                result.NotImplemented();
                return;
            }

            // Extract target method information from MethodCall.Method
            FlutnetMethodInfo methodInfo;
            Object dartReturnValue;
            try
            {
                methodInfo = JsonConvert.DeserializeObject<FlutnetMethodInfo>(call.Method, FlutterInterop.JsonSerializerSettings); 
                dartReturnValue = FlutterInterop.ToMethodChannelResult(0);
            }
            catch (Exception ex)
            {
                result.Error(FlutnetErrorCode.OperationNotImplemented.ToString(), ex.Message, null);
                return;
            }

            // Send an empty - successful - response to immediately free Flutter thread
            result.Success(dartReturnValue);
            Task.Run(() => { BackgroundHandleMethodCall(methodInfo, call); });
        }

        private void BackgroundHandleMethodCall(FlutnetMethodInfo methodInfo, MethodCall call)
        {
            FlutnetRuntime.OperationInfo operation;
            try
            {
                operation = FlutnetRuntime.GetOperation(methodInfo.Instance, methodInfo.Operation);
            }
            catch (Exception)
            {
                SendError(methodInfo, new FlutnetException(FlutnetErrorCode.OperationNotImplemented));
                return;
            }

            if (operation.Parameters.Length > 0 && call.Arguments() == null)
            {
                SendError(methodInfo, new FlutnetException(FlutnetErrorCode.OperationArgumentCountMismatch));
                return;
            }

            object[] arguments = new object[operation.Parameters.Length];
            try
            {
                for (int i = 0; i < operation.Parameters.Length; i++)
                {
                    ParameterInfo param = operation.Parameters[i];
                    Type paramType = param.IsOut || param.ParameterType.IsByRef
                        ? param.ParameterType.GetElementType()
                        : param.ParameterType;
                    string paramName = param.Name.FirstCharUpper();

                    object value;
                    if (call.HasArgument(paramName))
                    {
                        Object argumentValue = call.Argument(paramName);

                        string serializedArg = argumentValue.ToString();

                        // Deserialize the argument
                        value = JsonConvert.DeserializeObject(serializedArg, paramType, FlutterInterop.JsonSerializerSettings);
                    }
                    else if (param.HasDefaultValue)
                    {
                        value = param.DefaultValue;
                    }
                    else
                    {
                        SendError(methodInfo, new FlutnetException(FlutnetErrorCode.InvalidOperationArguments));
                        return;
                    }

                    arguments[i] = value;
                }
            }
            catch (Exception)
            {
                SendError(methodInfo, new FlutnetException(FlutnetErrorCode.OperationArgumentParsingError));
                return;
            }

            PlatformOperationResult result = PlatformOperationRunner.Run(operation, arguments);
            if (result.Error != null)
            {
                if (result.Error is PlatformOperationException flutterException)
                {
                    SendError(methodInfo, flutterException);
                }
                else
                {
                    //In case of an unhandled exception, send to Flutter a verbose error message for better diagnostic
                    FlutnetException error = new FlutnetException(FlutnetErrorCode.OperationFailed, result.Error.ToStringCleared(), result.Error);
                    SendError(methodInfo, error);
                }
            }
            else
            {
                SendResult(methodInfo, result.Result);
            }
        }

        private void FlutnetRuntimeOnPlatformEvent(object sender, OnPlatformEventArgs e)
        {
            // Prevent dispatching events to Flutter through event channel
            // if bridge is configured for WebSocket communication
            if (Mode == FlutnetBridgeMode.WebSocket)
                return;

            FlutnetEventInfo eventInfo = new FlutnetEventInfo
            {
                InstanceId = e.ServiceName,
                EventName = e.EventName.FirstCharLower(),
                EventData = e.EventData
            };

            Object eventValue = FlutterInterop.ToMethodChannelResult(eventInfo);

            if (!MainThread.IsMainThread)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        _streamHandler.EventSink?.Success(eventValue);
                    }
                    catch (Exception ex)
                    {
                        // TODO: Properly log any error
                        Console.WriteLine(ex.Message);
                    }
                });
            }
            else
            {
                try
                {
                    _streamHandler.EventSink?.Success(eventValue);
                }
                catch (Exception ex)
                {
                    // TODO: Properly log any error
                    Console.WriteLine(ex.Message);
                }
            }

        }

        private void SendResult(FlutnetMethodInfo methodInfo, object result)
        {
            Dictionary<string, object> resultValue = new Dictionary<string, object>();
            resultValue.Add("ReturnValue", result);

            FlutnetMessage message = new FlutnetMessage
            {
                MethodInfo = methodInfo,
                Result = resultValue
            };

            Object dartReturnValue = FlutterInterop.ToMethodChannelResult(message);
            Console.WriteLine("Sending result to Flutter...");
            MainThread.BeginInvokeOnMainThread(() => _methodChannelIncoming.InvokeMethod("result", dartReturnValue));
        }

        private void SendError(FlutnetMethodInfo methodInfo, PlatformOperationException exception)
        {
            FlutnetMessage message = new FlutnetMessage
            {
                MethodInfo = methodInfo,
                // NOTE: Please consider removing ErrorCode and ErrorMessage
                ErrorCode = FlutnetErrorCode.OperationFailed,
                ErrorMessage = exception.Message,
                Exception = exception
            };

            Object dartReturnValue = FlutterInterop.ToMethodChannelResult(message);
            Console.WriteLine("Sending error to Flutter...");
            MainThread.BeginInvokeOnMainThread(() => _methodChannelIncoming.InvokeMethod("error", dartReturnValue));
        }
    }
}