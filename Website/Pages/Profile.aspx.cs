using EnterpriseWebLibrary.EnterpriseWebFramework;
using EwlRealWorld.Library.DataAccess.TableRetrieval;

// Parameter: int userId
// OptionalParameter: bool showFavorites

namespace EwlRealWorld.Website.Pages {
	partial class Profile: EwfPage {
		partial class Info {
			internal UsersTableRetrieval.Row User { get; private set; }

			protected override void init() {
				User = UsersTableRetrieval.GetRowMatchingId( UserId );
			}
		}

		protected override void loadData() {}
	}
}