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
using System.Threading;
using System.Threading.Tasks;
using Flutnet.Data;
using Flutnet.Utilities;

namespace Flutnet
{

    internal static class PlatformOperationRunner
    {
        /// <summary>
        /// Invoke a platform operation with the specified arguments.
        /// </summary>
        public static PlatformOperationResult Run(FlutnetRuntime.OperationInfo operation, object[] arguments)
        {
            // Check if the operation must be invoked on the main (UI) thread
            bool mainThreadRequired = false;

#if __ANDROID__
            mainThreadRequired = operation.OperationAttribute.AndroidMainThreadRequired;
#elif __IOS__
            mainThreadRequired = operation.OperationAttribute.IosMainThreadRequired;
#endif

            object operationResult = null;
            Exception operationError = null;
            
            // 1. Async call on UI Thread
            if (mainThreadRequired && operation.IsAsyncTask)
            {
                ManualResetEvent uiFinishEvent = new ManualResetEvent(false);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Task task = (Task) operation.DelegateWithResult.Invoke(arguments);
                    task.ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                        {
                            operationError = t.Exception?.GetBaseException();
                        }
                        else if (t.IsCanceled)
                        {
                            operationError = new FlutnetException(FlutnetErrorCode.OperationCanceled);
                        }
                        else
                        {
                            operationResult = t.TaskResult();
                        }
                        uiFinishEvent.Set();
                    });
                });
                uiFinishEvent.WaitOne();
            }
            // 2. Sync call on UI Thread
            else if (mainThreadRequired)
            {
                ManualResetEvent uiFinishEvent = new ManualResetEvent(false);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        if (operation.HasResult)
                        {
                            operationResult = operation.DelegateWithResult.Invoke(arguments);
                        }
                        else
                        {
                            operation.Delegate.Invoke(arguments);
                            operationResult = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        operationError = ex;
                    }
                    finally
                    {
                        uiFinishEvent.Set();
                    }
                });
                uiFinishEvent.WaitOne();
            }
            // 3. Async call on Background Thread
            else if (operation.IsAsyncTask)
            {
                ManualResetEvent taskFinishEvent = new ManualResetEvent(false);
                Task task = (Task) operation.DelegateWithResult.Invoke(arguments);
                task.ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        operationError = t.Exception?.GetBaseException();
                    }
                    else if (t.IsCanceled)
                    {
                        operationError = new FlutnetException(FlutnetErrorCode.OperationCanceled);
                    }
                    else
                    {
                        operationResult = t.TaskResult();
                    }
                    taskFinishEvent.Set();
                });
                taskFinishEvent.WaitOne();
            }
            // 4. Sync call on Background Thread
            else
            {
                try
                {
                    if (operation.HasResult)
                    {
                        operationResult = operation.DelegateWithResult.Invoke(arguments);
                    }
                    else
                    {
                        operation.Delegate.Invoke(arguments);
                        operationResult = null;
                    }
                }
                catch (Exception ex)
                {
                    operationError = ex;
                }
            }

            // Return the result
            return new PlatformOperationResult
            {
                Result = operationResult,
                Error = operationError
            };
        }
    }

    internal class PlatformOperationResult
    {
        public object Result { get; set; }
        public Exception Error { get; set; }
    }
}