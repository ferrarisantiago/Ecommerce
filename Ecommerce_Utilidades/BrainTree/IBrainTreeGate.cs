using Braintree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce_Utilidades.BrainTree
{
    public interface IBrainTreeGate
    {
        IBraintreeGateway CreateGateWay();

        IBraintreeGateway GetGateWay();
    }
}
