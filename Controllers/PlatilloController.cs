using System;
using System.Collections.Generic;
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
public IActionResult Guardar(PlatilloIngredienteViewModel platillo, IngredienteViewModel[] ingredientes)
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
            cmd.ExecuteNonQuery();

            // Obtener el ID del platillo recién insertado
            long platilloId = cmd.LastInsertedId;

            // Insertar los ingredientes asociados al platillo
            foreach (var ingrediente in ingredientes)
            {
                string ingredienteQuery = "INSERT INTO ingrediente (nombreIngrediente, cantidadDisponible) VALUES (@nombreIngrediente, @cantidadDisponible)";
                MySqlCommand ingredienteCmd = new MySqlCommand(ingredienteQuery, cnx);
                ingredienteCmd.Parameters.AddWithValue("@nombreIngrediente", ingrediente.nombreIngrediente);
                ingredienteCmd.Parameters.AddWithValue("@cantidadDisponible", ingrediente.cantidadDisponible);
                ingredienteCmd.ExecuteNonQuery();

                // Obtener el ID del ingrediente recién insertado
                long ingredienteId = ingredienteCmd.LastInsertedId;

                // Insertar el registro en la tabla platillo_ingrediente
                string platilloIngredienteQuery = "INSERT INTO platillo_ingrediente (idPlatillo, idIngrediente, cantidadDisponible) VALUES (@idPlatillo, @idIngrediente, @cantidadDisponible)";
                MySqlCommand platilloIngredienteCmd = new MySqlCommand(platilloIngredienteQuery, cnx);
                platilloIngredienteCmd.Parameters.AddWithValue("@idPlatillo", platilloId);
                platilloIngredienteCmd.Parameters.AddWithValue("@idIngrediente", ingredienteId);
                platilloIngredienteCmd.Parameters.AddWithValue("@cantidadDisponible", ingrediente.cantidadDisponible);
                platilloIngredienteCmd.ExecuteNonQuery();
            }

            cnx.Close();
        }

        return RedirectToAction("Index");
    }

    return View(platillo);
}


        public IActionResult Crear()
        {
            var modelo = new PlatilloIngredienteViewModel
            {
                Ingredientes = new List<IngredienteViewModel>()
            };
            return View(modelo);
        }



        public IActionResult Editar(int id)
{
    using (MySqlConnection cnx = new MySqlConnection(_conf.GetConnectionString("DevConnection")))
    {
        cnx.Open();
        
        // Obtener los detalles del platillo
        string platilloQuery = "SELECT * FROM platillo WHERE idPlatillo = @idPlatillo";
        MySqlCommand platilloCmd = new MySqlCommand(platilloQuery, cnx);
        platilloCmd.Parameters.AddWithValue("@idPlatillo", id);
        
        PlatilloIngredienteViewModel platillo = null;

        using (MySqlDataReader platilloReader = platilloCmd.ExecuteReader())
        {
            if (platilloReader.Read())
            {
                platillo = new PlatilloIngredienteViewModel
                {
                    idPlatillo = Convert.ToInt32(platilloReader["idPlatillo"]),
                    nombrePlatillo = platilloReader["nombrePlatillo"].ToString(),
                    precioPlatillo = Convert.ToSingle(platilloReader["precioPlatillo"]),
                    descripcionPlatillo = platilloReader["descripcionPlatillo"].ToString(),
                    Ingredientes = new List<IngredienteViewModel>()
                };
            }
        }

        // Obtener los ingredientes asociados
        string ingredientesQuery = "SELECT * FROM ingrediente AS i INNER JOIN platillo_ingrediente AS pi ON i.idIngrediente = pi.idIngrediente WHERE pi.idPlatillo = @idPlatillo";
        MySqlCommand ingredientesCmd = new MySqlCommand(ingredientesQuery, cnx);
        ingredientesCmd.Parameters.AddWithValue("@idPlatillo", id);

        using (MySqlDataReader ingredientesReader = ingredientesCmd.ExecuteReader())
        {
            while (ingredientesReader.Read())
            {
                IngredienteViewModel ingrediente = new IngredienteViewModel
                {
                    idIngrediente = Convert.ToInt32(ingredientesReader["idIngrediente"]),
                    nombreIngrediente = ingredientesReader["nombreIngrediente"].ToString(),
                    cantidadDisponible = Convert.ToInt32(ingredientesReader["cantidadDisponible"])
                };

                platillo.Ingredientes.Add(ingrediente);
            }
        }

        cnx.Close();

        if (platillo != null)
        {
            return View(platillo);
        }
    }

    return NotFound();
}

