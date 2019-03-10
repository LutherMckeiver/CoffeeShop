﻿using CoffeeShop.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoffeeShop.Models
{
    public class ShoppingCart
    {
        private readonly CoffeeShopDbContext _context;

        private ShoppingCart(CoffeeShopDbContext context)
        {
            _context = context;
        }

        public string ShoppingCartId { get; set; }

        public List<ShoppingCartItem> ShoppingCartItems { get; set; }

        /// <summary>
        /// acesses coffee shop db 
        /// </summary>
        /// <param name="services"></param>
        /// <returns> returns new shopping cart</returns>
        public static ShoppingCart GetCart(IServiceProvider services)
        {
            ISession session = services.GetRequiredService<IHttpContextAccessor>()?.HttpContext.Session;

            var context = services.GetService<CoffeeShopDbContext>();
            string cartId = session.GetString("CartId") ?? Guid.NewGuid().ToString();

            session.SetString("CartId", cartId);

            return new ShoppingCart(context) { ShoppingCartId = cartId };
        }
        /// <summary>
        /// allows user to add item to cart
        /// </summary>
        /// <param name="coffee"> the specific coffee selected</param>
        /// <param name="amount"> the quantity of the coffee selected</param>
        public void AddToCart(Coffee coffee, int amount)
        {
            var shoppingCartItem = _context.ShoppingCartItems.SingleOrDefault(s => s.Coffee.ID == coffee.ID && s.ShoppingCartId == ShoppingCartId);

            if (shoppingCartItem == null)
            {
                shoppingCartItem = new ShoppingCartItem
                {
                    ShoppingCartId = ShoppingCartId,
                    Coffee = coffee,
                    Amount = 1
                };

                _context.ShoppingCartItems.Add(shoppingCartItem);
            }
            else
            {
                shoppingCartItem.Amount++;
            }
            _context.SaveChanges();
        }
        /// <summary>
        /// allows user to remove item from cart
        /// </summary>
        /// <param name="coffee"> the item to be removed from the cart</param>
        /// <returns> returns the cart with the specified item removed</returns>
        public int RemoveFromCart(Coffee coffee)
        {
            var shoppingCartItem = _context.ShoppingCartItems.SingleOrDefault(s => s.Coffee.ID == coffee.ID && s.ShoppingCartId == ShoppingCartId);

            int userAmount = 0;

            if (shoppingCartItem != null)
            {
                if (shoppingCartItem.Amount > 1)
                {
                    shoppingCartItem.Amount--;
                    userAmount = shoppingCartItem.Amount;
                }
                else
                {
                    _context.ShoppingCartItems.Remove(shoppingCartItem);
                }

            }
            _context.SaveChanges();
            return userAmount;
        }
        /// <summary>
        /// gets shopping cart items
        /// </summary>
        /// <returns> returns cart items</returns>
        public List<ShoppingCartItem> GetShoppingCartItems()
        {
            return ShoppingCartItems ?? 
                (ShoppingCartItems = _context.ShoppingCartItems.Where(c => c.ShoppingCartId == ShoppingCartId)
                .Include(s => s.Coffee).ToList());

        }

        public decimal GetTotal()
        {
             var total = _context.ShoppingCartItems.Where(c => c.ShoppingCartId == ShoppingCartId).Select(c => c.Amount).Sum();
             return total;
        } 

    }

}
