// using System;
// using System.Data;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Extensions.Configuration;
// using pruebarestaurante.Models;
// using MySql.Data.MySqlClient;

// namespace pruebarestaurante.Controllers
// {
//     public class IngredienteController : Controller
//     {
//         private readonly IConfiguration _conf;

//         public IngredienteController(IConfiguration conf)
//         {
//             _conf = conf;
//         }

//         public IActionResult Index()
//         {
//             DataTable tbl = new DataTable();
//             using (MySqlConnection cnx = new MySqlConnection(_conf.GetConnectionString("DevConnection")))
//             {
//                 cnx.Open();
//                 string query = "SELECT * FROM ingrediente";
//                 MySqlDataAdapter adp = new MySqlDataAdapter(query, cnx);
//                 adp.Fill(tbl);
//                 cnx.Close();
//             }
//             return View(tbl);
//         }

//         [HttpPost]
//         public IActionResult Guardar(PlatilloIngredienteViewModel ingrediente)
//         {
//             if (ModelState.IsValid)
//             {
//                 using (MySqlConnection cnx = new MySqlConnection(_conf.GetConnectionString("DevConnection")))
//                 {
//                     cnx.Open();
//                     string query = "INSERT INTO ingrediente (nombreIngrediente, cantidadDisponible) VALUES (@nombreIngrediente, @cantidadDisponible)";
//                     MySqlCommand cmd = new MySqlCommand(query, cnx);
//                     cmd.Parameters.AddWithValue("@nombreIngrediente", ingrediente.nombreIngrediente);
//                     cmd.Parameters.AddWithValue("@cantidadDisponible", ingrediente.cantidadDisponible);
//                     cmd.Parameters.AddWithValue("@idIngrediente", ingrediente.idIngrediente);
//                     cmd.ExecuteNonQuery();
//                     cnx.Close();
//                 }

//                 return RedirectToAction("Index");
//             }

//             return View(ingrediente);
//         }

//         public IActionResult Editar(int id)
//         {
//             using (MySqlConnection cnx = new MySqlConnection(_conf.GetConnectionString("DevConnection")))
//             {
//                 cnx.Open();
//                 string query = "SELECT * FROM ingrediente WHERE idIngrediente = @idIngrediente";
//                 MySqlCommand cmd = new MySqlCommand(query, cnx);
//                 cmd.Parameters.AddWithValue("@idIngrediente", id);
//                 using (MySqlDataReader reader = cmd.ExecuteReader())
//                 {
//                     if (reader.Read())
//                     {
//                         PlatilloIngredienteViewModel ingrediente = new PlatilloIngredienteViewModel
//                         {
//                             idIngrediente = Convert.ToInt32(reader["idIngrediente"]),
//                             nombreIngrediente = reader["nombreIngrediente"].ToString(),
//                             cantidadDisponible = Convert.ToInt32(reader["cantidadDisponible"]),
//                         };

//                         return View(ingrediente);
//                     }
//                 }
//             }

//             return NotFound();
//         }

//         [HttpPost]
//         public IActionResult Actualizar(PlatilloIngredienteViewModel ingrediente)
//         {
//             if (ModelState.IsValid)
//             {
//                 using (MySqlConnection cnx = new MySqlConnection(_conf.GetConnectionString("DevConnection")))
//                 {
//                     cnx.Open();
//                     string query = "UPDATE ingrediente SET nombreIngrediente = @nombreIngrediente, cantidadDisponible = @cantidadDisponible";
//                     MySqlCommand cmd = new MySqlCommand(query, cnx);
//                     cmd.Parameters.AddWithValue("@nombreIngrediente", ingrediente.nombreIngrediente);
//                     cmd.Parameters.AddWithValue("@cantidadDisponible", ingrediente.cantidadDisponible);
//                     cmd.ExecuteNonQuery();
//                     cnx.Close();
//                 }

//                 return RedirectToAction("Index");
//             }

//             return View(ingrediente);
//         }

//         public IActionResult Crear()
//         {
//             return View();
//         }

//         public IActionResult Eliminar(int id)
//         {
//             using (MySqlConnection cnx = new MySqlConnection(_conf.GetConnectionString("DevConnection")))
//             {
//                 cnx.Open();

//                 // Eliminar los registros relacionados en la tabla "platillo_ingrediente"
//                 string deleteQuery = "DELETE FROM platillo_ingrediente WHERE idIngrediente = @idIngrediente";
//                 MySqlCommand deleteCmd = new MySqlCommand(deleteQuery, cnx);
//                 deleteCmd.Parameters.AddWithValue("@idIngrediente", id);
//                 deleteCmd.ExecuteNonQuery();

//                 // Eliminar el ingrediente
//                 string query = "DELETE FROM ingrediente WHERE idIngrediente = @idIngrediente";
//                 MySqlCommand cmd = new MySqlCommand(query, cnx);
//                 cmd.Parameters.AddWithValue("@idIngrediente", id);
//                 cmd.ExecuteNonQuery();

//                 cnx.Close();
//             }

//             return RedirectToAction("Index");
//         }

//     }
// }
