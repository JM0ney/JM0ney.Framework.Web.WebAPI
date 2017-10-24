using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JM0ney.Framework.Web.WebAPI {

    public interface IWebAPIController : IDisposable {

        Data.IDataAdapter Adapter { get; }

    }

}
