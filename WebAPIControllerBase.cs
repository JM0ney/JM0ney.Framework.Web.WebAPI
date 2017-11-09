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

    public abstract class WebAPIControllerBase : System.Web.Http.ApiController, IWebAPIController {

        public abstract Data.IDataAdapter Adapter { get; }

        protected System.Transactions.TransactionScope NewTransactionScope( ) {
            System.Transactions.TransactionOptions options = new System.Transactions.TransactionOptions( );
            options.IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted;
            options.Timeout = TimeSpan.MaxValue;
            return new System.Transactions.TransactionScope( System.Transactions.TransactionScopeOption.RequiresNew, options );
        }

    }

}
