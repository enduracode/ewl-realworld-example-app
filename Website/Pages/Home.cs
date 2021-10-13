using System.Collections.Generic;
using System.Linq;
using EnterpriseWebLibrary;
using EnterpriseWebLibrary.EnterpriseWebFramework;
using EwlRealWorld.Library;
using EwlRealWorld.Library.DataAccess.Retrieval;
using EwlRealWorld.Library.DataAccess.TableRetrieval;
using Humanizer;
using Tewl.Tools;

// EwlPage

namespace EwlRealWorld.Website.Pages {
	partial class Home {
		protected override IEnumerable<UrlPattern> getChildUrlPatterns() =>
			RequestDispatchingStatics.GetFrameworkUrlPatterns()
				.Append( User.UrlPatterns.Literal( "user" ) )
				.Append( Editor.UrlPatterns.Literal( "editor" ) )
				.Append( Article.UrlPatterns.Literal( "article" ) )
				.Append( Profile.UrlPatterns.Literal( "profile" ) );

		protected override PageContent getContent() {
			var content = new UiPageContent();

			if( AppTools.User == null )
				content.Add(
					new GenericFlowContainer(
						new GenericFlowContainer( "Conduit".ToComponents() ).Append<FlowComponent>( new Paragraph( "A place to share your knowledge.".ToComponents() ) )
							.Materialize(),
						classes: ElementClasses.Banner ) );

			var filter = ComponentStateItem.Create( "filter", AppTools.User != null ? "user" : "global", v => true, false );
			var resultUpdateRegions = new UpdateRegionSet();
			content.Add(
				new GenericFlowContainer(
					getArticleSection( filter.Value, resultUpdateRegions ).Append( getTagSection( filter.Value, resultUpdateRegions ) ).Materialize(),
					classes: ElementClasses.HomeContainer,
					etherealContent: filter.ToCollection() ) );

			return content;
		}

		private FlowComponent getArticleSection( DataValue<string> filter, UpdateRegionSet resultUpdateRegions ) =>
			new Section(
				new LineList(
						new[]
								{
									getUserTabComponents( filter, resultUpdateRegions ), getGlobalTabComponents( filter, resultUpdateRegions ),
									getTagTabComponents( filter.Value )
								}.Where( i => i.Any() )
							.Select( i => (LineListItem)i.ToComponentListItem() ),
						verticalAlignment: FlexboxVerticalAlignment.Center ).Append<FlowComponent>(
						new FlowIdContainer( getResultTable( filter.Value ).ToCollection(), updateRegionSets: resultUpdateRegions.ToCollection() ) )
					.Materialize() );

		private IReadOnlyCollection<PhrasingComponent> getUserTabComponents( DataValue<string> filter, UpdateRegionSet resultUpdateRegions ) {
			const string label = "Your Feed";
			return AppTools.User != null
				       ? filter.Value == "user" ? label.ToComponents() :
				         new EwfButton(
					         new StandardButtonStyle( label ),
					         behavior: new PostBackBehavior(
						         postBack: PostBack.CreateIntermediate(
							         resultUpdateRegions.ToCollection(),
							         id: "user",
							         modificationMethod: () => filter.Value = "user" ) ) ).ToCollection()
				       : Enumerable.Empty<PhrasingComponent>().Materialize();
		}

		private IReadOnlyCollection<PhrasingComponent> getGlobalTabComponents( DataValue<string> filter, UpdateRegionSet resultUpdateRegions ) {
			const string label = "Global Feed";
			return filter.Value != "user" && !filter.Value.StartsWith( "tag" )
				       ? label.ToComponents()
				       : new EwfButton(
					       new StandardButtonStyle( label ),
					       behavior: new PostBackBehavior(
						       postBack: PostBack.CreateIntermediate(
							       resultUpdateRegions.ToCollection(),
							       id: "global",
							       modificationMethod: () => filter.Value = "global" ) ) ).ToCollection();
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

		private FlowComponent getTagSection( DataValue<string> filter, UpdateRegionSet resultUpdateRegions ) {
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
									modificationMethod: () => filter.Value = "tag{0}".FormatWith( i.TagId ) ) ) ).ToComponentListItem() ),
					generalSetup: new ComponentListSetup( classes: ElementClasses.Tag ) ).ToCollection(),
				style: SectionStyle.Box );
		}
	}
}