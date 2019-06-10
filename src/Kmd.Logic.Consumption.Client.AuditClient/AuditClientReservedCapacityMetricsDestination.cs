﻿using System;
using Kmd.Logic.Audit.Client;

namespace Kmd.Logic.Consumption.Client.AuditClient
{
    public class AuditClientReservedCapacityMetricsDestination : IReservedCapacityMetricsDestination
    {
        public static string Template { get; } = "Consumed {Amount} for {Meter} on resource {ResourceId} in subscription {SubscriptionId}";

        public static string GetDefaultSubOwnerContextName(string propertyName) => $"__Sub_{propertyName}";

        private readonly IAudit _audit;
        private readonly Func<string, string> _getSubOwnerContextName;

        public AuditClientReservedCapacityMetricsDestination(IAudit audit, Func<string, string> getSubOwnerContextName = null)
        {
            this._audit = audit ?? throw new ArgumentNullException(nameof(audit));
            this._getSubOwnerContextName = getSubOwnerContextName ?? GetDefaultSubOwnerContextName;
        }

        public IReservedCapacityMetricsDestination ForInternalContext(string propertyName, string value)
        {
            return new AuditClientReservedCapacityMetricsDestination(
               this._audit.ForContext(propertyName, value),
               this._getSubOwnerContextName);
        }

        public IReservedCapacityMetricsDestination ForSubscriptionOwnerContext(string propertyName, string value)
        {
            return new AuditClientReservedCapacityMetricsDestination(
                 this._audit.ForContext(this._getSubOwnerContextName(propertyName), value),
                 this._getSubOwnerContextName);
        }

        public void ReleaseCapacity(Guid subscriptionId, Guid resourceId, string meter, int amount, string reason = null)
        {
            var audit = reason == null
                       ? this._audit
                       : this._audit.ForContext("Reason", reason);

            audit = audit.ForContext("Capacity", "Release");

            audit.Write(
                Template,
                amount,
                meter,
                resourceId,
                subscriptionId);
        }

        public void ReserveCapacity(Guid subscriptionId, Guid resourceId, string meter, int amount, string reason = null)
        {
            var audit = reason == null
                       ? this._audit
                       : this._audit.ForContext("Reason", reason);

            audit = audit.ForContext("Capacity", "Reserve");

            audit.Write(
                Template,
                amount,
                meter,
                resourceId,
                subscriptionId);
        }
    }
}