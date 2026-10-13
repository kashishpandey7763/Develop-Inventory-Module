
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using InventoryManagement.Models;

namespace InventoryManagement.Controllers
{
    public class ProductController : Controller
    {
        private readonly string cs =
            ConfigurationManager.ConnectionStrings["InventoryConnection"]
            .ConnectionString;

        // READ: Display all products
        public ActionResult Index()
        {
            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlDataAdapter da = new SqlDataAdapter(
                    "SELECT * FROM Products ORDER BY ProductId DESC", con);

                da.Fill(dt);
            }

            return View(dt);
        }

        // CREATE: Display Add Product form
        public ActionResult Create()
        {
            return View();
        }

        // CREATE: Save new product
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Product p)
        {
            if (string.IsNullOrWhiteSpace(p.ProductName) ||
                p.Quantity < 0 || p.Price < 0)
            {
                ModelState.AddModelError("",
                    "Enter a product name and valid quantity and price.");
                return View(p);
            }

            using (SqlConnection con = new SqlConnection(cs))
            {
                string query = @"INSERT INTO Products
                    (ProductName, Quantity, Price)
                    VALUES (@name, @qty, @price)";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.Add("@name", SqlDbType.VarChar, 100)
                        .Value = p.ProductName.Trim();

                    cmd.Parameters.Add("@qty", SqlDbType.Int)
                        .Value = p.Quantity;

                    cmd.Parameters.Add("@price", SqlDbType.Decimal)
                        .Value = p.Price;

                    cmd.Parameters["@price"].Precision = 10;
                    cmd.Parameters["@price"].Scale = 2;

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index");
        }

        // UPDATE: Display existing product
        public ActionResult Edit(int id)
        {
            Product p = null;

            using (SqlConnection con = new SqlConnection(cs))
            {
                string query =
                    "SELECT * FROM Products WHERE ProductId = @id";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    con.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            p = new Product
                            {
                                ProductId = Convert.ToInt32(dr["ProductId"]),
                                ProductName = dr["ProductName"].ToString(),
                                Quantity = Convert.ToInt32(dr["Quantity"]),
                                Price = Convert.ToDecimal(dr["Price"])
                            };
                        }
                    }
                }
            }

            if (p == null)
                return HttpNotFound();

            return View(p);
        }

        // UPDATE: Save edited product
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Product p)
        {
            if (string.IsNullOrWhiteSpace(p.ProductName) ||
                p.Quantity < 0 || p.Price < 0)
            {
                ModelState.AddModelError("",
                    "Enter a product name and valid quantity and price.");
                return View(p);
            }

            using (SqlConnection con = new SqlConnection(cs))
            {
                string query = @"UPDATE Products
                    SET ProductName = @name,
                        Quantity = @qty,
                        Price = @price
                    WHERE ProductId = @id";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int)
                        .Value = p.ProductId;

                    cmd.Parameters.Add("@name", SqlDbType.VarChar, 100)
                        .Value = p.ProductName.Trim();

                    cmd.Parameters.Add("@qty", SqlDbType.Int)
                        .Value = p.Quantity;

                    cmd.Parameters.Add("@price", SqlDbType.Decimal)
                        .Value = p.Price;

                    cmd.Parameters["@price"].Precision = 10;
                    cmd.Parameters["@price"].Scale = 2;

                    con.Open();
                    int rows = cmd.ExecuteNonQuery();

                    if (rows == 0)
                        return HttpNotFound();
                }
            }

            return RedirectToAction("Index");
        }

        // DELETE: Remove product using POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                string query =
                    "DELETE FROM Products WHERE ProductId = @id";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index");
        }
    }
}
