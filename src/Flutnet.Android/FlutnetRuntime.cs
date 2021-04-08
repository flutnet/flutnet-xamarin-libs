using System;
using System.Collections.Concurrent;
using System.Reflection;
using Flutnet.Data;
using Flutnet.ServiceModel;
using Flutnet.Utilities;

namespace Flutnet
{
    /// <summary>
    /// Runtime where you can store your PlatformService.
    /// </summary>
    public static class FlutnetRuntime
    {
        #region Initialization

        internal const string EnvironmentNotInitializedMsg = "The Flutnet environment has not been initialized yet. Please call FlutnetRuntime.Init().";

        /// <summary>
        /// Initializes the Flutnet environment.
        /// </summary>
        public static void Init(string appKey = null)
        {
            Initialized = true;
        }

        internal static  bool Initialized { get; private set; }

        internal static void EnsureInitialized()
        {
            if (!Initialized)
                throw new FlutnetException(FlutnetErrorCode.EnvironmentNotInitialized, EnvironmentNotInitializedMsg);
        }

        #endregion

        #region Registry

        static readonly ConcurrentDictionary<string, ContextInfo> _contexts = new ConcurrentDictionary<string, ContextInfo>();

        internal class ContextInfo
        {
            readonly ConcurrentDictionary<string, ServiceInfo> _services = new ConcurrentDictionary<string, ServiceInfo>();

            public ContextInfo(string name)
            {
                ContextName = name;
            }

            public string ContextName { get; }

            public bool TryAddService(ServiceInfo service)
            {
                return _services.TryAdd(service.ServiceInstanceName, service);
            }

            public bool TryRemoveService(string serviceName)
            {
                return _services.TryRemove(serviceName, out _);
            }

            public bool TryGetService(string name, out ServiceInfo service)
            {
                return _services.TryGetValue(name, out service);
            }
        }

        internal class ServiceEventReceiver
        {
            readonly Action<object,EventArgs> _handle;

            public ServiceEventReceiver(Action<object, EventArgs> handle)
            {
                _handle = handle;
            }

            [Obfuscation(Exclude = true)]
            private void Handle(object sender, EventArgs args)
            {
                _handle.Invoke(sender,args);
            }
        }

        internal class ServiceInfo
        {
            private readonly Type _type;
            private readonly object _instance;

            readonly ConcurrentDictionary<string, OperationInfo> _operations = new ConcurrentDictionary<string, OperationInfo>();

            public ServiceInfo(Type type, string instanceName, object instance = null)
            {
                ServiceInstanceName = instanceName;
                _type = type;
                _instance = instance;

                foreach (Type typedef in type.GetPlatformServiceTypeDefinitions())
                {
                    foreach (MethodInfo method in typedef.GetPlatformOperations())
                    {
                        OperationInfo operation = new OperationInfo(method, instance);
                        _operations.TryAdd(operation.OperationName, operation);
                    }
                }
            }

            public string ServiceInstanceName { get; }

            public bool TryGetOperation(string name, out OperationInfo operation)
            {
                return _operations.TryGetValue(name, out operation);
            }

            readonly ConcurrentDictionary<string, Delegate> _events = new ConcurrentDictionary<string, Delegate>();

            public void SubscribeToEvents()
            {
                if (_instance == null)
                    return;

                // Method on class ServiceEventReceiver that will be used as handler for all the events
                MethodInfo handleMethod = typeof(ServiceEventReceiver).GetMethod("Handle", BindingFlags.NonPublic | BindingFlags.Instance);

                foreach (EventInfo eventInfo in _type.GetPlatformEvents())
                {
                    string eventName = eventInfo.Name;

                    ServiceEventReceiver receiver = new ServiceEventReceiver((sender, e) =>
                    {
                        PropagatePlatformEvent(ServiceInstanceName, eventName, sender, e);
                    });

                    // Create an handler for the instance event
                    Delegate delegateForEvent = Delegate.CreateDelegate(eventInfo.EventHandlerType, receiver, handleMethod);

                    // Register the handler in the object instance
                    eventInfo.AddEventHandler(_instance, delegateForEvent);

                    _events.TryAdd(eventName, delegateForEvent);

                    #region Some documentation

                    // Connect this service info with the instance event
                    //Delegate delegateForEvent = Delegate.CreateDelegate(eventInfo.EventHandlerType, this, handleMethod);
                    //eventInfo.AddEventHandler(instance, delegateForEvent);

                    //// Create an instance of the delegate. Using the overloads
                    //// of CreateDelegate that take MethodInfo is recommended.
                    //Delegate d = Delegate.CreateDelegate(eventInfo.EventHandlerType, this, handleMethod);

                    //// Get the "add" accessor of the event and invoke it late-
                    //// bound, passing in the delegate instance. This is equivalent
                    //// to using the += operator in C#, or AddHandler in Visual
                    //// Basic. The instance on which the "add" accessor is invoked
                    //// is the form; the arguments must be passed as an array.
                    //MethodInfo addHandler = eventInfo.GetAddMethod();
                    //Object[] addHandlerArgs = { d };
                    //addHandler.Invoke(instance, addHandlerArgs);

                    #endregion
                }
            }

            public void UnsubscribeFromEvents()
            {
                if (_instance == null)
                    return;

                foreach (EventInfo eventInfo in _type.GetPlatformEvents())
                {
                    bool exists = _events.TryRemove(eventInfo.Name, out Delegate delegateForEvent);
                    if (exists && delegateForEvent != null)
                    {
                        // Unregister the handler in the object instance
                        eventInfo.RemoveEventHandler(_instance, delegateForEvent);
                    }
                }
            }
        }

