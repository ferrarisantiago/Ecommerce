using Braintree;
using Ecommerce.Application.Cart;
using Ecommerce_Datos.Datos;
using Ecommerce_Datos.Datos.Repositorio.IRepositorio;
using Ecommerce_Modelos;
using Ecommerce_Modelos.ViewModels;
using Ecommerce_Utilidades;
using Ecommerce_Utilidades.BrainTree;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Security.Claims;
using System.Text;

namespace Ecommerce.Controllers
{
    [Authorize]
    public class CarroController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailSender _emailSender;
        private readonly IProductoRepositorio _prodRepo;
        private readonly IUsuarioAplicacionRepositorio _UsuarioRepo;
        private readonly IOrdenRepositorio _orderRepo;
        private readonly IOrdenDetalleRepositorio _orderDetalleRepo;
        private readonly IVentaRepositorio _ventaRepo;
        private readonly IVentaDetalleRepositorio _ventaDetalleRepo;
        private readonly IBrainTreeGate _brainTreeGate;
        private readonly ICartCalculatorService _cartCalculatorService;

        [BindProperty]
        public UserProductosVM userProductosVM { get; set; }

        public CarroController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment, 
            IEmailSender emailSender, IUsuarioAplicacionRepositorio usuarioRepo, IOrdenRepositorio orderRepo, 
            IOrdenDetalleRepositorio orderDetalleRepo, IProductoRepositorio prodRepo,
            IVentaRepositorio ventaRepo, IVentaDetalleRepositorio ventaDetalleRepo,
            IBrainTreeGate brainTreeGate,
            ICartCalculatorService cartCalculatorService
            )
        {
            _webHostEnvironment = webHostEnvironment;
            _emailSender = emailSender;
            _UsuarioRepo = usuarioRepo;
            _orderRepo = orderRepo;
            _orderDetalleRepo = orderDetalleRepo;
            _prodRepo = prodRepo;
            _ventaRepo = ventaRepo;
            _ventaDetalleRepo = ventaDetalleRepo;
            _brainTreeGate = brainTreeGate;
            _cartCalculatorService = cartCalculatorService;
        }

        public IActionResult Index()
        {
            List<CarroCompras> carroCompraLista = new List<CarroCompras>();
            if (HttpContext.Session.Get<IEnumerable<CarroCompras>>(WC.SessionCarroCompras) != null
                && HttpContext.Session.Get<IEnumerable<CarroCompras>>(WC.SessionCarroCompras).Count() > 0)
            {
                carroCompraLista = HttpContext.Session.Get<List<CarroCompras>>(WC.SessionCarroCompras);
            }
            List<int> prodEnCarro = carroCompraLista.Select(i => i.ProductoId).ToList();
            //IEnumerable<Producto> prodList = _db.Productos.Where(p => prodEnCarro.Contains(p.Id));
            IEnumerable<Producto> prodList = _prodRepo.ObtenerTodos(p => prodEnCarro.Contains(p.Id) && !p.IsDeleted && p.IsActive);
            List<Producto> prodlistFinal = new List<Producto>();
            List<CarroCompras> normalizedCart = new List<CarroCompras>();
            bool cartWasUpdated = false;

            List<string> adjustedProducts = new();
            foreach(var objCarro in carroCompraLista)
            {
                Producto prodTemp = prodList.FirstOrDefault(p => p.Id == objCarro.ProductoId);
                if (prodTemp == null)
                {
                    cartWasUpdated = true;
                    continue;
                }

                int qty = objCarro.Cantidad < 1 ? 1 : objCarro.Cantidad;
                if (qty > prodTemp.Stock)
                {
                    qty = prodTemp.Stock;
                    cartWasUpdated = true;
                    adjustedProducts.Add(prodTemp.NombreProducto);
                }

                if (qty <= 0)
                {
                    cartWasUpdated = true;
                    adjustedProducts.Add(prodTemp.NombreProducto);
                    continue;
                }

                prodTemp.TempCantidad = qty;
                prodlistFinal.Add(prodTemp);

                normalizedCart.Add(new CarroCompras
                {
                    ProductoId = prodTemp.Id,
                    Cantidad = qty
                });
            }

            if (cartWasUpdated)
            {
                HttpContext.Session.Set(WC.SessionCarroCompras, normalizedCart);
                if (adjustedProducts.Count > 0)
                {
                    TempData[WC.Error] = "Se ajustaron cantidades por stock disponible: " + string.Join(", ", adjustedProducts.Distinct());
                }
            }

            var cartSummary = _cartCalculatorService.CalculateSummary(new CartSummaryRequest
            {
                Items = prodlistFinal.Select(x => new CartItemInput
                {
                    ProductId = x.Id,
                    ProductName = x.NombreProducto,
                    UnitPrice = Convert.ToDecimal(x.Precio),
                    Quantity = x.TempCantidad
                }).ToList()
            });
            ViewBag.CartSummary = cartSummary;
            return View(prodlistFinal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public IActionResult IndexPost(IEnumerable<Producto> ProdLista)
        {
            List<CarroCompras> carroComprasLista = new List<CarroCompras>();
            List<string> stockErrors = new();
            foreach (Producto prod in ProdLista)
            {
                Producto existing = _prodRepo.ObtenerPrimero(p => p.Id == prod.Id && !p.IsDeleted && p.IsActive);
                if (existing == null || existing.Stock <= 0)
                {
                    stockErrors.Add($"'{prod.NombreProducto}' ya no esta disponible.");
                    continue;
                }

                int cantidad = prod.TempCantidad < 1 ? 1 : prod.TempCantidad;
                if (cantidad > existing.Stock)
                {
                    stockErrors.Add($"'{existing.NombreProducto}' solo tiene {existing.Stock} unidades disponibles.");
                    continue;
                }

                carroComprasLista.Add(new CarroCompras
                {
                    ProductoId = prod.Id,
                    Cantidad = cantidad
                });
            }

            if (stockErrors.Count > 0)
            {
                HttpContext.Session.Set(WC.SessionCarroCompras, carroComprasLista);
                TempData[WC.Error] = string.Join(" ", stockErrors);
                return RedirectToAction(nameof(Index));
            }

            HttpContext.Session.Set(WC.SessionCarroCompras, carroComprasLista);
            return RedirectToAction("Index", "Checkout");
        }

        public IActionResult Resumen()
        {
            if (!User.IsInRole(WC.AdminRole))
            {
                return RedirectToAction("Index", "Checkout");
            }

            //Si es administrador, debe cargar los datos del comprador
            //Si es el mismo comprador, se cargara con sus datos
            //si esta asignado a una orden, se mostrara los datos al uauario que esta asignado

            UsuarioAplicacion usuarioAplicacion;
            if (User.IsInRole(WC.AdminRole))  //si es administrador
            {
                if(HttpContext.Session.Get<int>(WC.SessionOrdenId) != 0) //Si esta ligado a una orden
                {
                    Orden orden = _orderRepo.ObtenerPrimero(u => u.Id ==
                                    HttpContext.Session.Get<int>(WC.SessionOrdenId));
                    usuarioAplicacion = new UsuarioAplicacion()
                    {
                        Email = orden.Email,
                        NombreCompleto = orden.NombreCompleto,
                        PhoneNumber = orden.Telefono
                    };
                }
                else //Si no pertenece a una orden
                {
                    usuarioAplicacion = new UsuarioAplicacion();
                }
                var gateway = _brainTreeGate.GetGateWay();
                var clientToken = gateway.ClientToken.Generate();
                ViewBag.ClientToken = clientToken;
            }
            else
            {
                //traer el usuario que esta conectado
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

                usuarioAplicacion = _UsuarioRepo.ObtenerPrimero(u => u.Id == claim.Value);
            }


            List<CarroCompras> carroCompraLista = new List<CarroCompras>();
            if (HttpContext.Session.Get<IEnumerable<CarroCompras>>(WC.SessionCarroCompras) != null
                && HttpContext.Session.Get<IEnumerable<CarroCompras>>(WC.SessionCarroCompras).Count() > 0)
            {
                carroCompraLista = HttpContext.Session.Get<List<CarroCompras>>(WC.SessionCarroCompras);
            }
            List<int> prodEnCarro = carroCompraLista.Select(i => i.ProductoId).ToList();
            //IEnumerable<Producto> prodList = _db.Productos.Where(p => prodEnCarro.Contains(p.Id));
            IEnumerable<Producto> prodList = _prodRepo.ObtenerTodos(p => prodEnCarro.Contains(p.Id) && !p.IsDeleted && p.IsActive);

            userProductosVM = new UserProductosVM()
            {
                //UsuarioAplicacion = _db.UsuariosAplicaciones.FirstOrDefault(u => u.Id == claim.Value),
                UsuarioAplicacion = usuarioAplicacion
            };
            foreach(var carro in carroCompraLista)
            {
                Producto prodTemp = _prodRepo.ObtenerPrimero(p => p.Id == carro.ProductoId && !p.IsDeleted && p.IsActive);
                if (prodTemp == null)
                {
                    continue;
                }
                prodTemp.TempCantidad = carro.Cantidad;
                userProductosVM.ProductoLista.Add(prodTemp);
            }
            return View(userProductosVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Resumen")]
       public async Task<IActionResult> ResumenPost(IFormCollection collection, UserProductosVM userProductosVM)
        {
            if (!User.IsInRole(WC.AdminRole))
            {
                return RedirectToAction("Index", "Checkout");
            }

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (User.IsInRole(WC.AdminRole))
            {
                //Se crea la venta si es administrador
                Venta venta = new Venta()
                {
                    CreadoPorUsusarioId = claim.Value,
                    FinalVentaTotal = userProductosVM.ProductoLista.Sum(x => x.TempCantidad * x.Precio),
                    Direccion = userProductosVM.UsuarioAplicacion.Direccion,
                    Ciudad = userProductosVM.UsuarioAplicacion.Ciudad,
                    Telefono = userProductosVM.UsuarioAplicacion.PhoneNumber,
                    NombreCompleto = userProductosVM.UsuarioAplicacion.NombreCompleto,
                    FechaVenta = DateTime.Now,
                    EstadoVenta = WC.EstadoPendiente,
                    Email = userProductosVM.UsuarioAplicacion.Email
                };

                //se graba la transaccion venta
                _ventaRepo.Agregar(venta);
                _ventaRepo.Grabar();

                //se graba las lineas del detalle mediante un foreach
                foreach(var item in userProductosVM.ProductoLista)
                {
                    VentaDetalle ventaDetalle = new VentaDetalle()
                    {
                        VentaId = venta.Id,
                        Precio = item.Precio,
                        Cantidad = item.TempCantidad,
                        ProductoId = item.Id
                    };
                    _ventaDetalleRepo.Agregar(ventaDetalle);
                }
                _ventaDetalleRepo.Grabar();

                string nonceFromTheClient = collection["payment_method_nonce"];

                var request = new TransactionRequest
                {
                    Amount = Convert.ToDecimal(venta.FinalVentaTotal),
                    PaymentMethodNonce = nonceFromTheClient,
                    OrderId = venta.Id.ToString(),
                    Options = new TransactionOptionsRequest
                    {
                        SubmitForSettlement = true
                    }
                };

                var gateway = _brainTreeGate.GetGateWay();
                Result<Transaction> result = gateway.Transaction.Sale(request);

                //se modifica la transaccion de venta
                if(result.Target.ProcessorResponseText == "Approved")
                {
                    venta.TransaccionId = result.Target.Id;
                    venta.EstadoVenta = WC.EstadoAprobado;
                }
                else
                {
                    venta.EstadoVenta = WC.EstadoCancelado;
                }
                _ventaRepo.Grabar();

                return RedirectToAction(nameof(Confirmacion), new {id=venta.Id});
            }
            else
            //se envia la venta
            {
                var rutaTemplate = _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString() +
                "Templates" + Path.DirectorySeparatorChar.ToString() + "PlantillaOrden.html";
                var subjecct = "Nueva orden";
                string HtmlBody = "";

                using (StreamReader sr = System.IO.File.OpenText(rutaTemplate))
                {
                    HtmlBody = sr.ReadToEnd();
                }

                StringBuilder productoListaSB = new StringBuilder();

                foreach (var prod in userProductosVM.ProductoLista)
                {
                    productoListaSB.Append($" - Nombre: {prod.NombreProducto} <span style='font-size14px;'> (Id: {prod.Id})</span><br />");
                }

                string messageBody = string.Format(HtmlBody,
                    userProductosVM.UsuarioAplicacion.NombreCompleto,
                    userProductosVM.UsuarioAplicacion.Email,
                    userProductosVM.UsuarioAplicacion.PhoneNumber,
                    productoListaSB.ToString());

                await _emailSender.SendEmailAsync(WC.EmailAdmin, subjecct, messageBody);

                //Grabar la orden y detalle
                Orden orden = new Orden()
                {
                    usuarioId = claim.Value,
                    NombreCompleto = userProductosVM.UsuarioAplicacion.NombreCompleto,
                    Email = userProductosVM.UsuarioAplicacion.Email,
                    Telefono = userProductosVM.UsuarioAplicacion.PhoneNumber,
                    FechaOrden = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    Estado = WC.EstadoPendiente,
                    DireccionEnvio = userProductosVM.UsuarioAplicacion.Direccion ?? "No especificada",
                    CiudadEnvio = userProductosVM.UsuarioAplicacion.Ciudad ?? "No especificada",
                    Total = userProductosVM.ProductoLista.Sum(x => x.TempCantidad * x.Precio)
                };

                _orderRepo.Agregar(orden);
                _orderRepo.Grabar();

                foreach (var prod in userProductosVM.ProductoLista)
                {
                    OrdenDetalle ordenDetalle = new OrdenDetalle()
                    {
                        OrdenId = orden.Id,
                        ProductoId = prod.Id,
                        Cantidad = prod.TempCantidad,
                        UnitPrice = prod.Precio
                    };
                    _orderDetalleRepo.Agregar(ordenDetalle);
                }
                _orderDetalleRepo.Grabar();
            }
            
            return RedirectToAction(nameof(Confirmacion));
        }

        public IActionResult Confirmacion(int id=0)
        {
            Venta venta = _ventaRepo.ObtenerPrimero(v => v.Id == id);
            HttpContext.Session.Clear();
            return View(venta);
        }

        public IActionResult Remover(int Id)
        {
            List<CarroCompras> carroCompraLista = new List<CarroCompras>();
            if (HttpContext.Session.Get<IEnumerable<CarroCompras>>(WC.SessionCarroCompras) != null
                && HttpContext.Session.Get<IEnumerable<CarroCompras>>(WC.SessionCarroCompras).Count() > 0)
            {
                carroCompraLista = HttpContext.Session.Get<List<CarroCompras>>(WC.SessionCarroCompras);
            }
            carroCompraLista.Remove(carroCompraLista.FirstOrDefault(p => p.ProductoId == Id));
            //se actualiza la sesion
            HttpContext.Session.Set(WC.SessionCarroCompras, carroCompraLista);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ActualizarCarro(IEnumerable<Producto> ProdLista)
        {
            List<CarroCompras> carroComprasLista = new List<CarroCompras>();
            List<string> stockErrors = new();
            foreach (Producto prod in ProdLista)
            {
                Producto existing = _prodRepo.ObtenerPrimero(p => p.Id == prod.Id && !p.IsDeleted && p.IsActive);
                if (existing == null || existing.Stock <= 0)
                {
                    stockErrors.Add($"'{prod.NombreProducto}' ya no esta disponible.");
                    continue;
                }

                int cantidad = prod.TempCantidad < 1 ? 1 : prod.TempCantidad;
                if (cantidad > existing.Stock)
                {
                    stockErrors.Add($"'{existing.NombreProducto}' solo tiene {existing.Stock} unidades disponibles.");
                    continue;
                }

                carroComprasLista.Add(new CarroCompras
                {
                    ProductoId = prod.Id,
                    Cantidad = cantidad
                });
            }
            HttpContext.Session.Set(WC.SessionCarroCompras,carroComprasLista);

            if (stockErrors.Count > 0)
            {
                TempData[WC.Error] = string.Join(" ", stockErrors);
            }

            return RedirectToAction("Index");
        }

        public IActionResult Limpiar()
        {
            HttpContext.Session.Clear();
            return RedirectToActionPermanent("Index","Home");
        }
    }
}
