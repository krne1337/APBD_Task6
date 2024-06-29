using Microsoft.AspNetCore.Mvc;

namespace APBD_Task6.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WarehouseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly string _connectionString;

        public WarehouseController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost("add-product")]
        public IActionResult AddProduct([FromBody] ProductWarehouseRequest request)
        {
            if (request == null || request.IdProduct <= 0 || request.IdWarehouse <= 0 || request.Amount <= 0)
                return BadRequest("Invalid input");

            var product = _context.Products.Find(request.IdProduct);
            var warehouse = _context.Warehouses.Find(request.IdWarehouse);

            if (product == null || warehouse == null)
                return BadRequest("Invalid Product or Warehouse");

            var order = _context.Orders
                .Where(o => o.IdProduct == request.IdProduct && o.Amount == request.Amount && o.CreatedAt < request.CreatedAt)
                .FirstOrDefault();

            if (order == null || _context.ProductWarehouses.Any(pw => pw.IdOrder == order.IdOrder))
                return BadRequest("No matching order found or order already fulfilled");

            order.FulfilledAt = DateTime.Now;
            _context.SaveChanges();

            var productWarehouse = new ProductWarehouse
            {
                IdProduct = request.IdProduct,
                IdWarehouse = request.IdWarehouse,
                IdOrder = order.IdOrder,
                Amount = request.Amount,
                Price = product.Price * request.Amount,
                CreatedAt = DateTime.Now
            };

            _context.ProductWarehouses.Add(productWarehouse);
            _context.SaveChanges();

            return Ok(productWarehouse.IdProductWarehouse);
        }

        [HttpPost("add-product-sp")]
        public IActionResult AddProductUsingSP([FromBody] ProductWarehouseRequest request)
        {
            if (request == null || request.IdProduct <= 0 || request.IdWarehouse <= 0 || request.Amount <= 0)
                return BadRequest("Invalid input");

            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("AddProductToWarehouse", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@IdProduct", request.IdProduct);
                    command.Parameters.AddWithValue("@IdWarehouse", request.IdWarehouse);
                    command.Parameters.AddWithValue("@Amount", request.Amount);
                    command.Parameters.AddWithValue("@CreatedAt", request.CreatedAt);

                    connection.Open();
                    try
                    {
                        var newId = command.ExecuteScalar();
                        return Ok(newId);
                    }
                    catch (SqlException ex)
                    {
                        return StatusCode(500, ex.Message);
                    }
                }
            }
        }
    }