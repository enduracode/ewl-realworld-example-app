using System.Collections.Generic;
using System.Linq;
using EnterpriseWebLibrary;
using EnterpriseWebLibrary.EnterpriseWebFramework;
using EwlRealWorld.Library;
using EwlRealWorld.Library.DataAccess.TableRetrieval;
using EwlRealWorld.Website.Pages;

namespace EwlRealWorld.Website {
	internal static class AppStatics {
		internal static IReadOnlyCollection<FlowComponent> GetAuthorDisplay( ArticlesTableRetrieval.Row article ) {
			var author = UsersTableRetrieval.GetRowMatchingId( article.AuthorId );
			return new GenericFlowContainer(
				new EwfHyperlink(
						Profile.GetInfo( article.AuthorId ),
						new ImageHyperlinkStyle(
							new ExternalResourceInfo(
								author.ProfilePictureUrl.Any() ? author.ProfilePictureUrl : "https://static.productionready.io/images/smiley-cyrus.jpg" ),
							"" ) ).ToCollection<PhrasingComponent>()
					.Append(
						new GenericPhrasingContainer(
							new EwfHyperlink( Profile.GetInfo( article.AuthorId ), new StandardHyperlinkStyle( author.Username ) ).ToCollection<PhrasingComponent>()
								.Append( new LineBreak() )
								.Append(
									new GenericPhrasingContainer( article.CreationDateAndTime.ToDayMonthYearString( false ).ToComponents(), classes: ElementClasses.Date ) )
								.Materialize() ) )
					.Materialize(),
				classes: ElementClasses.Author ).ToCollection();
		}
	}
}