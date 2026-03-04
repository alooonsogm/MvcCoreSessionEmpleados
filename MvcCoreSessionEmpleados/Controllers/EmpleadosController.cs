using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MvcCoreSessionEmpleados.Extensions;
using MvcCoreSessionEmpleados.Models;
using MvcCoreSessionEmpleados.Repositories;
using System.Threading.Tasks;

namespace MvcCoreSessionEmpleados.Controllers
{
    public class EmpleadosController : Controller
    {
        private RepositoryEmpleados repo;
        private IMemoryCache memoryCache;

        public EmpleadosController(RepositoryEmpleados repo, IMemoryCache memoryCache)
        {
            this.repo = repo;
            this.memoryCache = memoryCache;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> SessionSalarios(int? salario)
        {
            if (salario != null)
            {
                //Queremos almacenar la suma total de salarios, que tengamos en session.
                int sumaTotal = 0;
                if (HttpContext.Session.GetString("SUMASALARIAL") != null)
                {
                    //Si ya tenemos datos almacenados, los recuperamos.
                    sumaTotal = HttpContext.Session.GetObject<int>("SUMASALARIAL");
                }
                //Sumamos el nuevo salario a la suma total.
                sumaTotal += salario.Value;
                //Almacenamos el valor dentro de session.
                HttpContext.Session.SetObjetc("SUMASALARIAL", sumaTotal);
                ViewData["MENSAJE"] = "Salario almacenado: " + salario;
            }
            List<Empleado> empleados = await this.repo.GetEmpleadosAsync();
            return View(empleados);
        }

        public IActionResult SumaSalarial()
        {
            return View();
        }

        public async Task<IActionResult> SessionEmpleados(int? idEmpleado)
        {
            if (idEmpleado != null)
            {
                Empleado emp = await this.repo.FindEmpleadoAsync(idEmpleado.Value);
                //En session tendremos alamacenado un conjunto de empleados.
                List<Empleado> empleadosList;
                //Debemos preguntar si ya tenemos empleados en session.
                if (HttpContext.Session.GetObject<List<Empleado>>("EMPLEADOS") != null)
                {
                    empleadosList = HttpContext.Session.GetObject<List<Empleado>>("EMPLEADOS");
                }
                else
                {
                    //Creamos una nueva lista para almacenar los empleados.
                    empleadosList = new List<Empleado>();
                }
                //Agregamos el empleado al list.
                empleadosList.Add(emp);
                //Almacenamos la lista en session.
                HttpContext.Session.SetObjetc("EMPLEADOS", empleadosList);
                ViewData["MENSAJE"] = "Empleado " + emp.Apellido + " alamacenado correctamente";
            }
            List<Empleado> empleados = await this.repo.GetEmpleadosAsync();
            return View(empleados);
        }

        public IActionResult EmpleadosAlmacenado()
        {
            return View();
        }

        public async Task<IActionResult> SessionEmpleadosOk(int? idEmpleado)
        {
            if (idEmpleado != null)
            {
                List<int> idsEmpleados;
                if (HttpContext.Session.GetObject<List<int>>("IDSEMPLEADOS") != null)
                {
                    idsEmpleados = HttpContext.Session.GetObject<List<int>>("IDSEMPLEADOS");
                }
                else
                {
                    idsEmpleados = new List<int>();
                }
                idsEmpleados.Add(idEmpleado.Value);
                HttpContext.Session.SetObjetc("IDSEMPLEADOS", idsEmpleados);
                ViewData["MENSAJE"] = "Empleados alamacenados: " + idsEmpleados.Count();
            }
            List<Empleado> empleados = await this.repo.GetEmpleadosAsync();
            return View(empleados);
        }

        public async Task<IActionResult> EmpleadosAlmacenadoOk()
        {
            //Recuperamos los datos de Session
            List<int> idsEmpleados = HttpContext.Session.GetObject<List<int>>("IDSEMPLEADOS");
            if (idsEmpleados != null)
            {
                List<Empleado> empleados = await this.repo.GetEmpleadosSessionAsync(idsEmpleados);
                return View(empleados);
            }
            else
            {
                ViewData["MENSAJE"] = "No existen empleados OK en session";
                return View();
            }
        }

        public async Task<IActionResult> SessionEmpleadosV4(int? idEmpleado)
        {
            List<int> idsEmpleados;
            if (idEmpleado != null)
            {
                if (HttpContext.Session.GetObject<List<int>>("IDSEMPLEADOSV4") != null)
                {
                    idsEmpleados = HttpContext.Session.GetObject<List<int>>("IDSEMPLEADOSV4");
                }
                else
                {
                    idsEmpleados = new List<int>();
                }
                idsEmpleados.Add(idEmpleado.Value);
                HttpContext.Session.SetObjetc("IDSEMPLEADOSV4", idsEmpleados);
                ViewData["MENSAJE"] = "Empleados alamacenados: " + idsEmpleados.Count();
                List<Empleado> empleados2 = await this.repo.GetEmpleadosV4Async(idsEmpleados);
                return View(empleados2);
            }
            else
            {
                if (HttpContext.Session.GetObject<List<int>>("IDSEMPLEADOSV4") != null)
                {
                    idsEmpleados = HttpContext.Session.GetObject<List<int>>("IDSEMPLEADOSV4");
                    List<Empleado> empleados2 = await this.repo.GetEmpleadosV4Async(idsEmpleados);
                    return View(empleados2);
                }
                else
                {
                    List<Empleado> empleados = await this.repo.GetEmpleadosAsync();
                    return View(empleados);
                }
            }
        }

        public async Task<IActionResult> EmpleadosAlmacenadoV4()
        {
            List<int> idsEmpleados = HttpContext.Session.GetObject<List<int>>("IDSEMPLEADOSV4");
            if (idsEmpleados != null)
            {
                List<Empleado> empleados = await this.repo.GetEmpleadosSessionAsync(idsEmpleados);
                return View(empleados);
            }
            else
            {
                ViewData["MENSAJE"] = "No existen empleados en session v4";
                return View();
            }
        }

        public async Task<IActionResult> SessionEmpleadosV5(int? idEmpleado, int? idFavorito)
        {
            if(idFavorito != null)
            {
                //Como estoy alamacenando en cache, vamos a guardar directamente los objetos en lugar de los IDS
                List<Empleado> empleadosFavoritos;
                if(this.memoryCache.Get("FAVORITOS") == null)
                {
                    //No existe nada en cache
                    empleadosFavoritos = new List<Empleado>();
                }
                else
                {
                    //Recuperamos el cache
                    empleadosFavoritos = this.memoryCache.Get<List<Empleado>>("FAVORITOS");
                }
                //Buscamos al empleado para guardarlo.
                Empleado empFav = await this.repo.FindEmpleadoAsync(idFavorito.Value);
                empleadosFavoritos.Add(empFav);
                this.memoryCache.Set("FAVORITOS", empleadosFavoritos);
            }

            List<int> idsEmpleados;
            if (idEmpleado != null)
            {
                if (HttpContext.Session.GetObject<List<int>>("IDSEMPLEADOSV5") != null)
                {
                    idsEmpleados = HttpContext.Session.GetObject<List<int>>("IDSEMPLEADOSV5");
                }
                else
                {
                    idsEmpleados = new List<int>();
                }
                idsEmpleados.Add(idEmpleado.Value);
                HttpContext.Session.SetObjetc("IDSEMPLEADOSV5", idsEmpleados);
            }

            List<Empleado> empleados = await this.repo.GetEmpleadosAsync();
            return View(empleados);
        }

        public async Task<IActionResult> EmpleadosAlmacenadoV5(int? idEliminar)
        {
            List<int> idsEmpleados = HttpContext.Session.GetObject<List<int>>("IDSEMPLEADOSV5");
            if (idsEmpleados != null)
            {
                //Preguntamos si hemos recibido algun dato para eliminar
                if (idEliminar != null)
                {
                    idsEmpleados.Remove(idEliminar.Value);
                    //Sino tenemos empleados en session, se queda a 0 la coleccion, por ello la eliminamos para que sea null.
                    if(idsEmpleados.Count == 0)
                    {
                        HttpContext.Session.Remove("IDSEMPLEADOSV5");
                        return View();
                    }
                    else
                    {
                        HttpContext.Session.SetObjetc("IDSEMPLEADOSV5", idsEmpleados);
                    }
                }
                List<Empleado> empleados = await this.repo.GetEmpleadosSessionAsync(idsEmpleados);
                return View(empleados);
            }
            return View();
        }

        public IActionResult EmpleadosFavoritos()
        {
            return View();
        }
    }
}
