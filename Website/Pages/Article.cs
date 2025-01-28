using EnterpriseWebLibrary.UserManagement;
using EwlRealWorld.Library;
using EwlRealWorld.Library.DataAccess;
using EwlRealWorld.Library.DataAccess.CommandConditions;
using EwlRealWorld.Library.DataAccess.Modification;
using EwlRealWorld.Library.DataAccess.Retrieval;
using EwlRealWorld.Library.DataAccess.TableRetrieval;
using Markdig;

namespace EwlRealWorld.Website.Pages;

// EwlPage
// Parameter: int articleId
partial class Article {
	private ArticlesRetrieval.Row articleRow = null!;
	private CommentsModification? commentMod;

	protected override void init() {
		articleRow = ArticlesRetrieval.GetRowMatchingId( ArticleId );
	}

	protected override string getResourceName() => articleRow.Title;

	protected override UrlHandler getUrlParent() => new Home();

	protected override PageContent getContent() =>
		new UiPageContent(
				pageActions: AppTools.User != null && articleRow.AuthorId == AppTools.User.UserId
					             ? new HyperlinkSetup( Editor.GetInfo( ArticleId ), "Edit Article", icon: new ActionComponentIcon( new FontAwesomeIcon( "fa-pencil" ) ) )
						             .Append<ActionComponentSetup>(
							             new ButtonSetup(
								             "Delete Article",
								             behavior: new PostBackBehavior(
									             postBack: PostBack.CreateFull(
										             id: "delete",
										             modificationMethod: () => {
											             ArticleTagsModification.DeleteRows( new ArticleTagsTableEqualityConditions.ArticleId( ArticleId ) );
											             ArticlesModification.DeleteRows( new ArticlesTableEqualityConditions.ArticleId( ArticleId ) );
										             },
										             actionGetter: () => new PostBackAction( Home.GetInfo() ) ) ),
								             icon: new ActionComponentIcon( new FontAwesomeIcon( "fa-trash" ) ) ) )
						             .Materialize()
					             : AppStatics.GetFollowAction( articleRow.AuthorId ).Append( getFavoriteAction() ).Materialize() )
			.Add( AppStatics.GetAuthorDisplay( articleRow, UsersTableRetrieval.GetRowMatchingId( articleRow.AuthorId ) ) )
			.Add( new HtmlBlockContainer( Markdown.ToHtml( articleRow.BodyMarkdown ) ) )
			.Add( AppStatics.GetTagDisplay( ArticleId, ArticleTagsTableRetrieval.GetRowsLinkedToArticle( ArticleId ) ) )
			.Add( getCommentComponents() );

	private ActionComponentSetup getFavoriteAction() {
		var text = "{0} Article ({1})".FormatWith(
			AppTools.User == null || !FavoritesTableRetrieval.TryGetRowMatchingPk( AppTools.User.UserId, ArticleId, out _ ) ? "Favorite" : "Unfavorite",
			FavoritesTableRetrieval.GetRows( new FavoritesTableEqualityConditions.ArticleId( ArticleId ) ).Count );
		var icon = new ActionComponentIcon( new FontAwesomeIcon( "fa-heart" ) );

		return AppTools.User == null
			       ? new HyperlinkSetup( User.GetInfo(), text, icon: icon )
			       : new ButtonSetup(
				       text,
				       behavior: new PostBackBehavior(
					       postBack: !FavoritesTableRetrieval.TryGetRowMatchingPk( AppTools.User.UserId, ArticleId, out _ )
						                 ? PostBack.CreateFull(
							                 id: "favorite",
							                 modificationMethod: () => FavoritesModification.InsertRow( AppTools.User.UserId, ArticleId ) )
						                 : PostBack.CreateFull(
							                 id: "unfavorite",
							                 modificationMethod: () => FavoritesModification.DeleteRows(
								                 new FavoritesTableEqualityConditions.UserId( AppTools.User.UserId ),
								                 new FavoritesTableEqualityConditions.ArticleId( ArticleId ) ) ) ),
				       icon: icon );
	}

