using System;
using System.Collections.Generic;
using System.Linq;
using EnterpriseWebLibrary;
using EnterpriseWebLibrary.EnterpriseWebFramework;
using EwlRealWorld.Library;
using EwlRealWorld.Library.DataAccess;
using EwlRealWorld.Library.DataAccess.CommandConditions;
using EwlRealWorld.Library.DataAccess.Modification;
using EwlRealWorld.Library.DataAccess.Retrieval;
using EwlRealWorld.Library.DataAccess.TableRetrieval;
using Humanizer;
using Markdig;
using Tewl.Tools;

// Parameter: int articleId

namespace EwlRealWorld.Website.Pages {
	partial class Article: EwfPage {
		partial class Info {
			internal ArticlesRetrieval.Row Article { get; private set; }

			protected override void init() {
				Article = ArticlesRetrieval.GetRowMatchingId( ArticleId );
			}

			public override string ResourceName => Article.Title;
		}

		private CommentsModification commentMod;

		protected override PageContent getContent() =>
			new UiPageContent(
					pageActions: AppTools.User != null && info.Article.AuthorId == AppTools.User.UserId
						             ? new HyperlinkSetup(
								             Editor.GetInfo( info.ArticleId ),
								             "Edit Article",
								             icon: new ActionComponentIcon( new FontAwesomeIcon( "fa-pencil" ) ) ).Append<ActionComponentSetup>(
								             new ButtonSetup(
									             "Delete Article",
									             behavior: new PostBackBehavior(
										             postBack: PostBack.CreateFull(
											             id: "delete",
											             modificationMethod: () => {
												             ArticleTagsModification.DeleteRows( new ArticleTagsTableEqualityConditions.ArticleId( info.ArticleId ) );
												             ArticlesModification.DeleteRows( new ArticlesTableEqualityConditions.ArticleId( info.ArticleId ) );
											             },
											             actionGetter: () => new PostBackAction( Home.GetInfo() ) ) ),
									             icon: new ActionComponentIcon( new FontAwesomeIcon( "fa-trash" ) ) ) )
							             .Materialize()
						             : AppStatics.GetFollowAction( info.Article.AuthorId ).Append( getFavoriteAction() ).Materialize() )
				.Add( AppStatics.GetAuthorDisplay( info.Article, UsersTableRetrieval.GetRowMatchingId( info.Article.AuthorId ) ) )
				.Add( new HtmlBlockContainer( Markdown.ToHtml( info.Article.BodyMarkdown ) ) )
				.Add( AppStatics.GetTagDisplay( info.ArticleId, ArticleTagsTableRetrieval.GetRowsLinkedToArticle( info.ArticleId ) ) )
				.Add( getCommentComponents() );

		private ActionComponentSetup getFavoriteAction() {
			var text = "{0} Article ({1})".FormatWith(
				AppTools.User == null || FavoritesTableRetrieval.GetRowMatchingPk( AppTools.User.UserId, info.ArticleId, returnNullIfNoMatch: true ) == null
					? "Favorite"
					: "Unfavorite",
				FavoritesTableRetrieval.GetRows( new FavoritesTableEqualityConditions.ArticleId( info.ArticleId ) ).Count() );
			var icon = new ActionComponentIcon( new FontAwesomeIcon( "fa-heart" ) );

			return AppTools.User == null
				       ? (ActionComponentSetup)new HyperlinkSetup( Pages.User.GetInfo(), text, icon: icon )
				       : new ButtonSetup(
					       text,
					       behavior: new PostBackBehavior(
						       postBack: FavoritesTableRetrieval.GetRowMatchingPk( AppTools.User.UserId, info.ArticleId, returnNullIfNoMatch: true ) == null
							                 ? PostBack.CreateFull(
								                 id: "favorite",
								                 modificationMethod: () => FavoritesModification.InsertRow( AppTools.User.UserId, info.ArticleId ) )
							                 : PostBack.CreateFull(
								                 id: "unfavorite",
								                 modificationMethod: () => FavoritesModification.DeleteRows(
									                 new FavoritesTableEqualityConditions.UserId( AppTools.User.UserId ),
									                 new FavoritesTableEqualityConditions.ArticleId( info.ArticleId ) ) ) ),
					       icon: icon );
		}

