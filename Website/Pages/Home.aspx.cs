using System.Collections.Generic;
using System.Linq;
using EnterpriseWebLibrary;
using EnterpriseWebLibrary.EnterpriseWebFramework;
using EwlRealWorld.Library;
using EwlRealWorld.Library.DataAccess.Retrieval;
using EwlRealWorld.Library.DataAccess.TableRetrieval;
using Humanizer;
using Tewl.Tools;

// PageState: string filter

namespace EwlRealWorld.Website.Pages {
	partial class Home: EwfPage {
		protected override void loadData() {
			if( AppTools.User == null )
				ph.AddControlsReturnThis(
					new GenericFlowContainer(
							new GenericFlowContainer( EwfApp.Instance.AppDisplayName.ToComponents() )
								.Append<FlowComponent>( new Paragraph( "A place to share your knowledge.".ToComponents() ) )
								.Materialize(),
							classes: ElementClasses.Banner ).ToCollection()
						.GetControls() );

			var resultUpdateRegions = new UpdateRegionSet();
			ph.AddControlsReturnThis(
				new GenericFlowContainer(
						getArticleSection( resultUpdateRegions ).Append( getTagSection( resultUpdateRegions ) ).Materialize(),
						classes: ElementClasses.HomeContainer ).ToCollection()
					.GetControls() );
		}

		private FlowComponent getArticleSection( UpdateRegionSet resultUpdateRegions ) {
			var filter = getFilter( AppTools.User != null ? "user" : "global" );
			return new Section(
				new LineList(
						new[] { getUserTabComponents( filter, resultUpdateRegions ), getGlobalTabComponents( filter, resultUpdateRegions ), getTagTabComponents( filter ) }
							.Where( i => i.Any() )
							.Select( i => (LineListItem)i.ToComponentListItem() ),
						verticalAlignment: FlexboxVerticalAlignment.Center ).Append<FlowComponent>(
						new FlowIdContainer( getResultTable( filter ).ToCollection(), updateRegionSets: resultUpdateRegions.ToCollection() ) )
					.Materialize() );
		}

		private IReadOnlyCollection<PhrasingComponent> getUserTabComponents( string filter, UpdateRegionSet resultUpdateRegions ) {
			const string label = "Your Feed";
			return AppTools.User != null
				       ? filter == "user" ? label.ToComponents() :
				         new EwfButton(
					         new StandardButtonStyle( label ),
					         behavior: new PostBackBehavior(
						         postBack: PostBack.CreateIntermediate(
							         resultUpdateRegions.ToCollection(),
							         id: "user",
							         firstModificationMethod: () => setFilter( "user" ) ) ) ).ToCollection()
				       : Enumerable.Empty<PhrasingComponent>().Materialize();
		}

		private IReadOnlyCollection<PhrasingComponent> getGlobalTabComponents( string filter, UpdateRegionSet resultUpdateRegions ) {
			const string label = "Global Feed";
			return filter != "user" && !filter.StartsWith( "tag" )
				       ? label.ToComponents()
				       : new EwfButton(
					       new StandardButtonStyle( label ),
					       behavior: new PostBackBehavior(
						       postBack: PostBack.CreateIntermediate(
							       resultUpdateRegions.ToCollection(),
							       id: "global",
							       firstModificationMethod: () => setFilter( "global" ) ) ) ).ToCollection();
		}

		private IReadOnlyCollection<PhrasingComponent> getTagTabComponents( string filter ) =>
			filter.StartsWith( "tag" )
				? new FontAwesomeIcon( "fa-hashtag" )
					.Concat( " {0}".FormatWith( TagsTableRetrieval.GetRowMatchingId( int.Parse( filter.Substring( 3 ) ) ).TagName ).ToComponents() )
					.Materialize()
				: Enumerable.Empty<PhrasingComponent>().Materialize();

		private FlowComponent getResultTable( string filter ) {
			var results = filter == "user" && AppTools.User != null ? ArticlesRetrieval.GetRowsLinkedToFollower( AppTools.User.UserId ) :
			              filter.StartsWith( "tag" ) ? ArticlesRetrieval.GetRowsLinkedToTag( int.Parse( filter.Substring( 3 ) ) ) :
			              ArticlesRetrieval.GetRowsOrderedByCreation();
			var usersById = UsersTableRetrieval.GetRows().ToIdDictionary();
			var tagsByArticleId = ArticleTagsTableRetrieval.GetRows().ToArticleIdLookup();
			var favoritesByArticleId = FavoritesTableRetrieval.GetRows().ToArticleIdLookup();

			var table = EwfTable.Create( defaultItemLimit: DataRowLimit.Fifty );
			table.AddData( results, i => EwfTableItem.Create( AppStatics.GetArticleDisplay( i, usersById, tagsByArticleId, favoritesByArticleId ).ToCell() ) );
			return table;
		}

		private FlowComponent getTagSection( UpdateRegionSet resultUpdateRegions ) {
			var tags = ArticleTagsTableRetrieval.GetRows()
				.Select( i => i.TagId )
				.GroupBy( i => i )
				.OrderByDescending( i => i.Count() )
				.Take( 20 )
				.Select( i => TagsTableRetrieval.GetRowMatchingId( i.Key ) );

			return new Section(
				"Popular Tags",
				new WrappingList(
					tags.Select(
						i => (WrappingListItem)new EwfButton(
							new StandardButtonStyle( i.TagName, buttonSize: ButtonSize.ShrinkWrap ),
							behavior: new PostBackBehavior(
								postBack: PostBack.CreateIntermediate(
									resultUpdateRegions.ToCollection(),
									id: PostBack.GetCompositeId( "tag", i.TagId.ToString() ),
									firstModificationMethod: () => setFilter( "tag{0}".FormatWith( i.TagId ) ) ) ) ).ToComponentListItem() ),
					generalSetup: new ComponentListSetup( classes: ElementClasses.Tag ) ).ToCollection(),
				style: SectionStyle.Box );
		}
	}
}