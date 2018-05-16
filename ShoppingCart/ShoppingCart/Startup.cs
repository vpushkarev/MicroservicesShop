using Microsoft.AspNetCore.Builder;
using Nancy.Owin;

namespace ShoppingCart
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseOwin(buildFunc => buildFunc.UseNancy());
        }
    }
}