        internal class OperationInfo
        {
            internal OperationInfo(MethodInfo method, object serviceInstance = null)
            {
                OperationName = method.GetCSharpSignature();
                MethodName = method.Name;
                HasResult = method.ReturnType.Name != "Void";
                Parameters = method.GetParameters();
                OperationAttribute  = (PlatformOperationAttribute) method.GetCustomAttribute(typeof(PlatformOperationAttribute), true);
                IsAsyncTask = method.ReturnType.IsTask();

                if (method.IsStatic)
                {
                    if (HasResult)
                        DelegateWithResult = ExpressionTools.CreateLazyStaticMethodWithResult(method);
                    else
                        Delegate = ExpressionTools.CreateLazyStaticMethodWithNoResult(method);
                }
                else
                {
                    if (HasResult)
                        DelegateWithResult = ExpressionTools.CreateLazyMethodWithResult(serviceInstance, method);
                    else
                        Delegate = ExpressionTools.CreateLazyMethodWithNoResult(serviceInstance, method);
                }
            }

            public string OperationName { get; }

            public string MethodName { get; }

            public bool HasResult { get; }

            public bool IsAsyncTask { get; }

            public PlatformOperationAttribute OperationAttribute { get; }

            public Action<object[]> Delegate { get; }

            public Func<object[], object> DelegateWithResult { get; }

            public ParameterInfo[] Parameters { get; }
        }

        /// <summary>
        /// Creates a named class registration of a platform service instance.
        /// </summary>
        /// <param name="instance">Instance to register</param>
        /// <param name="name">Name of registration</param>
        public static void RegisterPlatformService(object instance, string name)
        {
            EnsureInitialized();

            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            Type type = instance.GetType();
            if (!type.IsValidPlatformService())
                throw new ArgumentException("Instance does not represent a valid platform service.", nameof(instance));

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Registration name cannot be null or empty.", nameof(name));

            // Right now use the default context
            string contextName = string.Empty;
            ContextInfo contextObj = _contexts.GetOrAdd(contextName, s => new ContextInfo(s));

            ServiceInfo service = new ServiceInfo(type, name, instance);
            if (!contextObj.TryAddService(service))
                throw new ArgumentException("A service has already been registered with the same name.", nameof(name));

            service.SubscribeToEvents();
        }

        /// <summary>
        /// Creates a named class registration of a static platform service.
        /// </summary>
        /// <param name="type">Type to register</param>
        /// <param name="name">Name of registration</param>
        public static void RegisterStaticPlatformService(Type type, string name)
        {
            EnsureInitialized();

            // This is a sufficient check since an abstract class cannot be sealed or static in C#
            if (!(type.IsAbstract && type.IsSealed))
                throw new ArgumentException("The provided type is not a static class.", nameof(type));

            object[] attributes = type.GetCustomAttributes(typeof(PlatformServiceAttribute), false);
            if (attributes.Length == 0)
                throw new ArgumentException("Service class must be decorated with PlatformService attribute.", nameof(type));

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Registration name cannot be null or empty.", nameof(name));

            // Right now use the default context
            string contextName = string.Empty;
            ContextInfo contextObj = _contexts.GetOrAdd(contextName, s => new ContextInfo(s));

            ServiceInfo service = new ServiceInfo(type, name);
            if (!contextObj.TryAddService(service))
                throw new ArgumentException("A service has already been registered with the same name.", nameof(name));
        }

        /// <summary>
        /// Removes a named class registration of a platform service.
        /// </summary>
        /// <param name="name">Name of registration</param>
        /// <returns>true if the registration is successfully found and removed; otherwise, false.</returns>
        public static bool UnregisterPlatformService(string name)
        {
            EnsureInitialized();

            if (string.IsNullOrEmpty(name))
                return false;

            // Right now use the default context
            string contextName = string.Empty;
            if (!_contexts.TryGetValue(contextName, out ContextInfo contextObj))
                return false;

            bool serviceExists = contextObj.TryGetService(name, out ServiceInfo service);

            if (serviceExists)
            {
                service?.UnsubscribeFromEvents();
            }

            return contextObj.TryRemoveService(name);
        }

        internal static OperationInfo GetOperation(string serviceName, string operation)
        {
            // Right now use the default context
            string contextName = string.Empty;
            if (!_contexts.TryGetValue(contextName, out ContextInfo contextObj) || 
                !contextObj.TryGetService(serviceName, out ServiceInfo serviceObj))
                throw new ArgumentException("No service registered with the specified name.", nameof(serviceName));

            if (!serviceObj.TryGetOperation(operation, out OperationInfo operationObj))
                throw new ArgumentException("Operation not found on the specified service.", nameof(operation));

            return operationObj;
        }

        /// <summary>
        /// Occurs when a .NET event that must be propagated to Flutter is raised.
        /// </summary>
        internal static event EventHandler<OnPlatformEventArgs> OnPlatformEvent;

        /// <summary>
        /// Called when a .NET event that must be propagated to Flutter is raised.
        /// This method propagates the event through <see cref="OnPlatformEvent"/>
        /// so that <see cref="FlutnetBridge"/> can subscribe and send data to Flutter.
        /// </summary>
        private static void PropagatePlatformEvent(string serviceName, string eventName, object sender, EventArgs eventArgs)
        {
            OnPlatformEventArgs args = new OnPlatformEventArgs
            {
                ServiceName = serviceName,
                EventName = eventName,
                EventData = eventArgs
            };

            OnPlatformEvent?.Invoke(sender, args);
        }

        #endregion
    }

    internal class OnPlatformEventArgs : EventArgs
    {
        public string ServiceName { get; set; }
        public string EventName { get; set; }
        public EventArgs EventData { get; set; }
    }
}