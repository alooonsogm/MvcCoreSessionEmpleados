using Microsoft.EntityFrameworkCore;
using MvcCoreSessionEmpleados.Models;

namespace MvcCoreSessionEmpleados.Data
{
    public class HospitalContext: DbContext
    {
        public HospitalContext(DbContextOptions<HospitalContext> options): base(options)
        {

        }

        public DbSet<Empleado> Empleados { get; set; }
    }
}
