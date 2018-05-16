using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nancy;
using Nancy.ModelBinding;

namespace ShoppingCart.ShoppingCart
{
    using EventFeed;

    public class ShoppingCartModule : NancyModule
    {
        public ShoppingCartModule(
            IShoppingCartStore shoppingCartStore,
            IProductCatalogueClient productcatalog, 
            IEventStore eventStore) 
            : base("/shoppingcart")
        {
            Get("/userid:int", parameters =>
             {
                 var userId = (int)parameters.userid;
                 return shoppingCartStore.Get(userId);
             });
            Post("/{userid:int}/items", async parameters =>
            {
                var productcatalogIds = this.Bind<int[]>();
                var userId = (int)parameters.userid;

                var shoppingCart = await shoppingCartStore.Get(userId).ConfigureAwait(false);
                var shoppingCartItems = await productcatalog.GetShoppingCartItems(productcatalogIds).ConfigureAwait(false);

                shoppingCart.AddItems(shoppingCartItems, eventStore);
                await shoppingCartStore.Save(shoppingCart);

                return shoppingCart;
            });
            Delete("/{userid:int}/items", async parameters =>
            {
                var productCatalogIds = this.Bind<int[]>();
                var userId = (int)parameters.userid;

                var shoppingCart = await shoppingCartStore.Get(userId).ConfigureAwait(false);
                shoppingCart.RemoveItems(productCatalogIds, eventStore);
                await shoppingCartStore.Save(shoppingCart);

                return shoppingCart;
            });
        }
    }
}
