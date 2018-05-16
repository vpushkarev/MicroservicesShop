using System;
using System.Collections.Generic;
using System.Linq;

namespace ShoppingCart.ShoppingCart
{
    using EventFeed;

    public class ShoppingCart
    {
        private HashSet<ShoppingCartItem> items = new HashSet<ShoppingCartItem>();

        public int UserId { get; }
        public IEnumerable<ShoppingCartItem> Items { get { return items; } }

        public ShoppingCart(int userId)
        {
            this.UserId = userId;
        }

        public ShoppingCart(int userId, IEnumerable<ShoppingCartItem> items)
        {
            this.UserId = userId;
            foreach (var item in items)
            {
                this.items.Add(item);
            }
        }

        /// <summary>
        /// Генерация события ADD
        /// </summary>
        public void AddItems(
          IEnumerable<ShoppingCartItem> shoppingCartItems,
          IEventStore eventStore)
        {
            foreach (var item in shoppingCartItems)
                if (this.items.Add(item))
                    eventStore.Raise(
                      "ShoppingCartItemAdded",
                      new { UserId, item });
        }

        /// <summary>
        /// Генерация события Remove
        /// </summary>
        public void RemoveItems(
          int[] productCatalogueIds,
          IEventStore eventStore)
        {
            items.RemoveWhere(i => productCatalogueIds.Contains(i.ProductCatalogueId));
        }

        /// <summary>
        /// Сохранение данных событий в БД (не завершенное!)
        /// </summary>
        /*public void Raise(string eventName, object content)
        {
            var seqNumber = database.NextSequenceNumber();
            database.Add(
                new Event(
                    seqNumber,
                    DateTimeOffset.UtcNow,
                    eventName,
                    content
                ));
        }
        */
    }
}
