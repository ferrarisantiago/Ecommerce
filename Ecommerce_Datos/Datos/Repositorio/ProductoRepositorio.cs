using Ecommerce_Datos.Datos.Repositorio.IRepositorio;
using Ecommerce_Modelos;
using Ecommerce_Utilidades;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce_Datos.Datos.Repositorio
{
    public class ProductoRepositorio : Repositorio<Producto>, IProductoRepositorio
    {
        private readonly ApplicationDbContext _db;
        public ProductoRepositorio(ApplicationDbContext db):base(db)
        {
            _db = db;
        }

        public void Actualizar(Producto producto)
        {
            _db.Update(producto);
        }

        public IEnumerable<SelectListItem> ObtenerDrowdownList(string obj)
        {
            if(obj == WC.CategoriaNombre)
            {
                return _db.Categorias.Select(c => new SelectListItem
                {
                    Text = c.NombreCategoria,
                    Value = c.Id.ToString(),
                });
            }
            if (obj == WC.AplicacionNombre)
            {
                return _db.TiposAplicaciones.Select(t => new SelectListItem
                {
                    Text = t.Nombre,
                    Value = t.Id.ToString(),
                });
            }
            return null;
        }
    }
}
