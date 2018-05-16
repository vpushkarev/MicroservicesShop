using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Polly;

namespace ShoppingCart
{
    using ShoppingCart;
    using System.Net.Http.Headers;

    public class ProductCatalogClient : IProductCatalogueClient
    {
        /// <summary>
        /// Обращение к микросервису ProductCatalog обернули Стратегией повторной отправки с экспоненциальной отсрочкой отправки
        /// </summary>
        private static Policy exponentialRetryPolicy =
            Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                3,
                attempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)), (ex, _) => Console.WriteLine(ex.ToString()));

        private static string productCatalogBaseUrl = @"https://private-8ce8d1-productcatalog6.apiary-mock.com/products";
        /*@"http://private-05cc8-chapter2productcataloguemicroservice.apiary-mock.com";
  */      private static string getProductPathTemplate =
        "/products?productIds=[{0}]";

        private ICache cache;

        public ProductCatalogClient(ICache cache)
        {
            this.cache = cache;
        }

        public async Task<IEnumerable<ShoppingCartItem>> GetShoppingCartItems(int[] productCatalogIds) =>
        await exponentialRetryPolicy
        .ExecuteAsync(async () => await GetItemsFromCatalogueService(productCatalogIds).ConfigureAwait(false));

        /// <summary>
        /// Извлечение товаров и преобразование их в элементы корзины заказов
        /// </summary>
        private async Task<IEnumerable<ShoppingCartItem>> GetItemsFromCatalogueService(int[] productCatalogueIds)
        {
            var response = await RequestProductFromProductCatalogue(productCatalogueIds).ConfigureAwait(false);
            return await ConvertToShoppingCartItems(response).ConfigureAwait(false);
        }

        /// <summary>
        /// HTTP-запрос типа Get к микросервису Product Catalog
        /// </summary>
        private async Task<HttpResponseMessage> RequestProductFromProductCatalogue(int[] productcatalogueIds)
        {
            var productsResource = string.Format(getProductPathTemplate, string.Join(",", new int[]{ 1,2 }/*productcatalogueIds*/));
            var response = this.cache.Get(productsResource) as HttpResponseMessage;
            if (response == null)
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(productCatalogBaseUrl);
                    response = await httpClient
                        .GetAsync("previews / productcatalog6 / reference / 0 / products - collection / undefined")
                        .ConfigureAwait(false);
                    AddToCache(productsResource, response); 
                }
            }
            return response;
        }

        private void AddToCache(string resource, HttpResponseMessage response)
        {
            var cacheHeader = response
              .Headers
              .FirstOrDefault(h => h.Key == "cache-control");
            if (string.IsNullOrEmpty(cacheHeader.Key))
                return;
            var maxAge =
              CacheControlHeaderValue.Parse(cacheHeader.Value.ToString())
                .MaxAge;
            if (maxAge.HasValue)
                this.cache.Add(key: resource, value: response, ttl: maxAge.Value);
        }

        /// <summary>
        /// Десериализация и преобразование данных ответа
        /// </summary>
        private static async Task<IEnumerable<ShoppingCartItem>> ConvertToShoppingCartItems(HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();
            var products = 
                JsonConvert.DeserializeObject<List<ProductCatalogProduct>>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            return products
                .Select(p => new ShoppingCartItem(
                    int.Parse(p.ProductId)/*,
                    p.ProductName,
                    p.ProductDescription,
                    p.Price*/
                    ));
        }

        /// <summary>
        /// Данные по товарам
        /// </summary>
        private class ProductCatalogProduct
        {
            public string ProductId { get; set; }
            public string ProductName { get; set; }
            public string ProductDescription { get; set; }
            public Money Price { get; set; }
        }
    }
    
}

