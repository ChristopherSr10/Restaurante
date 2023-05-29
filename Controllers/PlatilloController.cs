using System;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using pruebarestaurante.Models;
using MySql.Data.MySqlClient;

namespace pruebarestaurante.Controllers
{
    public class PlatilloController : Controller
    {
        private readonly IConfiguration _conf;

        public PlatilloController(IConfiguration conf)
        {
            _conf = conf;
        }

        public IActionResult Index()
        {
            DataTable tbl = new DataTable();
            using (MySqlConnection cnx = new MySqlConnection(_conf.GetConnectionString("DevConnection")))
            {
                cnx.Open();
                string query = "SELECT * FROM platillo";
                MySqlDataAdapter adp = new MySqlDataAdapter(query, cnx);
                adp.Fill(tbl);
                cnx.Close();
            }
            return View(tbl);
        }

        [HttpPost]
public IActionResult Guardar(PlatilloViewModel platillo)
{
    if (ModelState.IsValid)
    {
        using (MySqlConnection cnx = new MySqlConnection(_conf.GetConnectionString("DevConnection")))
        {
            cnx.Open();
            string query = "INSERT INTO platillo (nombrePlatillo, precioPlatillo, descripcionPlatillo) VALUES (@nombrePlatillo, @precioPlatillo, @descripcionPlatillo)";
            MySqlCommand cmd = new MySqlCommand(query, cnx);
            cmd.Parameters.AddWithValue("@nombrePlatillo", platillo.nombrePlatillo);
            cmd.Parameters.AddWithValue("@precioPlatillo", platillo.precioPlatillo);
            cmd.Parameters.AddWithValue("@descripcionPlatillo", platillo.descripcionPlatillo);
            cmd.Parameters.AddWithValue("@idPlatillo", platillo.idPlatillo);
            cmd.ExecuteNonQuery();
            cnx.Close();
        }

        return RedirectToAction("Index");
    }

    return View(platillo);
}

        public IActionResult Editar(int id)
        {
            using (MySqlConnection cnx = new MySqlConnection(_conf.GetConnectionString("DevConnection")))
            {
                cnx.Open();
                string query = "SELECT * FROM platillo WHERE idPlatillo = @idPlatillo";
                MySqlCommand cmd = new MySqlCommand(query, cnx);
                cmd.Parameters.AddWithValue("@idPlatillo", id);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        PlatilloViewModel platillo = new PlatilloViewModel
                        {
                            idPlatillo = Convert.ToInt32(reader["idPlatillo"]),
                            nombrePlatillo = reader["nombrePlatillo"].ToString(),
                            precioPlatillo = Convert.ToSingle(reader["precioPlatillo"]),
                            descripcionPlatillo = reader["descripcionPlatillo"].ToString(),
                        };

                        return View(platillo);
                    }
                }
            }

            return NotFound();
        }

        [HttpPost]
        public IActionResult Actualizar(PlatilloViewModel platillo)
        {
            if (ModelState.IsValid)
            {
                using (MySqlConnection cnx = new MySqlConnection(_conf.GetConnectionString("DevConnection")))
                {
                    cnx.Open();
                    string query = "UPDATE platillo SET nombrePlatillo = @nombrePlatillo, precioPlatillo = @precioPlatillo, descripcionPlatillo = @descripcionPlatillo WHERE idPlatillo = @idPlatillo";
                    MySqlCommand cmd = new MySqlCommand(query, cnx);
                    cmd.Parameters.AddWithValue("@nombrePlatillo", platillo.nombrePlatillo);
                    cmd.Parameters.AddWithValue("@precioPlatillo", platillo.precioPlatillo);
                    cmd.Parameters.AddWithValue("@descripcionPlatillo", platillo.descripcionPlatillo);
                    cmd.Parameters.AddWithValue("@idPlatillo", platillo.idPlatillo);
                    cmd.ExecuteNonQuery();
                    cnx.Close();
                }

                return RedirectToAction("Index");
            }

            return View(platillo);
        }

        public IActionResult Crear()
        {
            return View();
        }

        public IActionResult Eliminar(int id)
{
    using (MySqlConnection cnx = new MySqlConnection(_conf.GetConnectionString("DevConnection")))
    {
        cnx.Open();
        
        // Eliminar los registros relacionados en la tabla "platillo_ingrediente"
        string deleteQuery = "DELETE FROM platillo_ingrediente WHERE idPlatillo = @idPlatillo";
        MySqlCommand deleteCmd = new MySqlCommand(deleteQuery, cnx);
        deleteCmd.Parameters.AddWithValue("@idPlatillo", id);
        deleteCmd.ExecuteNonQuery();

        // Eliminar el platillo
        string query = "DELETE FROM platillo WHERE idPlatillo = @idPlatillo";
        MySqlCommand cmd = new MySqlCommand(query, cnx);
        cmd.Parameters.AddWithValue("@idPlatillo", id);
        cmd.ExecuteNonQuery();
        
        cnx.Close();
    }

    return RedirectToAction("Index");
}

    }
}
