//
//  Do you like this project? Do you find it helpful? Pay it forward by hiring me as a consultant!
//  https://jason-iverson.com
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JM0ney.AssetLibrary.ViewModels;
using JM0ney.Framework.Data;

namespace JM0ney.Framework.Web.WebAPI {

    public abstract class WebAPIControllerBase<TViewModel, TModel, TInterface> : WebAPIControllerBase
        where TViewModel : AssetLibrary.ViewModels.Base.ViewModelBase, TInterface, IAssumable<TInterface>, new()
        where TModel : class, IDataObject, TInterface, IAssumable<TInterface>, new() {

        /// <summary>
        /// Copies values from the Model to the ViewModel
        /// </summary>
        /// <param name="sourceObject">Object to copy from</param>
        /// <param name="targetObject">Object to copy to</param>
        protected virtual void AssumingValuesOfModel( ref TModel sourceObject, ref TViewModel targetObject ) {
            targetObject.AssumeValuesOf( sourceObject );
        }

        /// <summary>
        /// Copies values from the ViewModel to the Model
        /// </summary>
        /// <param name="sourceObject">Object to copy from</param>
        /// <param name="targetObject">Object to copy to</param>
        protected virtual void AssumingValuesOfViewModel( ref TViewModel sourceObject, ref TModel targetObject ) {
            targetObject.AssumeValuesOf( sourceObject );
        }

        /// <summary>
        /// Saves a record that was created as a result of calling <see cref="Instantiate(Guid?)"/> previously.
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public virtual Result Create( TViewModel viewModel ) {
            Result result = Result.SuccessResult( );
            TModel model = new TModel( );
            model.Adapter = this.Adapter;
            this.AssumingValuesOfViewModel( ref viewModel, ref model );

            using ( System.Transactions.TransactionScope scope = this.NewTransactionScope( ) ) {
                result = model.Save( );
                if ( result.IsSuccess ) {
                    result = this.OnCreated( ref viewModel, ref model );
                }
                if ( result.IsSuccess )
                    scope.Complete( );
            }
            return result;
        }

        /// <summary>
        /// Executed after a record is successfully created
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        protected virtual Result OnCreated( ref TViewModel viewModel, ref TModel model ) {
            return Result.SuccessResult( );
        }
        
        /// <summary>
        /// Instantiates and returns a Result containing a new <typeparamref name="TViewModel" />
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public virtual Result<TViewModel> Instantiate( Guid? identity ) {
            Guid realIdentity = Guid.Empty;
            if ( identity.HasValue )
                realIdentity = identity.Value;
            TModel model = this.OnInstantiateModel( identity );
            TViewModel viewModel = this.OnInstantiateViewModel( );

            model.Adapter = this.Adapter;
            viewModel.EditorMode = EditorModes.Creating;
            this.AssumingValuesOfModel( ref model, ref viewModel );
            return Result.SuccessResult<TViewModel>( String.Empty, viewModel );
        }

        /// <summary>
        /// Loads an existing <see cref="TModel"/> based on the <paramref name="identity"/> provided, copies the values to a <see cref="TViewModel"/>, and returns the <see cref="TViewModel"/>.
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public Result<TViewModel> Load(Guid identity ) {
            Guid realIdentity = identity;
            TModel model = new TModel( );
            TViewModel viewModel = new TViewModel( );

            if (realIdentity != Guid.Empty ) {
                Result<TModel> loadResult = this.Adapter.Load<TModel>( realIdentity );
                if ( loadResult.IsSuccess ) {
                    model = loadResult.ReturnValue;
                }
                else {
                    viewModel = null;
                    return Result.ErrorResult<TViewModel>( loadResult.Message, null );
                }                    
            }
            else {
                model.Adapter = this.Adapter;
            }

            if (viewModel != null ) {
                viewModel.EditorMode = EditorModes.Editing;
                this.AssumingValuesOfModel( ref model, ref viewModel );
            }
            return Result.SuccessResult<TViewModel>( viewModel );
        }

        /// <summary>
        /// Returns a populated <see cref="List<typeparamref name="TViewModel"/>" />
        /// </summary>
        /// <returns></returns>
        public Result<List<TViewModel>> List( ) {
            return this.DoList( null );
        }

        /// <summary>
        /// Returns a populated <see cref="List<typeparamref name="TViewModel"/>" />
        /// </summary>
        /// <param name="keyValuePairs"></param>
        /// <returns></returns>
        public Result<List<TViewModel>> ListBy( params KeyValuePair<String, Object>[] keyValuePairs ) {
            return this.DoList( keyValuePairs );
        }

        /// <summary>
        /// Saves an existing record by retrieving a <see cref="TModel"/>, assumes the values of the <paramref name="viewModel"/>, then persisting the <see cref="TModel"/> to the persistence layer.
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public virtual Result Save( TViewModel viewModel ) {
            TModel model;
            Result<TModel> loadResult = this.Adapter.Load<TModel>( viewModel.Identity );
            if ( loadResult.IsSuccess )
                return loadResult;

            Result returnValue;
            model = loadResult.ReturnValue;
            this.AssumingValuesOfViewModel( ref viewModel, ref model );

            using(System.Transactions.TransactionScope scope = this.NewTransactionScope( ) ) {
                returnValue = model.Save( );
                if ( returnValue.IsSuccess ) {
                    returnValue = this.OnSaved( ref viewModel, ref model );
                }
                if ( returnValue.IsSuccess )
                    scope.Complete( );
            }
            return returnValue;
        }

        protected virtual Result OnSaved( ref TViewModel viewModel, ref TModel model ) {
            return Result.SuccessResult( );
        }

        /// <summary>
        /// Deletes a previously created record according to the <paramref name="identity"/> provided.
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public virtual Result Delete( Guid identity ) {
            TModel model;
            Result<TModel> loadResult = this.Adapter.Load<TModel>( identity );
            if ( !loadResult.IsSuccess )
                return loadResult;

            Result result;
            model = loadResult.ReturnValue;
            using(System.Transactions.TransactionScope transactionScope = this.NewTransactionScope( ) ) {
                result = model.Delete( );
                if ( result.IsSuccess )
                    result = this.OnDeleted( ref model );
                if ( result.IsSuccess )
                    transactionScope.Complete( );
            }
            return result;
        }

        protected virtual Result OnDeleted( ref TModel model ) {
            return Result.SuccessResult( );
        }

        public String FriendlyNameSingular {
            get { return this.GetModelInstance( ).Metadata.FriendlyNameSingular; }
        }

        public String FriendlyNamePlural {
            get { return this.GetModelInstance( ).Metadata.FriendlyNamePlural; }
        }

        private Result<List<TViewModel>> DoList( params KeyValuePair<String, Object>[ ] keyValuePairs ) {
            Result<IEnumerable<TModel>> result;
            List<TViewModel> returnValue = new List<TViewModel>( );
            if ( keyValuePairs == null || keyValuePairs.Length == 0 )
                result = this.Adapter.List<TModel>( );
            else
                result = this.Adapter.ListBy<TModel>( keyValuePairs );

            if ( !result.IsSuccess )
                return Result.ErrorResult<List<TViewModel>>( result.Message, null );

            for ( int i = 0; i < result.ReturnValue.Count( ); i++ ) {
                TViewModel tempViewModel = new TViewModel( );
                TModel model = result.ReturnValue.ElementAt( i );
                this.AssumingValuesOfModel( ref model, ref tempViewModel );
                returnValue.Add( tempViewModel );
            }
            return Result.SuccessResult<List<TViewModel>>( returnValue );
        }

        private TModel GetModelInstance( ) {
            if ( this._Model == null )
                this._Model = new TModel( );
            return this._Model;
        }
        private TModel _Model = null;

        protected virtual TModel OnInstantiateModel( Guid? identity ) {
            return new TModel( );
        }

        protected virtual TViewModel OnInstantiateViewModel( ) {
            return new TViewModel( );
        }

    }

}
