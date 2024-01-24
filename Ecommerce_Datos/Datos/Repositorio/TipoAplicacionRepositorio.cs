using Ecommerce_Datos.Datos.Repositorio.IRepositorio;
using Ecommerce_Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce_Datos.Datos.Repositorio
{
    public class TipoAplicacionRepositorio : Repositorio<TipoAplicacion>, ITipoAplicacionRepositorio
    {
        private readonly ApplicationDbContext _db;
        public TipoAplicacionRepositorio(ApplicationDbContext db):base(db)
        {
            _db = db;
        }

        public void Actualizar(TipoAplicacion tipoAplicacion)
        {
            var TipoAnt = _db.TiposAplicaciones.FirstOrDefault(c => c.Id == tipoAplicacion.Id);
            if(TipoAnt != null)
            {
                TipoAnt.Nombre = tipoAplicacion.Nombre;
            }
        }
    }
}
