﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShoppingCart.ShoppingCart;

namespace ShoppingCart.EventFeed
{
    public interface IEventStore
    {
        Task<IEnumerable<Event>> GetEvents(long firstEventSequenceNumber, long lastEventSequenceNumber);
        Task Raise(string eventName, object content);
    }
}