	private IReadOnlyCollection<FlowComponent> getCommentComponents() {
		var components = new List<FlowComponent>();

		var createUpdateRegions = new UpdateRegionSet();
		components.AddRange( getNewCommentComponents( createUpdateRegions ) );

		var usersById = UsersTableRetrieval.GetRows().ToIdDictionary();
		components.Add(
			new StackList(
				CommentsTableRetrieval.GetRows( new CommentsTableEqualityConditions.ArticleId( ArticleId ) )
					.OrderByDescending( i => i.CommentId )
					.Select(
						i => {
							var deleteUpdateRegions = new UpdateRegionSet();
							return new Paragraph( i.BodyText.ToComponents() ).Append( getCommentFooter( i, usersById[ i.AuthorId ], deleteUpdateRegions ) )
								.Materialize()
								.ToComponentListItem( i.CommentId.ToString(), removalUpdateRegionSets: deleteUpdateRegions );
						} ),
				setup: new ComponentListSetup(
					classes: ElementClasses.Comment,
					itemInsertionUpdateRegions: AppTools.User != null
						                            ? new ItemInsertionUpdateRegion( createUpdateRegions, () => commentMod!.CommentId.ToString().ToCollection() )
						                            : null ) ) );

		return components;
	}

	private IReadOnlyCollection<FlowComponent> getNewCommentComponents( UpdateRegionSet createUpdateRegions ) {
		if( AppTools.User == null )
			return new Paragraph(
				new EwfHyperlink(
						EnterpriseWebLibrary.EnterpriseWebFramework.UserManagement.Pages.LogIn.GetInfo( Home.GetInfo().GetUrl() ),
						new StandardHyperlinkStyle( "Sign in" ) ).Concat( " or ".ToComponents() )
					.Append( new EwfHyperlink( User.GetInfo(), new StandardHyperlinkStyle( "sign up" ) ) )
					.Concat( " to add comments on this article.".ToComponents() )
					.Materialize() ).ToCollection();

		commentMod = getCommentMod();
		return FormState.ExecuteWithActions(
			PostBack.CreateIntermediate(
				createUpdateRegions,
				id: "comment",
				modificationMethod: () => {
					commentMod.CommentId = MainSequence.GetNextValue();
					commentMod.Execute();
				} ),
			() => new FlowIdContainer(
				commentMod.GetBodyTextTextControlFormItem(
						false,
						label: Enumerable.Empty<PhrasingComponent>().Materialize(),
						controlSetup: TextControlSetup.Create( numberOfRows: 3, placeholder: "Write a comment..." ),
						value: "" )
					.ToComponentCollection()
					.Append( new EwfButton( new StandardButtonStyle( "Post Comment" ) ) ),
				updateRegionSets: createUpdateRegions ).ToCollection() );
	}

	private CommentsModification getCommentMod() {
		var mod = CommentsModification.CreateForInsert();
		mod.AuthorId = SystemUser.Current!.UserId;
		mod.ArticleId = ArticleId;
		mod.CreationDateAndTime = DateTime.UtcNow;
		return mod;
	}

	private FlowComponent getCommentFooter( CommentsTableRetrieval.Row comment, UsersTableRetrieval.Row author, UpdateRegionSet deleteUpdateRegions ) =>
		new GenericFlowContainer(
			new GenericFlowContainer(
					new GenericPhrasingContainer(
							new EwfHyperlink(
								Profile.GetInfo( comment.AuthorId ),
								new ImageHyperlinkStyle(
									new ExternalResource(
										author.ProfilePictureUrl.Any() ? author.ProfilePictureUrl : "https://static.productionready.io/images/smiley-cyrus.jpg" ),
									"" ) ).ToCollection() )
						.Append(
							new GenericPhrasingContainer(
								new EwfHyperlink( Profile.GetInfo( comment.AuthorId ), new StandardHyperlinkStyle( author.Username ) ).ToCollection() ) )
						.Append( new GenericPhrasingContainer( comment.CreationDateAndTime.ToDayMonthYearString( false ).ToComponents() ) )
						.Materialize() ).Concat<FlowComponent>(
					comment.AuthorId == AppTools.User?.UserId
						? new EwfButton(
								new StandardButtonStyle( "Delete", icon: new ActionComponentIcon( new FontAwesomeIcon( "fa-trash" ) ) ),
								behavior: new PostBackBehavior(
									postBack: PostBack.CreateIntermediate(
										deleteUpdateRegions,
										id: PostBack.GetCompositeId( "delete", comment.CommentId.ToString() ),
										modificationMethod: () => CommentsModification.DeleteRows( new CommentsTableEqualityConditions.CommentId( comment.CommentId ) ) ) ) )
							.ToCollection()
						: Enumerable.Empty<PhrasingComponent>() )
				.Materialize() );
}