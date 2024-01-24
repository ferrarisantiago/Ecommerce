﻿using Ecommerce_Modelos;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce_Datos.Datos.Repositorio.IRepositorio
{
    public interface IOrdenRepositorio : IRepositorio<Orden>
    {
        void Actualizar(Orden orden);
    }
}
