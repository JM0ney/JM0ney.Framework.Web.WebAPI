//
//  Do you like this project? Do you find it helpful? Pay it forward by hiring me as a consultant!
//  https://jason-iverson.com
//
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
