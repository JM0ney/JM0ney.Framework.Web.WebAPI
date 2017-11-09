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

    public interface IWebAPIController<TViewModel> : IWebAPIController
        where TViewModel : AssetLibrary.ViewModels.Base.ViewModelBase, new() {

        Result Delete( Guid identity );

        Result<List<TViewModel>> List( );

        Result<List<TViewModel>> ListBy( params KeyValuePair<String, Object>[ ] keyValuePairs );

        Result<TViewModel> Load( Guid identity );

        Result<TViewModel> Instantiate( Guid? identity );

        Result Create( TViewModel viewModel );

        Result Save( TViewModel viewModel );

        String FriendlyNameSingular { get; }

        String FriendlyNamePlural { get; }

    }

}
