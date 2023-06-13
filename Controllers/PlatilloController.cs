using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using pruebarestaurante.Models;
using MySql.Data.MySqlClient;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;


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
            return View();
        }

        //---------------------------------------------------------------MENU----------------------------------------------------------------------------------
        public IActionResult Menu()
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

                    long platilloId = cmd.LastInsertedId;

                    foreach (var ingrediente in ingredientes)
                    {
                        string ingredienteQuery = "INSERT INTO ingrediente (nombreIngrediente, cantidadDisponible) VALUES (@nombreIngrediente, @cantidadDisponible)";
                        MySqlCommand ingredienteCmd = new MySqlCommand(ingredienteQuery, cnx);
                        ingredienteCmd.Parameters.AddWithValue("@nombreIngrediente", ingrediente.nombreIngrediente);
                        ingredienteCmd.Parameters.AddWithValue("@cantidadDisponible", ingrediente.cantidadDisponible);
                        ingredienteCmd.ExecuteNonQuery();

                        long ingredienteId = ingredienteCmd.LastInsertedId;

                        string platilloIngredienteQuery = "INSERT INTO platillo_ingrediente (idPlatillo, idIngrediente, cantidadDisponible) VALUES (@idPlatillo, @idIngrediente, @cantidadDisponible)";
                        MySqlCommand platilloIngredienteCmd = new MySqlCommand(platilloIngredienteQuery, cnx);
                        platilloIngredienteCmd.Parameters.AddWithValue("@idPlatillo", platilloId);
                        platilloIngredienteCmd.Parameters.AddWithValue("@idIngrediente", ingredienteId);
                        platilloIngredienteCmd.Parameters.AddWithValue("@cantidadDisponible", ingrediente.cantidadDisponible);
                        platilloIngredienteCmd.ExecuteNonQuery();
                    }

                    cnx.Close();
                }

                return RedirectToAction("Menu");
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
                        string updatePlatilloQuery = "UPDATE platillo SET nombrePlatillo = @nombrePlatillo, precioPlatillo = @precioPlatillo, descripcionPlatillo = @descripcionPlatillo WHERE idPlatillo = @idPlatillo";
                        MySqlCommand updatePlatilloCmd = new MySqlCommand(updatePlatilloQuery, cnx, transaction);
                        updatePlatilloCmd.Parameters.AddWithValue("@nombrePlatillo", platillo.nombrePlatillo);
                        updatePlatilloCmd.Parameters.AddWithValue("@precioPlatillo", platillo.precioPlatillo);
                        updatePlatilloCmd.Parameters.AddWithValue("@descripcionPlatillo", platillo.descripcionPlatillo);
                        updatePlatilloCmd.Parameters.AddWithValue("@idPlatillo", platillo.idPlatillo);
                        updatePlatilloCmd.ExecuteNonQuery();

                        string deletePlatilloIngredienteQuery = "DELETE FROM platillo_ingrediente WHERE idPlatillo = @idPlatillo";
                        MySqlCommand deletePlatilloIngredienteCmd = new MySqlCommand(deletePlatilloIngredienteQuery, cnx, transaction);
                        deletePlatilloIngredienteCmd.Parameters.AddWithValue("@idPlatillo", platillo.idPlatillo);
                        deletePlatilloIngredienteCmd.ExecuteNonQuery();

                        foreach (var ingrediente in platillo.Ingredientes)
                        {
                            if (ingrediente.idIngrediente == 0)
                            {
                                string insertIngredienteQuery = "INSERT INTO ingrediente (nombreIngrediente, cantidadDisponible) VALUES (@nombreIngrediente, @cantidadDisponible)";
                                MySqlCommand insertIngredienteCmd = new MySqlCommand(insertIngredienteQuery, cnx, transaction);
                                insertIngredienteCmd.Parameters.AddWithValue("@nombreIngrediente", ingrediente.nombreIngrediente);
                                insertIngredienteCmd.Parameters.AddWithValue("@cantidadDisponible", ingrediente.cantidadDisponible);
                                insertIngredienteCmd.ExecuteNonQuery();

                                long ingredienteId = insertIngredienteCmd.LastInsertedId;

                                string insertPlatilloIngredienteQuery = "INSERT INTO platillo_ingrediente (idPlatillo, idIngrediente, cantidadDisponible) VALUES (@idPlatillo, @idIngrediente, @cantidadDisponible)";
                                MySqlCommand insertPlatilloIngredienteCmd = new MySqlCommand(insertPlatilloIngredienteQuery, cnx, transaction);
                                insertPlatilloIngredienteCmd.Parameters.AddWithValue("@idPlatillo", platillo.idPlatillo);
                                insertPlatilloIngredienteCmd.Parameters.AddWithValue("@idIngrediente", ingredienteId);
                                insertPlatilloIngredienteCmd.Parameters.AddWithValue("@cantidadDisponible", ingrediente.cantidadDisponible);
                                insertPlatilloIngredienteCmd.ExecuteNonQuery();
                            }
                            else
                            {
                                string updateIngredienteQuery = "UPDATE ingrediente SET nombreIngrediente = @nombreIngrediente, cantidadDisponible = @cantidadDisponible WHERE idIngrediente = @idIngrediente";
                                MySqlCommand updateIngredienteCmd = new MySqlCommand(updateIngredienteQuery, cnx, transaction);
                                updateIngredienteCmd.Parameters.AddWithValue("@nombreIngrediente", ingrediente.nombreIngrediente);
                                updateIngredienteCmd.Parameters.AddWithValue("@cantidadDisponible", ingrediente.cantidadDisponible);
                                updateIngredienteCmd.Parameters.AddWithValue("@idIngrediente", ingrediente.idIngrediente);
                                updateIngredienteCmd.ExecuteNonQuery();

                                string insertPlatilloIngredienteQuery = "INSERT INTO platillo_ingrediente (idPlatillo, idIngrediente, cantidadDisponible) VALUES (@idPlatillo, @idIngrediente, @cantidadDisponible)";
                                MySqlCommand insertPlatilloIngredienteCmd = new MySqlCommand(insertPlatilloIngredienteQuery, cnx, transaction);
                                insertPlatilloIngredienteCmd.Parameters.AddWithValue("@idPlatillo", platillo.idPlatillo);
                                insertPlatilloIngredienteCmd.Parameters.AddWithValue("@idIngrediente", ingrediente.idIngrediente);
                                insertPlatilloIngredienteCmd.Parameters.AddWithValue("@cantidadDisponible", ingrediente.cantidadDisponible);
                                insertPlatilloIngredienteCmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();

                        return RedirectToAction("Menu");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine(ex.Message);
                        return View("Error");
                    }
                }
            }
            else
            {
                return View(platillo);
            }
        }



        public IActionResult Eliminar(int id)
        {
            using (MySqlConnection cnx = new MySqlConnection(_conf.GetConnectionString("DevConnection")))
            {
                cnx.Open();

                string deleteOrdenesQuery = "DELETE FROM ordenes WHERE idPlatillo = @idPlatillo";
                MySqlCommand deleteOrdenesCmd = new MySqlCommand(deleteOrdenesQuery, cnx);
                deleteOrdenesCmd.Parameters.AddWithValue("@idPlatillo", id);
                deleteOrdenesCmd.ExecuteNonQuery();

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

                string deletePlatilloIngredienteQuery = "DELETE FROM platillo_ingrediente WHERE idPlatillo = @idPlatillo";
                MySqlCommand deletePlatilloIngredienteCmd = new MySqlCommand(deletePlatilloIngredienteQuery, cnx);
                deletePlatilloIngredienteCmd.Parameters.AddWithValue("@idPlatillo", id);
                deletePlatilloIngredienteCmd.ExecuteNonQuery();

                string deletePlatilloQuery = "DELETE FROM platillo WHERE idPlatillo = @idPlatillo";
                MySqlCommand deletePlatilloCmd = new MySqlCommand(deletePlatilloQuery, cnx);
                deletePlatilloCmd.Parameters.AddWithValue("@idPlatillo", id);
                deletePlatilloCmd.ExecuteNonQuery();

                foreach (int ingredienteId in ingredientesIds)
                {
                    string deleteIngredienteQuery = "DELETE FROM ingrediente WHERE idIngrediente = @idIngrediente";
                    MySqlCommand deleteIngredienteCmd = new MySqlCommand(deleteIngredienteQuery, cnx);
                    deleteIngredienteCmd.Parameters.AddWithValue("@idIngrediente", ingredienteId);
                    deleteIngredienteCmd.ExecuteNonQuery();
                }

                cnx.Close();
            }

            return RedirectToAction("Menu");
        }


        //---------------------------------------------------------------ORDENES----------------------------------------------------------------------------------


        private string ObtenerNombrePlatillo(int idPlatillo)
        {
            string nombrePlatillo = string.Empty;

            using (MySqlConnection cnx = new MySqlConnection(_conf.GetConnectionString("DevConnection")))
            {
                cnx.Open();
                string query = "SELECT nombrePlatillo FROM platillo WHERE idPlatillo = @idPlatillo";
                MySqlCommand cmd = new MySqlCommand(query, cnx);
                cmd.Parameters.AddWithValue("@idPlatillo", idPlatillo);

                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    nombrePlatillo = result.ToString();
                }
            }

            return nombrePlatillo;
        }

        public IActionResult Ordenes()
        {
            List<Orden> ordenes = new List<Orden>();

            using (MySqlConnection cnx = new MySqlConnection(_conf.GetConnectionString("DevConnection")))
            {
                cnx.Open();
                string query = "SELECT * FROM ordenes";
                MySqlCommand cmd = new MySqlCommand(query, cnx);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Orden orden = new Orden();
                        orden.IdOrden = Convert.ToInt32(reader["idOrden"]);
                        orden.IdPlatillo = Convert.ToInt32(reader["idPlatillo"]);
                        orden.CantidadOrdenPlatillo = Convert.ToInt32(reader["cantidadOrdenPlatillo"]);

                        string nombrePlatillo = ObtenerNombrePlatillo(orden.IdPlatillo);
                        orden.NombrePlatillo = nombrePlatillo;

                        ordenes.Add(orden);
                    }
                }
            }

            return View(ordenes);
        }




        public IActionResult EliminarPlatillo(int id)
        {
            using (MySqlConnection cnx = new MySqlConnection(_conf.GetConnectionString("DevConnection")))
            {
                cnx.Open();
                string deleteQuery = "DELETE FROM ordenes WHERE idOrden = @id LIMIT 1"; // Limita la eliminación a una fila
                MySqlCommand cmd = new MySqlCommand(deleteQuery, cnx);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Ordenes");
        }


        //---------------------------------------------------------------NUEVA ORDEN----------------------------------------------------------------------------------


        public IActionResult NuevaOrden()
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
        public IActionResult CrearOrden(Dictionary<string, string> cantidad, Dictionary<string, string> idPlatillo, Dictionary<string, string> precioPlatillo)
        {
            using (MySqlConnection cnx = new MySqlConnection(_conf.GetConnectionString("DevConnection")))
            {
                cnx.Open();

                string selectMaxIdQuery = "SELECT MAX(idOrden) FROM ordenes";
                MySqlCommand selectMaxIdCmd = new MySqlCommand(selectMaxIdQuery, cnx);
                object lastOrderIdObj = selectMaxIdCmd.ExecuteScalar();

                int lastOrderId = 0;
                if (lastOrderIdObj != DBNull.Value)
                {
                    lastOrderId = Convert.ToInt32(lastOrderIdObj);
                }

                string insertOrdenQuery = "INSERT INTO ordenes (idOrden, idPlatillo, precioPlatillo, cantidadOrdenPlatillo) VALUES (@idOrden, @idPlatillo, @precioPlatillo, @cantidadOrdenPlatillo)";
                MySqlCommand insertOrdenCmd = new MySqlCommand(insertOrdenQuery, cnx);
                insertOrdenCmd.Parameters.AddWithValue("@idOrden", lastOrderId + 1);

                foreach (KeyValuePair<string, string> item in idPlatillo)
                {
                    int idPlatilloValue = Convert.ToInt32(item.Value);
                    int cantidadOrdenPlatillo = Convert.ToInt32(cantidad[item.Key]);
                    decimal precioPlatilloValue = Convert.ToDecimal(precioPlatillo[item.Key]);

                    if (cantidadOrdenPlatillo > 0)
                    {
                        insertOrdenCmd.Parameters.Clear();
                        insertOrdenCmd.Parameters.AddWithValue("@idOrden", lastOrderId + 1);
                        insertOrdenCmd.Parameters.AddWithValue("@idPlatillo", idPlatilloValue);
                        insertOrdenCmd.Parameters.AddWithValue("@precioPlatillo", precioPlatilloValue);
                        insertOrdenCmd.Parameters.AddWithValue("@cantidadOrdenPlatillo", cantidadOrdenPlatillo);

                        insertOrdenCmd.ExecuteNonQuery();
                    }
                }

                cnx.Close();
            }

            return RedirectToAction("Index");

        }



        //---------------------------------------------------------------EXISTENCIAS MENU----------------------------------------------------------------------------------



        public IActionResult ExistenciasMenu()
        {
            DataTable tbl = new DataTable();
            tbl.Columns.Add("Porciones Disponibles", typeof(int));

            using (MySqlConnection cnx = new MySqlConnection(_conf.GetConnectionString("DevConnection")))
            {
                cnx.Open();
                string query = "SELECT p.idPlatillo, p.nombrePlatillo, p.precioPlatillo, p.descripcionPlatillo " +
                               "FROM platillo p " +
                               "JOIN platillo_ingrediente pi ON p.idPlatillo = pi.idPlatillo " +
                               "JOIN ingrediente i ON pi.idIngrediente = i.idIngrediente " +
                               "GROUP BY p.idPlatillo, p.nombrePlatillo, p.precioPlatillo, p.descripcionPlatillo";
                MySqlDataAdapter adp = new MySqlDataAdapter(query, cnx);
                adp.Fill(tbl);

                for (int i = 0; i < tbl.Rows.Count; i++)
                {
                    long platilloId = Convert.ToInt64(tbl.Rows[i]["idPlatillo"]);
                    int porcionesDisponibles = CalcularPorcionesDisponibles(cnx, platilloId);
                    tbl.Rows[i]["Porciones Disponibles"] = porcionesDisponibles;
                }

                cnx.Close();
            }

            return View(tbl);
        }

        private int CalcularPorcionesDisponibles(MySqlConnection cnx, long platilloId)
        {
            int porcionesDisponibles = 0;

            string consultaIngredientes = "SELECT cantidadDisponible FROM platillo_ingrediente WHERE idPlatillo = @idPlatillo";
            MySqlCommand ingredientesCmd = new MySqlCommand(consultaIngredientes, cnx);
            ingredientesCmd.Parameters.AddWithValue("@idPlatillo", platilloId);

            using (MySqlDataReader reader = ingredientesCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int cantidadIngrediente = Convert.ToInt32(reader["cantidadDisponible"]);

                    if (cantidadIngrediente == 0)
                    {
                        return 0;
                    }

                    if (porcionesDisponibles == 0 || cantidadIngrediente < porcionesDisponibles)
                    {
                        porcionesDisponibles = cantidadIngrediente;
                    }
                }
            }

            return porcionesDisponibles;
        }

        //---------------------------------------------------------------LOGIN----------------------------------------------------------------------------------

        public IActionResult Login()
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
        public IActionResult Login(Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                using (var connection = new MySqlConnection(_conf.GetConnectionString("DevConnection")))
                {
                    connection.Open();
                    var query = "SELECT * FROM usuarios WHERE username = @Username AND password = @Password";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", usuario.username);
                        command.Parameters.AddWithValue("@Password", usuario.password);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                // Usuario autenticado correctamente
                                return RedirectToAction("Index", "Platillo");
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, "Credenciales inválidas");
                            }
                        }
                    }
                }
            }
            return View(usuario);
        }

        public IActionResult Logout()
        {

            return RedirectToAction("Login");
        }

    }
}
