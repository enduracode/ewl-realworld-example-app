using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using EnterpriseWebLibrary;
using EnterpriseWebLibrary.EnterpriseWebFramework;
using EnterpriseWebLibrary.EnterpriseWebFramework.Controls;
using EwlRealWorld.Library;
using EwlRealWorld.Library.DataAccess.Retrieval;
using EwlRealWorld.Library.DataAccess.TableRetrieval;
using Humanizer;

// PageState: string filter

namespace EwlRealWorld.Website.Pages {
	partial class Home: EwfPage {
		protected override void loadData() {
			if( AppTools.User == null )
				ph.AddControlsReturnThis(
					new GenericFlowContainer(
							new GenericFlowContainer( EwfApp.Instance.AppDisplayName.ToComponents() ).ToCollection<FlowComponent>()
								.Append( new Paragraph( "A place to share your knowledge.".ToComponents() ) )
								.Materialize(),
							classes: ElementClasses.Banner ).ToCollection()
						.GetControls() );

			var articleRs = new UpdateRegionSet();
			ph.AddControlsReturnThis(
				new Block( getArticleSection( articleRs ).ToCollection().Concat( getTagSection( articleRs ).ToCollection().GetControls() ).ToArray() )
					{
						CssClass = CssClasses.HomeContainer
					} );
		}

		private Control getArticleSection( UpdateRegionSet articleRs ) {
			var controls = new List<Control>();

			var filter = getFilter( AppTools.User != null ? "user" : "global" );

			var navItems = new List<LineListItem>();
			if( AppTools.User != null ) {
				const string userLabel = "Your Feed";
				navItems.Add(
					( filter == "user"
						  ? userLabel.ToComponents()
						  : new EwfButton(
								  new StandardButtonStyle( userLabel ),
								  behavior: new PostBackBehavior(
									  postBack: PostBack.CreateIntermediate( articleRs.ToCollection(), id: "user", firstModificationMethod: () => setFilter( "user" ) ) ) )
							  .ToCollection() ).ToComponentListItem() );
			}
			const string globalLabel = "Global Feed";
			navItems.Add(
				( filter != "user" && !filter.StartsWith( "tag" )
					  ? globalLabel.ToComponents()
					  : new EwfButton(
							  new StandardButtonStyle( globalLabel ),
							  behavior: new PostBackBehavior(
								  postBack: PostBack.CreateIntermediate( articleRs.ToCollection(), id: "global", firstModificationMethod: () => setFilter( "global" ) ) ) )
						  .ToCollection() ).ToComponentListItem() );
			if( filter.StartsWith( "tag" ) )
				navItems.Add(
					new FontAwesomeIcon( "fa-hashtag" ).ToCollection<PhrasingComponent>()
						.Concat( " {0}".FormatWith( TagsTableRetrieval.GetRowMatchingId( int.Parse( filter.Substring( 3 ) ) ).TagName ).ToComponents() )
						.Materialize()
						.ToComponentListItem() );
			controls.AddRange( new LineList( navItems, verticalAlignment: FlexboxVerticalAlignment.Center ).ToCollection().GetControls() );

			var results = filter == "user" && AppTools.User != null ? ArticlesRetrieval.GetRowsLinkedToFollower( AppTools.User.UserId ) :
			              filter.StartsWith( "tag" ) ? ArticlesRetrieval.GetRowsLinkedToTag( int.Parse( filter.Substring( 3 ) ) ) :
			              ArticlesRetrieval.GetRowsOrderedByCreation();
			var usersById = UsersTableRetrieval.GetRows().ToIdDictionary();
			var tagsByArticleId = ArticleTagsTableRetrieval.GetRows().ToArticleIdLookup();
			var favoritesByArticleId = FavoritesTableRetrieval.GetRows().ToLookup( i => i.ArticleId );

			var table = EwfTable.Create( defaultItemLimit: DataRowLimit.Fifty );
			table.AddData( results, i => new EwfTableItem( AppStatics.GetArticleDisplay( i, usersById, tagsByArticleId, favoritesByArticleId ).ToCell() ) );
			controls.Add( new NamingPlaceholder( table.ToCollection(), updateRegionSets: articleRs.ToCollection() ) );

			return new LegacySection( controls );
		}

		private FlowComponent getTagSection( UpdateRegionSet articleRs ) {
			TagsTableRetrieval.GetRows(); // prime cache for next line
			var tags = ArticleTagsTableRetrieval.GetRows()
				.Select( i => i.TagId )
				.GroupBy( i => i )
				.OrderByDescending( i => i.Count() )
				.Take( 20 )
				.Select( i => TagsTableRetrieval.GetRowMatchingId( i.Key ) );

			return new Section(
				"Popular Tags",
				tags.Select(
						i => new EwfButton(
							new StandardButtonStyle( i.TagName, buttonSize: ButtonSize.ShrinkWrap ),
							behavior: new PostBackBehavior(
								postBack: PostBack.CreateIntermediate(
									articleRs.ToCollection(),
									id: PostBack.GetCompositeId( "tag", i.TagId.ToString() ),
									firstModificationMethod: () => setFilter( "tag{0}".FormatWith( i.TagId ) ) ) ) ) )
					.Materialize(),
				style: SectionStyle.Box );
		}
	}
}