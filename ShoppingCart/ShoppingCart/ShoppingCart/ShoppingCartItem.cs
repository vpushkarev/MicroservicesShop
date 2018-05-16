namespace ShoppingCart.ShoppingCart
{
    public class ShoppingCartItem
    {
        public int ProductCatalogueId { get; }
        /*public string ProductName { get; }
        public string ProductDescription { get; }
        public Money Price { get; }*/

        public ShoppingCartItem(
          int productCatalogueId/*,
          string productName,
          string productdescription,
          Money price*/)
        {
            this.ProductCatalogueId = productCatalogueId;
         /*   this.ProductName = productName;
            this.ProductDescription = productdescription;
            this.Price = price;*/
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var that = obj as ShoppingCartItem;
            return this.ProductCatalogueId.Equals(that.ProductCatalogueId);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return this.ProductCatalogueId.GetHashCode();
        }
    }
}