[HttpPost]
public IActionResult Actualizar(PlatilloIngredienteViewModel platillo)
{
    if (ModelState.IsValid)
    {
        using (MySqlConnection cnx = new MySqlConnection(_conf.GetConnectionString("DevConnection")))
        {
            cnx.Open();
            var transaction = cnx.BeginTransaction();

            try
            {
                // Actualizar los detalles del platillo
                string updatePlatilloQuery = "UPDATE platillo SET nombrePlatillo = @nombrePlatillo, precioPlatillo = @precioPlatillo, descripcionPlatillo = @descripcionPlatillo WHERE idPlatillo = @idPlatillo";
                MySqlCommand updatePlatilloCmd = new MySqlCommand(updatePlatilloQuery, cnx, transaction);
                updatePlatilloCmd.Parameters.AddWithValue("@nombrePlatillo", platillo.nombrePlatillo);
                updatePlatilloCmd.Parameters.AddWithValue("@precioPlatillo", platillo.precioPlatillo);
                updatePlatilloCmd.Parameters.AddWithValue("@descripcionPlatillo", platillo.descripcionPlatillo);
                updatePlatilloCmd.Parameters.AddWithValue("@idPlatillo", platillo.idPlatillo);
                updatePlatilloCmd.ExecuteNonQuery();

                // Eliminar los ingredientes existentes asociados al platillo
                string deletePlatilloIngredienteQuery = "DELETE FROM platillo_ingrediente WHERE idPlatillo = @idPlatillo";
                MySqlCommand deletePlatilloIngredienteCmd = new MySqlCommand(deletePlatilloIngredienteQuery, cnx, transaction);
                deletePlatilloIngredienteCmd.Parameters.AddWithValue("@idPlatillo", platillo.idPlatillo);
                deletePlatilloIngredienteCmd.ExecuteNonQuery();

                // Insertar los nuevos ingredientes asociados al platillo
                foreach (var ingrediente in platillo.Ingredientes)
                {
                    // Insertar el ingrediente en la tabla "ingrediente" si es un ingrediente nuevo
                    if (ingrediente.idIngrediente == 0)
                    {
                        string insertIngredienteQuery = "INSERT INTO ingrediente (nombreIngrediente, cantidadDisponible) VALUES (@nombreIngrediente, @cantidadDisponible)";
                        MySqlCommand insertIngredienteCmd = new MySqlCommand(insertIngredienteQuery, cnx, transaction);
                        insertIngredienteCmd.Parameters.AddWithValue("@nombreIngrediente", ingrediente.nombreIngrediente);
                        insertIngredienteCmd.Parameters.AddWithValue("@cantidadDisponible", ingrediente.cantidadDisponible);
                        insertIngredienteCmd.ExecuteNonQuery();

                        // Obtener el ID del ingrediente recién insertado
                        long ingredienteId = insertIngredienteCmd.LastInsertedId;

                        // Insertar el registro en la tabla "platillo_ingrediente"
                        string insertPlatilloIngredienteQuery = "INSERT INTO platillo_ingrediente (idPlatillo, idIngrediente, cantidadDisponible) VALUES (@idPlatillo, @idIngrediente, @cantidadDisponible)";
                        MySqlCommand insertPlatilloIngredienteCmd = new MySqlCommand(insertPlatilloIngredienteQuery, cnx, transaction);
                        insertPlatilloIngredienteCmd.Parameters.AddWithValue("@idPlatillo", platillo.idPlatillo);
                        insertPlatilloIngredienteCmd.Parameters.AddWithValue("@idIngrediente", ingredienteId);
                        insertPlatilloIngredienteCmd.Parameters.AddWithValue("@cantidadDisponible", ingrediente.cantidadDisponible);
                        insertPlatilloIngredienteCmd.ExecuteNonQuery();
                    }
                    else // Actualizar el ingrediente existente en la tabla "ingrediente"
                    {
                        string updateIngredienteQuery = "UPDATE ingrediente SET nombreIngrediente = @nombreIngrediente, cantidadDisponible = @cantidadDisponible WHERE idIngrediente = @idIngrediente";
                        MySqlCommand updateIngredienteCmd = new MySqlCommand(updateIngredienteQuery, cnx, transaction);
                        updateIngredienteCmd.Parameters.AddWithValue("@nombreIngrediente", ingrediente.nombreIngrediente);
                        updateIngredienteCmd.Parameters.AddWithValue("@cantidadDisponible", ingrediente.cantidadDisponible);
                        updateIngredienteCmd.Parameters.AddWithValue("@idIngrediente", ingrediente.idIngrediente);
                        updateIngredienteCmd.ExecuteNonQuery();

                        // Insertar el registro en la tabla "platillo_ingrediente"
                        string insertPlatilloIngredienteQuery = "INSERT INTO platillo_ingrediente (idPlatillo, idIngrediente, cantidadDisponible) VALUES (@idPlatillo, @idIngrediente, @cantidadDisponible)";
                        MySqlCommand insertPlatilloIngredienteCmd = new MySqlCommand(insertPlatilloIngredienteQuery, cnx, transaction);
                        insertPlatilloIngredienteCmd.Parameters.AddWithValue("@idPlatillo", platillo.idPlatillo);
                        insertPlatilloIngredienteCmd.Parameters.AddWithValue("@idIngrediente", ingrediente.idIngrediente);
                        insertPlatilloIngredienteCmd.Parameters.AddWithValue("@cantidadDisponible", ingrediente.cantidadDisponible);
                        insertPlatilloIngredienteCmd.ExecuteNonQuery();
                    }
                }

                // Commit de la transacción
                transaction.Commit();

                // Redireccionar a la página de éxito
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Rollback de la transacción en caso de error
                transaction.Rollback();
                Console.WriteLine(ex.Message);
                // Manejar el error como desees (redireccionar a una página de error, mostrar un mensaje, etc.)
                return View("Error");
            }
        }
    }
    else
    {
        // El modelo no es válido, volver a mostrar el formulario con los mensajes de validación
        return View(platillo);
    }
}


       
public IActionResult Eliminar(int id)
{
    using (MySqlConnection cnx = new MySqlConnection(_conf.GetConnectionString("DevConnection")))
    {
        cnx.Open();

        // Obtener los IDs de los ingredientes asociados al platillo
        string ingredientesIdsQuery = "SELECT idIngrediente FROM platillo_ingrediente WHERE idPlatillo = @idPlatillo";
        MySqlCommand ingredientesIdsCmd = new MySqlCommand(ingredientesIdsQuery, cnx);
        ingredientesIdsCmd.Parameters.AddWithValue("@idPlatillo", id);

        List<int> ingredientesIds = new List<int>();

        using (MySqlDataReader ingredientesIdsReader = ingredientesIdsCmd.ExecuteReader())
        {
            while (ingredientesIdsReader.Read())
            {
                int ingredienteId = Convert.ToInt32(ingredientesIdsReader["idIngrediente"]);
                ingredientesIds.Add(ingredienteId);
            }
        }

        // Eliminar los registros relacionados en la tabla "platillo_ingrediente"
        string deletePlatilloIngredienteQuery = "DELETE FROM platillo_ingrediente WHERE idPlatillo = @idPlatillo";
        MySqlCommand deletePlatilloIngredienteCmd = new MySqlCommand(deletePlatilloIngredienteQuery, cnx);
        deletePlatilloIngredienteCmd.Parameters.AddWithValue("@idPlatillo", id);
        deletePlatilloIngredienteCmd.ExecuteNonQuery();

        // Eliminar el platillo
        string deletePlatilloQuery = "DELETE FROM platillo WHERE idPlatillo = @idPlatillo";
        MySqlCommand deletePlatilloCmd = new MySqlCommand(deletePlatilloQuery, cnx);
        deletePlatilloCmd.Parameters.AddWithValue("@idPlatillo", id);
        deletePlatilloCmd.ExecuteNonQuery();

        // Eliminar los ingredientes asociados al platillo de la tabla "ingrediente"
        foreach (int ingredienteId in ingredientesIds)
        {
            string deleteIngredienteQuery = "DELETE FROM ingrediente WHERE idIngrediente = @idIngrediente";
            MySqlCommand deleteIngredienteCmd = new MySqlCommand(deleteIngredienteQuery, cnx);
            deleteIngredienteCmd.Parameters.AddWithValue("@idIngrediente", ingredienteId);
            deleteIngredienteCmd.ExecuteNonQuery();
        }

        cnx.Close();
    }

    return RedirectToAction("Index");
}




    }
}
