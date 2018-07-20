using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using EnterpriseWebLibrary;
using EnterpriseWebLibrary.EnterpriseWebFramework;
using EnterpriseWebLibrary.EnterpriseWebFramework.Controls;
using EnterpriseWebLibrary.EnterpriseWebFramework.Ui;
using EwlRealWorld.Library;
using EwlRealWorld.Library.DataAccess;
using EwlRealWorld.Library.DataAccess.CommandConditions;
using EwlRealWorld.Library.DataAccess.Modification;
using EwlRealWorld.Library.DataAccess.Retrieval;
using EwlRealWorld.Library.DataAccess.TableRetrieval;
using Humanizer;
using Markdig;

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

		protected override void loadData() {
			if( AppTools.User != null && info.Article.AuthorId == AppTools.User.UserId )
				EwfUiStatics.SetPageActions(
					ActionButtonSetup.CreateWithUrl(
						"Edit Article",
						Editor.GetInfo( info.ArticleId ),
						icon: new ActionComponentIcon( new FontAwesomeIcon( "fa-pencil" ) ) ),
					new ActionButtonSetup(
						"Delete Article",
						new PostBackButton(
							postBack: PostBack.CreateFull(
								id: "delete",
								firstModificationMethod: () => {
									ArticleTagsModification.DeleteRows( new ArticleTagsTableEqualityConditions.ArticleId( info.ArticleId ) );
									ArticlesModification.DeleteRows( new ArticlesTableEqualityConditions.ArticleId( info.ArticleId ) );
								},
								actionGetter: () => new PostBackAction( Home.GetInfo() ) ) ),
						icon: new ActionComponentIcon( new FontAwesomeIcon( "fa-trash" ) ) ) );
			else
				EwfUiStatics.SetPageActions( AppStatics.GetFollowAction( info.Article.AuthorId ), getFavoriteAction() );

			ph.AddControlsReturnThis(
				AppStatics.GetAuthorDisplay( info.Article, UsersTableRetrieval.GetRowMatchingId( info.Article.AuthorId ) )
					.Append( new HtmlBlockContainer( Markdown.ToHtml( info.Article.BodyMarkdown ) ) )
					.Concat( AppStatics.GetTagDisplay( info.ArticleId, ArticleTagsTableRetrieval.GetRowsLinkedToArticle( info.ArticleId ) ) )
					.GetControls() );

			ph.AddControlsReturnThis( getCommentControls() );
		}

		private ActionButtonSetup getFavoriteAction() {
			ActionControl actionControl;
			if( AppTools.User == null )
				actionControl = new EwfLink( Pages.User.GetInfo() );
			else if( FavoritesTableRetrieval.GetRowMatchingPk( AppTools.User.UserId, info.ArticleId, returnNullIfNoMatch: true ) == null )
				actionControl = new PostBackButton(
					postBack: PostBack.CreateFull(
						id: "favorite",
						firstModificationMethod: () => FavoritesModification.InsertRow( AppTools.User.UserId, info.ArticleId ) ) );
			else
				actionControl = new PostBackButton(
					postBack: PostBack.CreateFull(
						id: "unfavorite",
						firstModificationMethod: () => FavoritesModification.DeleteRows(
							new FavoritesTableEqualityConditions.UserId( AppTools.User.UserId ),
							new FavoritesTableEqualityConditions.ArticleId( info.ArticleId ) ) ) );

			return new ActionButtonSetup(
				"{0} Article ({1})".FormatWith(
					AppTools.User == null || FavoritesTableRetrieval.GetRowMatchingPk( AppTools.User.UserId, info.ArticleId, returnNullIfNoMatch: true ) == null
						? "Favorite"
						: "Unfavorite",
					FavoritesTableRetrieval.GetRows( new FavoritesTableEqualityConditions.ArticleId( info.ArticleId ) ).Count() ),
				actionControl,
				icon: new ActionComponentIcon( new FontAwesomeIcon( "fa-heart" ) ) );
		}

		private IReadOnlyCollection<Control> getCommentControls() {
			var controls = new List<Control>();

			CommentsModification commentMod = null;
			var commentRs = new UpdateRegionSet();
			if( AppTools.User != null ) {
				commentMod = getCommentMod();
				FormState.ExecuteWithDataModificationsAndDefaultAction(
					PostBack.CreateIntermediate(
							commentRs.ToCollection(),
							id: "comment",
							firstModificationMethod: () => {
								commentMod.CommentId = MainSequence.GetNextValue();
								commentMod.Execute();
							} )
						.ToCollection(),
					() => controls.Add(
						new NamingPlaceholder(
							commentMod.GetBodyTextTextControlFormItem(
									false,
									label: Enumerable.Empty<PhrasingComponent>().Materialize(),
									controlSetup: TextControlSetup.Create( numberOfRows: 3, placeholder: "Write a comment..." ),
									value: "" )
								.ToControl()
								.ToCollection()
								.Concat( new EwfButton( new StandardButtonStyle( "Post Comment" ) ).ToCollection().GetControls() ),
							updateRegionSets: commentRs.ToCollection() ) ) );
			}
			else
				controls.AddRange(
					new Paragraph(
							new EwfHyperlink(
									EnterpriseWebLibrary.EnterpriseWebFramework.EwlRealWorld.Website.UserManagement.LogIn.GetInfo( Home.GetInfo().GetUrl() ),
									new StandardHyperlinkStyle( "Sign in" ) ).ToCollection()
								.Concat( " or ".ToComponents() )
								.Append( new EwfHyperlink( Pages.User.GetInfo(), new StandardHyperlinkStyle( "sign up" ) ) )
								.Concat( " to add comments on this article.".ToComponents() )
								.Materialize() ).ToCollection()
						.GetControls() );

			var usersById = UsersTableRetrieval.GetRows().ToIdDictionary();
			controls.AddRange(
				new StackList(
						CommentsTableRetrieval.GetRows( new CommentsTableEqualityConditions.ArticleId( info.ArticleId ) )
							.OrderByDescending( i => i.CommentId )
							.Select(
								i => {
									var rs = new UpdateRegionSet();
									var author = usersById[ i.AuthorId ];
									return new Paragraph( i.BodyText.ToComponents() ).ToCollection<FlowComponent>()
										.Append(
											new GenericFlowContainer(
												new GenericFlowContainer(
														new GenericPhrasingContainer(
																new EwfHyperlink(
																	Profile.GetInfo( i.AuthorId ),
																	new ImageHyperlinkStyle(
																		new ExternalResourceInfo(
																			author.ProfilePictureUrl.Any() ? author.ProfilePictureUrl : "https://static.productionready.io/images/smiley-cyrus.jpg" ),
																		"" ) ).ToCollection() ).ToCollection<PhrasingComponent>()
															.Append(
																new GenericPhrasingContainer(
																	new EwfHyperlink( Profile.GetInfo( i.AuthorId ), new StandardHyperlinkStyle( author.Username ) ).ToCollection() ) )
															.Append( new GenericPhrasingContainer( i.CreationDateAndTime.ToDayMonthYearString( false ).ToComponents() ) )
															.Materialize() ).ToCollection<FlowComponent>()
													.Concat(
														i.AuthorId == AppTools.User?.UserId
															? new EwfButton(
																new StandardButtonStyle( "Delete", icon: new ActionComponentIcon( new FontAwesomeIcon( "fa-trash" ) ) ),
																behavior: new PostBackBehavior(
																	postBack: PostBack.CreateIntermediate(
																		rs.ToCollection(),
																		id: PostBack.GetCompositeId( "delete", i.CommentId.ToString() ),
																		firstModificationMethod: () =>
																			CommentsModification.DeleteRows( new CommentsTableEqualityConditions.CommentId( i.CommentId ) ) ) ) ).ToCollection()
															: Enumerable.Empty<PhrasingComponent>() )
													.Materialize() ) )
										.Materialize()
										.ToComponentListItem( i.CommentId.ToString(), removalUpdateRegionSets: rs.ToCollection() );
								} ),
						setup: new ComponentListSetup(
							classes: ElementClasses.Comment,
							itemInsertionUpdateRegions: AppTools.User != null
								                            ? new ItemInsertionUpdateRegion( commentRs.ToCollection(), () => commentMod.CommentId.ToString().ToCollection() )
									                            .ToCollection()
								                            : null ) ).ToCollection()
					.GetControls() );

			return controls;
		}

		private CommentsModification getCommentMod() {
			var mod = CommentsModification.CreateForInsert();
			mod.AuthorId = AppTools.User.UserId;
			mod.ArticleId = info.ArticleId;
			mod.CreationDateAndTime = DateTime.UtcNow;
			return mod;
		}
	}
}