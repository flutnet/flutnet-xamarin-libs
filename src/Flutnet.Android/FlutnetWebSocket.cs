using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Flutnet.Data;
using Flutnet.ServiceModel;
using Flutnet.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Flutnet
{
    internal class FlutnetWebSocket
    {
        readonly WebSocketServer _server;

        public FlutnetWebSocket()
        {
            _server = new WebSocketServer(12345);
            _server.WaitTime = TimeSpan.MaxValue;
            _server.AddWebSocketService<FlutnetWebSocketBehavior>("/flutter");
        }

        public void Start()
        {
            _server.Start();
        }

        public void Stop()
        {
            _server.Stop();
        }
    }

    internal class FlutnetWebSocketBehavior : WebSocketBehavior
    {
        readonly object _responseBufferLock = new object();
        readonly Queue<FlutnetMessage> _responseBufferQueue = new Queue<FlutnetMessage>(20);

        public FlutnetWebSocketBehavior()
        {
            FlutnetRuntime.OnPlatformEvent += FlutnetRuntimeOnPlatformEvent;
        }

        private void FlutnetRuntimeOnPlatformEvent(object sender, OnPlatformEventArgs e)
        {
            FlutnetEventInfo eventInfo = new FlutnetEventInfo
            {
                InstanceId = e.ServiceName,
                EventName = e.EventName.FirstCharLower(),
                EventData = e.EventData
            };

            FlutnetMessage message = new FlutnetMessage()
            {
                MethodInfo = new FlutnetMethodInfo
                {
                    RequestId = -1,
                    Instance = e.ServiceName
                },
                Result = null,
                EventInfo = eventInfo
            };

            Send(message);
        }

        private void AddToBuffer(FlutnetMessage message)
        {
            lock (_responseBufferLock)
            {
                FlutnetMessage found = _responseBufferQueue.FirstOrDefault(r => r.MethodInfo.RequestId == message.MethodInfo.RequestId);
                if (found == null)
                {
                    _responseBufferQueue.Enqueue(message);

                    if (_responseBufferQueue.Count > 20)
                    {
                        _responseBufferQueue.Dequeue();
                    }
                }
            }
        }

        private FlutnetMessage GetFromBuffer(int requestId)
        {
            lock (_responseBufferLock)
            {
                FlutnetMessage response = _responseBufferQueue.FirstOrDefault(r => r.MethodInfo.RequestId == requestId);
                return response;
            }
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            if (e.Data == null)
                return;

            if (e.Data != null)
            {
                FlutnetMessage request;
                try
                {
                    request = JsonConvert.DeserializeObject<FlutnetMessage>(e.Data, FlutterInterop.JsonSerializerSettings);
                }
                catch (Exception)
                {
                    // ignore invalid messages
                    return;
                }

                FlutnetRuntime.OperationInfo operation;
                try
                {
                    operation = FlutnetRuntime.GetOperation(request.MethodInfo.Instance, request.MethodInfo.Operation);
                }
                catch (Exception)
                {
                    FlutnetException error = new FlutnetException(FlutnetErrorCode.OperationNotImplemented);
                    SendError(request.MethodInfo, error);
                    return;
                }

                if (operation.Parameters.Length != request.Arguments.Count)
                {
                    FlutnetException error = new FlutnetException(FlutnetErrorCode.OperationArgumentCountMismatch);
                    SendError(request.MethodInfo, error);
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
                        if (request.Arguments.ContainsKey(paramName))
                        {
                            object argumentValue = request.Arguments[paramName];
                            if (argumentValue == null)
                            {
                                value = null;
                            }
                            else if (argumentValue.GetType() == paramType)
                            {
                                value = argumentValue;
                            }
                            else if (argumentValue is string && paramType != null && paramType.IsEnum)
                            {
                                // Handle enums: remove double quotes from "enumName"
                                string enumString = (argumentValue as string);
                                value = Enum.Parse(paramType, enumString);
                            }
                            else if (paramType != null && argumentValue.GetType().IsPrimitive && paramType.IsPrimitive)
                            {
                                value = Convert.ChangeType(argumentValue, paramType);
                            }
                            else
                            {
                                JObject jobj = JObject.FromObject(argumentValue);
                                value = jobj.ToObject(paramType);
                            }
                        }
                        else if (param.HasDefaultValue)
                        {
                            value = param.DefaultValue;
                        }
                        else
                        {
                            FlutnetException error = new FlutnetException(FlutnetErrorCode.InvalidOperationArguments);
                            SendError(request.MethodInfo, error);
                            return;
                        }

                        arguments[i] = value;
                    }
                }
                catch (Exception)
                {
                    FlutnetException error = new FlutnetException(FlutnetErrorCode.OperationArgumentParsingError);
                    SendError(request.MethodInfo, error);
                    return;
                }

                PlatformOperationResult result = PlatformOperationRunner.Run(operation, arguments);
                if (result.Error != null)
                {
                    if (result.Error is PlatformOperationException flutterException)
                    {
                        SendError(request.MethodInfo, flutterException);
                    }
                    else
                    {
                        //In case of an unhandled exception, send to Flutter a verbose error message for better diagnostic
                        FlutnetException error = new FlutnetException(FlutnetErrorCode.OperationFailed, result.Error.ToStringCleared(), result.Error);
                        SendError(request.MethodInfo, error);
                    }
                }
                else
                {
                    SendResult(request.MethodInfo, result.Result);
                }
            }
        }

        private void SendError(FlutnetMethodInfo methodInfo, PlatformOperationException exception)
        {
            FlutnetMessage message = new FlutnetMessage
            {
                MethodInfo = methodInfo,
                // NOTE: Please consider remove ErrorCode and ErrorMessage
                ErrorCode = FlutnetErrorCode.OperationFailed,
                ErrorMessage = exception.Message,
                Exception = exception
            };

            Send(message);
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

            Send(message);
        }

        private void Send(FlutnetMessage message)
        {
            try
            {
                // OLD VERSION
                //string json = JsonConvert.SerializeObject(message, FlutterInterop.JsonSerializerSettings);

                // NEW - FIX ISSUES ABOUT DICTIONARY IN FLUTTER
                JObject jsonObject = JObject.FromObject(message, FlutterInterop.Serializer);
                FlutterInterop.CleanObjectFromInvalidTypes(ref jsonObject);
                string json = jsonObject.ToString(Formatting.None);
                Send(json);
            }
            catch (Exception)
            {
                AddToBuffer(message);
            }
        }
    }
}