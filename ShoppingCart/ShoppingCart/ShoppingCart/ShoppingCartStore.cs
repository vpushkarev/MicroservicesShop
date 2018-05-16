using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Npgsql;

namespace ShoppingCart.ShoppingCart
{
    public class ShoppingCartStore : IShoppingCartStore
    {
        private string connectionString = @"Host=localhost;Port=5432;User ID=postgres;Password=admin;Database=postgres;";

        private const string readitemsSql =
            @"select t2.* from public.""ShoppingCart"" as t1, public.""ShoppingCartItems"" as t2
where ""ShoppingCartId"" = t1.""ID""
and t1.""UserId""=@UserId";

        public async Task<ShoppingCart> Get(int userId)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                var items = await
                    conn.QueryAsync<ShoppingCartItem>(readitemsSql, new { UserId = userId });
                conn.Close();
                return new ShoppingCart(userId, items);
            }
        }

        private const string deleteAllForShoppingCartSql =
 @"delete item from public.""ShoppingCartItems"" item
inner join public.""ShoppingCart"" cart on item.""ShoppingCartId"" = cart.""ID""
and cart.""UserId""=@UserId";

        private const string addAllForShoppingCartSql =
  @"insert into public.""ShoppingCartItems""
(""ShoppingCartId"", ""ProducCatalogId"", ""ProductName"", 
""ProductDescription"", ""Amount"", ""Currency"")
values 
(@ShoppingCartId, @ProductCatalogId, @ProductName,v
@ProductDescription, @Amount, @Currency)";

        public async Task Save(ShoppingCart shoppingCart)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            using (var tx = conn.BeginTransaction())
            {
                await conn.ExecuteAsync(
                  deleteAllForShoppingCartSql,
                  new { UserId = shoppingCart.UserId },
                  tx).ConfigureAwait(false);
                await conn.ExecuteAsync(
                  addAllForShoppingCartSql,
                  shoppingCart.Items,
                  tx).ConfigureAwait(false);
            }
        }
    }
}
