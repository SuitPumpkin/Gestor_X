using GestorX.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GestorX.Pestañas
{
    public class Clases
    {
        /// <summary>
        /// Conjunto de funciones para facilitar el uso de imagenes
        /// </summary>
        public static class ManejoDeImagenes
        {
            /// <summary>
            /// Convierte una imagen desde una ruta proporcionada en Bytes
            /// </summary>
            /// <param name="Ruta"></param>
            /// <returns></returns>
            public static byte[] ImagenABytes(string Ruta)
            {
                return File.Exists(Ruta) ? File.ReadAllBytes(Ruta) : null;
            }
            /// <summary>
            /// Interpreta Bytes como una imagen que puede ser usada en un control
            /// </summary>
            /// <param name="Bytes"></param>
            /// <returns></returns>
            public static BitmapImage BytesAImagen(byte[] Bytes)
            {
                if (Bytes == null || Bytes.Length == 0) return null;
                var image = new BitmapImage();
                using (var mem = new MemoryStream(Bytes))
                {
                    mem.Position = 0;
                    image.BeginInit();
                    image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.UriSource = null;
                    image.StreamSource = mem;
                    image.EndInit();
                }
                image.Freeze();
                return image;
            }
        }
        /// <summary>
        /// Clase dedicada para la base de datos y sus funciones
        /// </summary>
        public class BaseDeDatos
        {
            //TODO: mover la ruta de UbicaciónDB a los settings del sistema (no del usuario) de manera que no sea visible en el codigo
            public static string UbicaciónDB = "//SuitPumpkin/Trabajo/Bases de Datos/GestorX.db";
            private static string connectionString = $@"Data Source={UbicaciónDB};Version=3;";
            public static event Action BaseDeDatosActualizada;
            public static void NotificarActualizacion()
            {
                BaseDeDatosActualizada?.Invoke();
            }
            /// <summary>
            /// Ejecuta un comando SQL en la base de datos que no devuelve información
            /// </summary>
            /// <param name="Solicitud"></param>
            /// <param name="Parametros"></param>
            public static void ComandoDeEscritura(string Solicitud, params SQLiteParameter[] Parametros)
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(Solicitud, connection))
                    {
                        if (Parametros != null)
                            command.Parameters.AddRange(Parametros);

                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            /// <summary>
            /// Ejecuta un comando SQL en la base de datos que si devuelve información
            /// </summary>
            /// <param name="Solicitud"></param>
            /// <param name="procesarFila"></param>
            /// <param name="Parametros"></param>
            public static void ComandoDeLectura(string Solicitud, Action<SQLiteDataReader> procesarFila, params SQLiteParameter[] Parametros)
            {
                try
                {
                    using (var connection = new SQLiteConnection(connectionString))
                    {
                        connection.Open();
                        using (var command = new SQLiteCommand(Solicitud, connection))
                        {
                            if (Parametros != null)
                                command.Parameters.AddRange(Parametros);

                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    procesarFila(reader);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exeption: {ex}");
                    Debug.WriteLine($"connectionstring: {connectionString}");
                }
            }
        }
        public class Ubicaciones
        {
            //TODO: Mover todas estas rutas a los settings del usuario (no del sistema) para que el usuario lo pueda modificar mediante los ajustes
            public static string CarpetaProyectos { get; } = Settings.Default.UbicaciónProyectos;
        }
        public enum Entidad
        {
            Agenda,
            Inventario,
            Proyecto
        }
        public class ItemBase
        {
            public string ID { get; set; } = string.Empty;
            public byte[] Imagen { get; set; } = null;
            public string Descripción { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
        }
        public class ItemAgenda : ItemBase
        {
            public string Telefono { get; set; } = string.Empty;
            public string Correo { get; set; } = string.Empty;
            public string PaginaWeb { get; set; } = string.Empty;
            public string TipoDeContacto { get; set; } = string.Empty;
            public string UbicaciónMaps { get; set; } = string.Empty;
            /// <summary>
            /// Añade un Item a la Agenda
            /// </summary>
            /// <param name="Item"></param>
            public static void Create(ItemAgenda Item)
            {
                BaseDeDatos.ComandoDeEscritura(
                    "INSERT INTO Agenda (Imagen, Descripción, Nombre, Telefono, Correo, PaginaWeb, TipoDeContacto, UbicaciónMaps) VALUES (@Imagen, @Descripción, @Nombre, @Telefono, @Correo, @PaginaWeb, @TipoDeContacto, @UbicaciónMaps)", 
                    new SQLiteParameter("@Imagen", Item.Imagen),
                    new SQLiteParameter("@Descripción", Item.Descripción),
                    new SQLiteParameter("@Nombre", Item.Nombre),
                    new SQLiteParameter("@Telefono", Item.Telefono),
                    new SQLiteParameter("@Correo", Item.Correo),
                    new SQLiteParameter("@PaginaWeb", Item.PaginaWeb),
                    new SQLiteParameter("@TipoDeContacto", Item.TipoDeContacto),
                    new SQLiteParameter("@UbicaciónMaps", Item.UbicaciónMaps)
                    );
            }
            /// <summary>
            /// Lee todos los Items de la Agenda
            /// </summary>
            /// <returns></returns>
            public static ObservableCollection<ItemAgenda> Read()
            {
                ObservableCollection<ItemAgenda> Retorno = new ObservableCollection<ItemAgenda>();
                BaseDeDatos.ComandoDeLectura("SELECT * FROM Agenda", reader =>
                {
                    Retorno.Add(new ItemAgenda
                    {
                        ID = $"{reader["ID"]}",
                        Imagen = reader["Imagen"] as byte[],
                        Descripción = $"{reader["Descripción"]}",
                        Nombre = $"{reader["Nombre"]}",
                        Telefono = $"{reader["Telefono"]}",
                        Correo = $"{reader["Correo"]}",
                        PaginaWeb = $"{reader["PaginaWeb"]}",
                        TipoDeContacto = $"{reader["TipoDeContacto"]}",
                        UbicaciónMaps = $"{reader["UbicaciónMaps"]}"
                    });
                });
                return Retorno;
            }
            /// <summary>
            /// Actualiza la información de un Item de la Agenda
            /// </summary>
            public static void Update(ItemAgenda Item)
            {
                //TODO: hacer que solo se actualicen los valores que si cambiaron mediante que Item devuelva nulificados los que no recibieron cambios para crear un string personalizado para el Query
                BaseDeDatos.ComandoDeEscritura(
                    "UPDATE Agenda SET Imagen = @Imagen, Descripción = @Descripción, Nombre = @Nombre, Telefono = @Telefono, Correo = @Correo, PaginaWeb = @PaginaWeb, TipoDeContacto = @TipoDeContacto, UbicaciónMaps = @UbicaciónMaps WHERE ID = @ID",
                    new SQLiteParameter("@ID", Item.ID),
                    new SQLiteParameter("@Imagen", Item.Imagen),
                    new SQLiteParameter("@Descripción", Item.Descripción),
                    new SQLiteParameter("@Nombre", Item.Nombre),
                    new SQLiteParameter("@Telefono", Item.Telefono),
                    new SQLiteParameter("@Correo", Item.Correo),
                    new SQLiteParameter("@PaginaWeb", Item.PaginaWeb),
                    new SQLiteParameter("@TipoDeContacto", Item.TipoDeContacto),
                    new SQLiteParameter("@UbicaciónMaps", Item.UbicaciónMaps)
                    );
            }
            /// <summary>
            /// Borra un Item de la agenda
            /// </summary>
            public static void Delete(string ID)
            {
                //TODO: hacer que solo se deba enviar el string del id en lugar de todo el objeto
                BaseDeDatos.ComandoDeEscritura("DELETE FROM Agenda WHERE ID = @ID",new SQLiteParameter("@ID", ID));
            }
        }
        public class ItemInventario : ItemBase
        {
            public int Cantidad { get; set; } = 0;
            public string Unidad { get; set; } = string.Empty;
            public float Precio { get; set; } = 0;
            public string Vendedor { get; set; } = string.Empty;

            /// <summary>
            /// Añade un Item al Invenario
            /// </summary>
            /// <param name="Item"></param>
            public static void Create(ItemInventario Item)
            {
                //TODO
                BaseDeDatos.ComandoDeEscritura(
                    "INSERT INTO Inventario (Imagen, Descripción, Nombre, Cantidad, Unidad, Vendedor, Precio) VALUES (@Imagen, @Descripción, @Nombre, @Cantidad, @Unidad, @Vendedor, @Precio)",
                    new SQLiteParameter("@Imagen", Item.Imagen),
                    new SQLiteParameter("@Descripción", Item.Descripción),
                    new SQLiteParameter("@Nombre", Item.Nombre),
                    new SQLiteParameter("@Cantidad", Item.Cantidad),
                    new SQLiteParameter("@Unidad", Item.Unidad),
                    new SQLiteParameter("@Vendedor", Item.Vendedor),
                    new SQLiteParameter("@Precio", Item.Precio)
                    );
            }
            /// <summary>
            /// Lee todos los Items del Inventario
            /// </summary>
            /// <returns></returns>
            public static ObservableCollection<ItemInventario> Read()
            {
                ObservableCollection<ItemInventario> Retorno = new ObservableCollection<ItemInventario>();
                BaseDeDatos.ComandoDeLectura("SELECT * FROM Inventario", reader =>
                {
                    Retorno.Add(new ItemInventario
                    {
                        ID = $"{reader["ID"]}",
                        Imagen = reader["Imagen"] as byte[],
                        Descripción = $"{reader["Descripción"]}",
                        Nombre = $"{reader["Nombre"]}",
                        Cantidad = int.Parse($"{reader["Cantidad"]}"),
                        Unidad = $"{reader["Unidad"]}",
                        Vendedor = $"{reader["Vendedor"]}",
                        Precio = float.Parse($"{reader["Precio"]}")
                    });
                });
                return Retorno;
            }
            /// <summary>
            /// Actualiza la información de un Item del inventario
            /// </summary>
            public static void Update(ItemInventario Item)
            {
                //TODO: hacer que solo se actualicen los valores que si cambiaron mediante que Item devuelva nulificados los que no recibieron cambios para crear un string personalizado para el Query
                BaseDeDatos.ComandoDeEscritura(
                    "UPDATE Inventario SET Imagen = @Imagen, Descripción = @Descripción, Nombre = @Nombre, Cantidad = @Cantidad, Unidad = @Unidad, Vendedor = @Vendedor, Precio = @Precio WHERE ID = @ID",
                    new SQLiteParameter("@ID", Item.ID),
                    new SQLiteParameter("@Imagen", Item.Imagen),
                    new SQLiteParameter("@Descripción", Item.Descripción),
                    new SQLiteParameter("@Nombre", Item.Nombre),
                    new SQLiteParameter("@Cantidad", Item.Cantidad),
                    new SQLiteParameter("@Unidad", Item.Unidad),
                    new SQLiteParameter("@Vendedor", Item.Vendedor),
                    new SQLiteParameter("@Precio", Item.Precio)
                    );
            }
            /// <summary>
            /// Borra un Item del Invenario
            /// </summary>
            public static void Delete(string ID)
            {
                BaseDeDatos.ComandoDeEscritura("DELETE FROM Inventario WHERE ID = @ID", new SQLiteParameter("@ID", ID));
            }
        }
        public class ItemProyecto : ItemBase
        {
            public int Progreso { get; set; } = 0;
            public float Precio { get; set; } = 0;
            public float Pagado { get; set; } = 0;
            public string FechaCreación { get; set; } = string.Empty;
            public string Cliente { get; set; } = string.Empty;
            public string Carpeta{get{return $"{Ubicaciones.CarpetaProyectos}/{ID}";}}
            public string CarpetaEditables{get{return $"{Carpeta}/Editables";}}
            public string CarpetaMockups{get{return $"{Carpeta}/Mockups";}}
            public string CarpetaResultados{get{return $"{Carpeta}/Resultados";}}

            /// <summary>
            /// Añade un Item a los Proyectos
            /// </summary>
            /// <param name="Item"></param>
            public static void Create(ItemProyecto Item)
            {
                BaseDeDatos.ComandoDeEscritura(
                    "INSERT INTO Proyecto (Imagen, Descripción, Nombre, Progreso, Precio, Pagado, FechaCreación, Cliente) VALUES (@Imagen, @Descripción, @Nombre, @Progreso, @Precio, @Pagado, @FechaCreación, @Cliente)",
                    new SQLiteParameter("@Imagen", Item.Imagen),
                    new SQLiteParameter("@Descripción", Item.Descripción),
                    new SQLiteParameter("@Nombre", Item.Nombre),
                    new SQLiteParameter("@Progreso", Item.Progreso),
                    new SQLiteParameter("@Precio", Item.Precio),
                    new SQLiteParameter("@Pagado", Item.Pagado),
                    new SQLiteParameter("@FechaCreación", Item.FechaCreación),
                    new SQLiteParameter("@Cliente", Item.Cliente)
                    );
                Directory.CreateDirectory(Item.Carpeta);
                Directory.CreateDirectory(Item.CarpetaEditables);
                Directory.CreateDirectory(Item.CarpetaMockups);
                Directory.CreateDirectory(Item.CarpetaResultados);
            }
            /// <summary>
            /// Lee todos los Items de los Proyectos
            /// </summary>
            /// <returns></returns>
            public static ObservableCollection<ItemProyecto> Read()
            {
                ObservableCollection<ItemProyecto> Retorno = new ObservableCollection<ItemProyecto>();
                BaseDeDatos.ComandoDeLectura("SELECT * FROM Proyecto", reader =>
                {
                    Retorno.Add(new ItemProyecto
                    {
                        ID = $"{reader["ID"]}",
                        Imagen = reader["Imagen"] as byte[],
                        Descripción = $"{reader["Descripción"]}",
                        Nombre = $"{reader["Nombre"]}",
                        Progreso = int.Parse($"{reader["Progreso"]}"),
                        Precio = float.Parse($"{reader["Precio"]}"),
                        Pagado = float.Parse($"{reader["Pagado"]}"),
                        FechaCreación = $"{reader["FechaCreación"]}",
                        Cliente = $"{reader["Cliente"]}",
                    });
                });
                return Retorno;
            }
            /// <summary>
            /// Actualiza la información de un Item de los Proyectos
            /// </summary>
            public static void Update(ItemProyecto Item)
            {
                //TODO: hacer que solo se actualicen los valores que si cambiaron mediante que Item devuelva nulificados los que no recibieron cambios para crear un string personalizado para el Query
                BaseDeDatos.ComandoDeEscritura(
                    "UPDATE Proyecto SET Imagen = @Imagen, Descripción = @Descripción, Nombre = @Nombre, Progreso = @Progreso, Precio = @Precio, Pagado = @Pagado, FechaCreación = @FechaCreación, Cliente = @Cliente WHERE ID = @ID",
                    new SQLiteParameter("@ID", Item.ID),
                    new SQLiteParameter("@Imagen", Item.Imagen),
                    new SQLiteParameter("@Descripción", Item.Descripción),
                    new SQLiteParameter("@Nombre", Item.Nombre),
                    new SQLiteParameter("@Progreso", Item.Progreso),
                    new SQLiteParameter("@Precio", Item.Precio),
                    new SQLiteParameter("@Pagado", Item.Pagado),
                    new SQLiteParameter("@FechaCreación", Item.FechaCreación),
                    new SQLiteParameter("@Cliente", Item.Cliente)
                    );
            }
            /// <summary>
            /// Borra un Item de los Proyectos
            /// </summary>
            public static void Delete(string ID)
            {
                BaseDeDatos.ComandoDeEscritura("DELETE FROM Proyecto WHERE ID = @ID", new SQLiteParameter("@ID", ID));
                //borrar la carpeta del proyecto
                if (Directory.Exists($"{Ubicaciones.CarpetaProyectos}/{ID}"))
                {
                    Directory.Delete($"{Ubicaciones.CarpetaProyectos}/{ID}", true);
                }
            }
        }
    }
}
