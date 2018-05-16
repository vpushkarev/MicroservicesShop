namespace ShoppingCart.EventFeed
{
    using System;
    using System.Text;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading.Tasks;
    using Dapper;
    using Newtonsoft.Json;
    using global::EventStore.ClientAPI;

    public class EventStore : IEventStore
    {
        private static long currentSequenceNumber = 0;
        private static readonly IList<Event> database = new List<Event>();

    private const string connectionString =
     "ConnectTo=discover://admin:changeit@127.0.0.1:2112/";
    private IEventStoreConnection connection =
      EventStoreConnection.Create(connectionString);
      
    public async Task Raise(string eventName, object content)
    {
      await connection.ConnectAsync().ConfigureAwait(false);
      var contentJson = JsonConvert.SerializeObject(content);
      var metaDataJson =
        JsonConvert.SerializeObject(new EventMetadata
        {
          OccurredAt = DateTimeOffset.Now,
          EventName = eventName
        });

      var eventData = new EventData(
        Guid.NewGuid(),
        "ShoppingCartEvent",
        isJson: true,
        data: Encoding.UTF8.GetBytes(contentJson),
        metadata: Encoding.UTF8.GetBytes(metaDataJson)
      );

      await
        connection.AppendToStreamAsync(
          "ShoppingCart",
          ExpectedVersion.Any,
           eventData);
    }
    
    public async Task<IEnumerable<Event>> GetEvents(
      long firstEventSequenceNumber,
      long lastEventSequenceNumber)
    {
      await connection.ConnectAsync().ConfigureAwait(false);

      var result = await connection.ReadStreamEventsForwardAsync(
        "ShoppingCart",
        start:(int) firstEventSequenceNumber,
        count: (int) (lastEventSequenceNumber - firstEventSequenceNumber),
        resolveLinkTos: false).ConfigureAwait(false);

      return
        result.Events
          .Select(ev =>
            new
            {
              Content = JsonConvert.DeserializeObject(
                Encoding.UTF8.GetString(ev.Event.Data)),
              Metadata = JsonConvert.DeserializeObject<EventMetadata>(
                Encoding.UTF8.GetString(ev.Event.Data))
            })
          .Select((ev, i) =>
            new Event(
              i + firstEventSequenceNumber,
              ev.Metadata.OccurredAt,
              ev.Metadata.EventName,
              ev.Content));
    }
        
    private class EventMetadata
    {
      public DateTimeOffset OccurredAt { get; set; }
      public string EventName { get; set; }
    }

    }
}