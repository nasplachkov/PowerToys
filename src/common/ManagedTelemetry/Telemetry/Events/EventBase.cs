// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.Tracing;

namespace Microsoft.PowerToys.Telemetry.Events
{
    /// <summary>
    /// A base class to implement properties that are common to all telemetry events.
    /// </summary>
    [EventData]
    public class EventBase
    {
        public bool UTCReplace_AppSessionGuid => true;
    }
}
