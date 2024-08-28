using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public class Ubicaciones
        {
            public static string Inventario { get; } = @"\\SuitPumpkin\Trabajo\Bases de Datos\inventario.csv";
            public static string Agenda { get; } = @"\\SuitPumpkin\Trabajo\Bases de Datos\agenda.csv";
            public static string Proyectos { get; } = @"\\SuitPumpkin\Trabajo\Bases de Datos\proyectos.csv";
            public static string CarpetaProyectos { get; } = @"\\SuitPumpkin\Trabajo\Bases de Datos\Proyectos";
            public static string Imagenes { get; } = @"\\SuitPumpkin\Trabajo\Bases de Datos\Imagenes\";
        }
        public class ItemBase
        {
            public string ID { get; set; } = string.Empty;
            public string Imagen { get; set; } = string.Empty;
            public ImageSource ImagenReal { get; set; }
            public string Descripción { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public static string[] ImagenesEnDesUso()
            {
                var imagenes = Directory.GetFiles(Ubicaciones.Imagenes);
                return imagenes;
            }
        }
        public class ItemAgenda : ItemBase
        {
            public string Telefono { get; set; } = string.Empty;
            public string Correo { get; set; } = string.Empty;
            public string PaginaWeb { get; set; } = string.Empty;
            public string TipoContacto { get; set; } = string.Empty;
            public string Ubicación { get; set; } = string.Empty;
            public static class Archivo
            {
                public static ObservableCollection<ItemAgenda> Leer()
                {
                    ObservableCollection<ItemAgenda> items = new ObservableCollection<ItemAgenda>();
                    //Si el archivo no existe lo crea
                    if (!System.IO.File.Exists(Ubicaciones.Agenda)) { System.IO.File.Create(Ubicaciones.Agenda); }
                    //Lee el archivo fisico y crea un archivo virtual
                    string[] ArchivoVirtual = System.IO.File.ReadAllLines(Ubicaciones.Agenda);
                    //Busca en el archivo virtual por coincidencias con el formato correcto
                    //Formato: ID;Nombre;Telefono;Correo;PaginaWeb;Descripción
                    for (int i = 0; i < ArchivoVirtual.Length; i++)
                    {
                        var Linea = ArchivoVirtual[i];
                        var Elemento = Linea.Split(';');
                        if (Elemento.Length >= 3)
                        {
                            items.Add(new ItemAgenda
                            {
                                ID = Elemento[0],
                                Nombre = Elemento[1],
                                Telefono = Elemento[2],
                                Correo = Elemento[3],
                                PaginaWeb = Elemento[4],
                                Descripción = Elemento[5],
                                TipoContacto = Elemento[6],
                                Ubicación = Elemento[7],
                                Imagen = System.IO.Path.Combine(Ubicaciones.Imagenes, $"A-{Elemento[0]}.jpg")
                            });
                        }
                    }
                    return items;
                }
                public static void Escribir(ObservableCollection<ItemAgenda> Items)
                {
                    //Defino el archivo virtual
                    StringBuilder ArchivoVirtual = new StringBuilder();
                    //Se escribe cada item en el archivo virtual
                    foreach (ItemAgenda i in Items)
                    {
                        //Formato: ID;Nombre;Telefono;Correo;PaginaWeb;Descripción;
                        ArchivoVirtual.AppendLine($"{i.ID};{i.Nombre};{i.Telefono};{i.Correo};{i.PaginaWeb};{i.Descripción};{i.TipoContacto};{i.Ubicación}");
                    }
                    //Se guarda el archivo virtual como archivo fisico
                    System.IO.File.WriteAllText(Ubicaciones.Agenda, ArchivoVirtual.ToString());
                }
            }

            //Añade un nuevo Item a la agenda
            public static void Create(ItemAgenda ItemAGuardar, ObservableCollection<ItemAgenda> Items)
            {
                //Le asigna un GUID con el formato: "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
                ItemAGuardar.ID = Guid.NewGuid().ToString();
                //Copia la imagen en la ruta correspondiente
                File.Copy(ItemAGuardar.Imagen, System.IO.Path.Combine(Ubicaciones.Imagenes, $"A-{ItemAGuardar.ID}.jpg"), true);
                //Añade un archivo a la lista
                Items.Add(ItemAGuardar);
                //Escribe el archivo con la modificación
                Archivo.Escribir(Items);
            }

            //Busca un Item por ID y retorna el encontrado
            public static ItemAgenda Read(string IDBuscado, ObservableCollection<ItemAgenda> Items)
            {
                ItemAgenda retornado = Items.FirstOrDefault(I => I.ID == IDBuscado);
                BitmapImage bitmap = new BitmapImage();
                using (FileStream stream = new FileStream(retornado.Imagen, FileMode.Open, FileAccess.Read))
                {
                    MemoryStream memoryStream = new MemoryStream();
                    stream.CopyTo(memoryStream);
                    memoryStream.Position = 0;
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = memoryStream;
                    bitmap.EndInit();
                }
                bitmap.Freeze(); // Hace que el bitmap sea accesible desde cualquier hilo
                retornado.ImagenReal = bitmap;
                return retornado;
            }

            //Actualiza la información de un Item
            public static void Update(ItemAgenda Editado, ObservableCollection<ItemAgenda> Items)
            {
                ItemAgenda encontrado = Items.FirstOrDefault(I => I.ID == Editado.ID);
                var ubi = Items.IndexOf(encontrado);
                Items.Remove(encontrado);
                //Copia la imagen en la ruta correspondiente en caso de ser necesario
                if (encontrado.Imagen != Editado.Imagen && Editado.Imagen != "")
                {
                    File.Copy(Editado.Imagen, System.IO.Path.Combine(Ubicaciones.Imagenes, $"A-{Editado.ID}.jpg"), true);
                }
                else if (encontrado.Imagen != "")
                {
                    Editado.Imagen = encontrado.Imagen;
                }
                Items.Insert(ubi, Editado);
                //Escribe el archivo con la modificación
                Archivo.Escribir(Items);
            }

            //Borra un Item de la agenda
            public static void Delete(ItemAgenda Eliminado, ObservableCollection<ItemAgenda> Items)
            {
                Items.Remove(Eliminado);
                //Borra la imagen
                GC.Collect();
                try
                {
                    File.Delete(System.IO.Path.Combine(Ubicaciones.Imagenes, $"A-{Eliminado.ID}.jpg"));
                }
                catch { }
                //Escribe el archivo con la modificación
                Archivo.Escribir(Items);
            }
        }
        public class ItemInventario : ItemBase
        {
            public int Cantidad { get; set; } = 0;
            public string Unidad { get; set; } = string.Empty;
            public float PrecioCompra { get; set; } = 0;
            public string Vendedor { get; set; } = string.Empty;
            public static class Archivo
            {
                public static ObservableCollection<ItemInventario> Leer()
                {
                    ObservableCollection<ItemInventario> items = new ObservableCollection<ItemInventario>();
                    //Si el archivo no existe lo crea
                    if (!System.IO.File.Exists(Ubicaciones.Inventario)) { System.IO.File.Create(Ubicaciones.Inventario); }
                    //Lee el archivo fisico y crea un archivo virtual
                    string[] ArchivoVirtual = System.IO.File.ReadAllLines(Ubicaciones.Inventario);
                    //Busca en el archivo virtual por coincidencias con el formato correcto
                    //Formato: ID;Nombre;Cantidad;Unidad;Descripción;PrecioCompra;Vendedor
                    for (int i = 0; i < ArchivoVirtual.Length; i++)
                    {
                        var Linea = ArchivoVirtual[i];
                        var Elemento = Linea.Split(';');
                        if (Elemento.Length >= 6)
                        {
                            items.Add(new ItemInventario
                            {
                                ID = Elemento[0],
                                Nombre = Elemento[1],
                                Cantidad = int.Parse(Elemento[2]),
                                Unidad = Elemento[3],
                                Descripción = Elemento[4],
                                PrecioCompra = int.Parse(Elemento[5]),
                                Vendedor = Elemento[6],
                                Imagen = System.IO.Path.Combine(Ubicaciones.Imagenes, $"I-{Elemento[0]}.jpg")
                            });
                        }
                    }
                    return items;
                }
                public static void Escribir(ObservableCollection<ItemInventario> Items)
                {
                    //Defino el archivo virtual
                    StringBuilder ArchivoVirtual = new StringBuilder();
                    //Se escribe cada item en el archivo virtual
                    foreach (ItemInventario i in Items)
                    {
                        //Formato: ID;Nombre;Cantidad;Unidad;Descripción;PrecioCompra;Vendedor
                        ArchivoVirtual.AppendLine($"{i.ID};{i.Nombre};{i.Cantidad};{i.Unidad};{i.Descripción};{i.PrecioCompra};{i.Vendedor};");
                    }
                    //Se guarda el archivo virtual como archivo fisico
                    System.IO.File.WriteAllText(Ubicaciones.Inventario, ArchivoVirtual.ToString());
                }
            }

            //Añade un nuevo Item al inventario
            public static void Create(ItemInventario ItemAAgregar, ObservableCollection<ItemInventario> Items)
            {
                //Le asigna un GUID con el formato: "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
                ItemAAgregar.ID = Guid.NewGuid().ToString();
                //Copia la imagen en la ruta correspondiente
                File.Copy(ItemAAgregar.Imagen, System.IO.Path.Combine(Ubicaciones.Imagenes, $"I-{ItemAAgregar.ID}.jpg"), true);
                //Añade un archivo a la lista
                Items.Add(ItemAAgregar);
                //Escribe el archivo con la modificación
                Archivo.Escribir(Items);
            }

            //Busca un Item por ID y retorna el encontrado
            public static ItemInventario Read(string IDBuscado, ObservableCollection<ItemInventario> Items)
            {
                ItemInventario retornado = Items.FirstOrDefault(I => I.ID == IDBuscado);
                BitmapImage bitmap = new BitmapImage();
                using (FileStream stream = new FileStream(retornado.Imagen, FileMode.Open, FileAccess.Read))
                {
                    MemoryStream memoryStream = new MemoryStream();
                    stream.CopyTo(memoryStream);
                    memoryStream.Position = 0;
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = memoryStream;
                    bitmap.EndInit();
                }
                bitmap.Freeze(); // Hace que el bitmap sea accesible desde cualquier hilo
                retornado.ImagenReal = bitmap;
                return retornado;
            }

            //Actualiza la información de un Item
            public static void Update(ItemInventario Editado, ObservableCollection<ItemInventario> Items)
            {
                ItemInventario encontrado = Items.FirstOrDefault(I => I.ID == Editado.ID);
                var ubi = Items.IndexOf(encontrado);
                Items.Remove(encontrado);
                //Copia la imagen en la ruta correspondiente en caso de ser necesario
                if (encontrado.Imagen != Editado.Imagen && Editado.Imagen != "")
                {
                    File.Copy(Editado.Imagen, System.IO.Path.Combine(Ubicaciones.Imagenes, $"I-{Editado.ID}.jpg"), true);
                }
                else if (encontrado.Imagen != "")
                {
                    Editado.Imagen = encontrado.Imagen;
                }
                Items.Insert(ubi, Editado);
                //Escribe el archivo con la modificación
                Archivo.Escribir(Items);
            }

            //Borra un Item del inventario
            public static void Delete(ItemInventario Eliminado, ObservableCollection<ItemInventario> Items)
            {
                Items.Remove(Eliminado);
                //Borra la imagen
                GC.Collect();
                try
                {
                    File.Delete(System.IO.Path.Combine(Ubicaciones.Imagenes, $"I-{Eliminado.ID}.jpg"));
                }
                catch { }
                //Escribe el archivo con la modificación
                Archivo.Escribir(Items);
            }
        }
        public class ItemProyecto : ItemBase
        {
            public int Progreso { get; set; } = 0;
            public float Precio { get; set; } = 0;
            public float Pagado { get; set; } = 0;
            public string FechaCreación { get; set; } = string.Empty;
            public string Cliente { get; set; } = string.Empty; //en una lista dropdown debe salir el nombre de cada Cliente
            public string Carpeta { get; set; } = string.Empty; //la pone el sistema en base al id de proyecto
            public string CarpetaEditables { get; set; } = string.Empty; //$"{Carpeta}/Editables"; Para los archivos editables usados
            public string CarpetaMockups { get; set; } = string.Empty; //$"{Carpeta}/Mockups"; Para los Mockups con los resultados puestos para mostrarlos, debe ser el archivo del mockup y la imagen exportada de como queda el mockup
            public string CarpetaResultados { get; set; } = string.Empty; //$"{Carpeta}/Resultados"; Para los Resutados que se le imprimiran al cliente, aqui tambien se guarda la nota de venta que se le muestra al cliente

            public static class Archivo
            {
                public static ObservableCollection<ItemProyecto> Leer()
                {
                    ObservableCollection<ItemProyecto> items = new ObservableCollection<ItemProyecto>();
                    //Si el archivo no existe lo crea
                    if (!System.IO.File.Exists(Ubicaciones.Proyectos)) { System.IO.File.Create(Ubicaciones.Proyectos); }
                    //Lee el archivo fisico y crea un archivo virtual
                    string[] ArchivoVirtual = System.IO.File.ReadAllLines(Ubicaciones.Proyectos);
                    //Busca en el archivo virtual por coincidencias con el formato correcto
                    //Formato: ID;Nombre;Precio;Pagado;Progreso;Cliente;FechaCreación;Descripción;
                    for (int i = 0; i < ArchivoVirtual.Length; i++)
                    {
                        var Linea = ArchivoVirtual[i];
                        var Elemento = Linea.Split(';');
                        if (Elemento.Length >= 7)
                        {
                            items.Add(new ItemProyecto
                            {
                                ID = Elemento[0],
                                Nombre = Elemento[1],
                                Precio = float.Parse(Elemento[2]),
                                Pagado = float.Parse(Elemento[3]),
                                Progreso = int.Parse(Elemento[4]),
                                Cliente = Elemento[5],
                                FechaCreación = Elemento[6],
                                Descripción = Elemento[7],
                                Imagen = System.IO.Path.Combine(Ubicaciones.Imagenes, $"P-{Elemento[0]}.jpg"),
                                Carpeta = System.IO.Path.Combine(Ubicaciones.CarpetaProyectos, $"P-{Elemento[0]}"),
                                CarpetaEditables = System.IO.Path.Combine(Ubicaciones.CarpetaProyectos, $"P-{Elemento[0]}/Editables"),
                                CarpetaMockups = System.IO.Path.Combine(Ubicaciones.CarpetaProyectos, $"P-{Elemento[0]}/Mockups"),
                                CarpetaResultados = System.IO.Path.Combine(Ubicaciones.CarpetaProyectos, $"P-{Elemento[0]}/Resultados")
                            });
                        }
                    }
                    return items;
                }
                public static void Escribir(ObservableCollection<ItemProyecto> Items)
                {
                    //Defino el archivo virtual
                    StringBuilder ArchivoVirtual = new StringBuilder();
                    //Se escribe cada item en el archivo virtual
                    foreach (ItemProyecto i in Items)
                    {
                        //Formato: ID;Nombre;Precio;Pagado;Progreso;Cliente;FechaCreación;Descripción;
                        ArchivoVirtual.AppendLine($"{i.ID};{i.Nombre};{i.Precio};{i.Pagado};{i.Progreso};{i.Cliente};{i.FechaCreación};{i.Descripción};");
                    }
                    //Se guarda el archivo virtual como archivo fisico
                    System.IO.File.WriteAllText(Ubicaciones.Proyectos, ArchivoVirtual.ToString());
                }
            }

            //Añade un nuevo Item a los proyectos
            public static void Create(ItemProyecto ItemACrear, ObservableCollection<ItemProyecto> Items)
            {
                //Le asigna un GUID con el formato: "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
                ItemACrear.ID = Guid.NewGuid().ToString();
                //Copia la imagen en la ruta correspondiente
                File.Copy(ItemACrear.Imagen, System.IO.Path.Combine(Ubicaciones.Imagenes, $"P-{ItemACrear.ID}.jpg"), true);
                //Crea las carpetas del proyecto
                var ruta = Path.Combine(System.IO.Path.Combine(Ubicaciones.CarpetaProyectos, $"P-{ItemACrear.ID}"));
                if (!Directory.Exists(ruta))
                {
                    Directory.CreateDirectory(ruta);
                    Directory.CreateDirectory(Path.Combine(ruta, "Editables"));
                    Directory.CreateDirectory(Path.Combine(ruta, "Mockups"));
                    Directory.CreateDirectory(Path.Combine(ruta, "Resultados"));
                    Directory.CreateDirectory(Path.Combine(ruta, "Recursos"));
                }
                ItemACrear.Carpeta = ruta;
                ItemACrear.CarpetaEditables = Path.Combine(ruta, "Editables");
                ItemACrear.CarpetaMockups = Path.Combine(ruta, "Mockups");
                ItemACrear.CarpetaResultados = Path.Combine(ruta, "Resultados");
                //Añade un archivo a la lista
                Items.Add(ItemACrear);
                //Escribe el archivo con la modificación
                Archivo.Escribir(Items);
            }

            //Busca un Item por ID y retorna el encontrado
            public static ItemProyecto Read(string IDBuscado, ObservableCollection<ItemProyecto> Items)
            {
                ItemProyecto retornado = Items.FirstOrDefault(I => I.ID == IDBuscado);
                BitmapImage bitmap = new BitmapImage();
                using (FileStream stream = new FileStream(retornado.Imagen, FileMode.Open, FileAccess.Read))
                {
                    MemoryStream memoryStream = new MemoryStream();
                    stream.CopyTo(memoryStream);
                    memoryStream.Position = 0;
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = memoryStream;
                    bitmap.EndInit();
                }
                bitmap.Freeze(); // Hace que el bitmap sea accesible desde cualquier hilo
                retornado.ImagenReal = bitmap;
                return retornado;
            }

            //Actualiza la información de un Item
            public static void Update(ItemProyecto Editado, ObservableCollection<ItemProyecto> Items)
            {
                ItemProyecto encontrado = Items.FirstOrDefault(I => I.ID == Editado.ID);
                var ubi = Items.IndexOf(encontrado);
                Items.Remove(encontrado);
                //Copia la imagen en la ruta correspondiente en caso de ser necesario
                if (encontrado.Imagen != Editado.Imagen && Editado.Imagen != "")
                {
                    File.Copy(Editado.Imagen, System.IO.Path.Combine(Ubicaciones.Imagenes, $"P-{Editado.ID}.jpg"), true);
                }
                else if (encontrado.Imagen != "")
                {
                    Editado.Imagen = encontrado.Imagen;
                }
                Items.Insert(ubi, Editado);
                //Escribe el archivo con la modificación
                Archivo.Escribir(Items);
            }

            //Borra un Item de los proyectos
            public static void Delete(ItemProyecto Eliminado, ObservableCollection<ItemProyecto> Items)
            {
                Items.Remove(Eliminado);
                //Borra la imagen
                GC.Collect();
                try
                {
                    File.Delete(System.IO.Path.Combine(Ubicaciones.Imagenes, $"P-{Eliminado.ID}.jpg"));
                }
                catch { }
                //Crea las carpetas del proyecto
                var ruta = Path.Combine(System.IO.Path.Combine(Ubicaciones.CarpetaProyectos, $"P-{Eliminado.ID}"));
                if (Directory.Exists(ruta))
                {
                    Directory.Delete(ruta, true);
                }
                //Escribe el archivo con la modificación
                Archivo.Escribir(Items);
            }
        }
    }
}