		private IReadOnlyCollection<FlowComponent> getCommentComponents() {
			var components = new List<FlowComponent>();

			var createUpdateRegions = new UpdateRegionSet();
			components.AddRange( getNewCommentComponents( createUpdateRegions ) );

			var usersById = UsersTableRetrieval.GetRows().ToIdDictionary();
			components.Add(
				new StackList(
					CommentsTableRetrieval.GetRows( new CommentsTableEqualityConditions.ArticleId( info.ArticleId ) )
						.OrderByDescending( i => i.CommentId )
						.Select(
							i => {
								var deleteUpdateRegions = new UpdateRegionSet();
								return new Paragraph( i.BodyText.ToComponents() ).Append( getCommentFooter( i, usersById[ i.AuthorId ], deleteUpdateRegions ) )
									.Materialize()
									.ToComponentListItem( i.CommentId.ToString(), removalUpdateRegionSets: deleteUpdateRegions.ToCollection() );
							} ),
					setup: new ComponentListSetup(
						classes: ElementClasses.Comment,
						itemInsertionUpdateRegions: AppTools.User != null
							                            ? new ItemInsertionUpdateRegion(
								                            createUpdateRegions.ToCollection(),
								                            () => commentMod.CommentId.ToString().ToCollection() ).ToCollection()
							                            : null ) ) );

			return components;
		}

		private IReadOnlyCollection<FlowComponent> getNewCommentComponents( UpdateRegionSet createUpdateRegions ) {
			if( AppTools.User == null )
				return new Paragraph(
					new EwfHyperlink(
							EnterpriseWebLibrary.EnterpriseWebFramework.EwlRealWorld.Website.UserManagement.LogIn.GetInfo( Home.GetInfo().GetUrl() ),
							new StandardHyperlinkStyle( "Sign in" ) ).Concat( " or ".ToComponents() )
						.Append( new EwfHyperlink( Pages.User.GetInfo(), new StandardHyperlinkStyle( "sign up" ) ) )
						.Concat( " to add comments on this article.".ToComponents() )
						.Materialize() ).ToCollection();

			commentMod = getCommentMod();
			return FormState.ExecuteWithDataModificationsAndDefaultAction(
				PostBack.CreateIntermediate(
						createUpdateRegions.ToCollection(),
						id: "comment",
						modificationMethod: () => {
							commentMod.CommentId = MainSequence.GetNextValue();
							commentMod.Execute();
						} )
					.ToCollection(),
				() => new FlowIdContainer(
					commentMod.GetBodyTextTextControlFormItem(
							false,
							label: Enumerable.Empty<PhrasingComponent>().Materialize(),
							controlSetup: TextControlSetup.Create( numberOfRows: 3, placeholder: "Write a comment..." ),
							value: "" )
						.ToComponentCollection()
						.Append( new EwfButton( new StandardButtonStyle( "Post Comment" ) ) ),
					updateRegionSets: createUpdateRegions.ToCollection() ).ToCollection() );
		}

		private CommentsModification getCommentMod() {
			var mod = CommentsModification.CreateForInsert();
			mod.AuthorId = AppTools.User.UserId;
			mod.ArticleId = info.ArticleId;
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
											deleteUpdateRegions.ToCollection(),
											id: PostBack.GetCompositeId( "delete", comment.CommentId.ToString() ),
											modificationMethod: () => CommentsModification.DeleteRows( new CommentsTableEqualityConditions.CommentId( comment.CommentId ) ) ) ) )
								.ToCollection()
							: Enumerable.Empty<PhrasingComponent>() )
					.Materialize() );
	}
}