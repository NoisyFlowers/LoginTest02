using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ArcGIS.Core.Events;
using ArcGIS.Desktop.Framework;

namespace LoginTest02
{

    public class UserSelectionFinishedEvent : CompositePresentationEvent<UserSelectionFinishedEventArgs>
    {
        public static SubscriptionToken Subscribe(Action<UserSelectionFinishedEventArgs> action, bool keepSubscriberAlive = false)
        {
            return FrameworkApplication.EventAggregator.GetEvent<UserSelectionFinishedEvent>().Register(action, keepSubscriberAlive);
        }
        public static void Unsubscribe(Action<UserSelectionFinishedEventArgs> action)
        {
            FrameworkApplication.EventAggregator.GetEvent<UserSelectionFinishedEvent>().Unregister(action);
        }

        public static void Unsubscribe(SubscriptionToken token)
        {
            FrameworkApplication.EventAggregator.GetEvent<UserSelectionFinishedEvent>().Unregister(token);
        }

        internal static void Publish(UserSelectionFinishedEventArgs activeMapViewEventArgs)
        {
            FrameworkApplication.EventAggregator.GetEvent<UserSelectionFinishedEvent>().Broadcast(activeMapViewEventArgs);
        }
    }
}
