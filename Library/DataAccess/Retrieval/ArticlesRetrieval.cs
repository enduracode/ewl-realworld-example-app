using System.Linq;

namespace EwlRealWorld.Library.DataAccess.Retrieval {
	partial class ArticlesRetrieval {
		partial class Row {
			public Modification.ArticlesModification ToModification() =>
				Modification.ArticlesModification.CreateForSingleRowUpdate( ArticleId, AuthorId, Slug, Title, Description, BodyMarkdown, CreationDateAndTime );
		}

		public static Row GetRowMatchingId( int articleId ) => GetRowsMatchingId( articleId ).Single();
	}
}