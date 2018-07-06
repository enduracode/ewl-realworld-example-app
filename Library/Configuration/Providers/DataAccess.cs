using System;
using System.Collections.Generic;
using System.Linq;
using EnterpriseWebLibrary.DataAccess;
using EnterpriseWebLibrary.DataAccess.RevisionHistory;
using EwlRealWorld.Library.DataAccess;
using EwlRealWorld.Library.DataAccess.CommandConditions;
using EwlRealWorld.Library.DataAccess.Modification;
using EwlRealWorld.Library.DataAccess.TableConstants;
using EwlRealWorld.Library.DataAccess.TableRetrieval;

namespace EwlRealWorld.Library.Configuration.Providers {
	internal class DataAccess: RevisionHistoryProvider {
		int SystemDataAccessProvider.GetNextMainSequenceValue() {
			return MainSequence.GetNextValue();
		}

		IEnumerable<UserTransaction> RevisionHistoryProvider.GetAllUserTransactions() {
			return from i in UserTransactionsTableRetrieval.GetRows() select new UserTransaction( i.UserTransactionId, i.TransactionDateAndTime, i.UserId );
		}

		void RevisionHistoryProvider.InsertUserTransaction( int userTransactionId, DateTime transactionDateAndTime, int? userId ) {
			UserTransactionsModification.InsertRow( userTransactionId, transactionDateAndTime, userId );
		}

		IEnumerable<Revision> RevisionHistoryProvider.GetAllRevisions() {
			return from i in RevisionsTableRetrieval.GetRows() select new Revision( i.RevisionId, i.LatestRevisionId, i.UserTransactionId );
		}

		Revision RevisionHistoryProvider.GetRevision( int revisionId ) {
			var revision = RevisionsTableRetrieval.GetRowMatchingId( revisionId );
			return new Revision( revision.RevisionId, revision.LatestRevisionId, revision.UserTransactionId );
		}

		void RevisionHistoryProvider.InsertRevision( int revisionId, int latestRevisionId, int userTransactionId ) {
			RevisionsModification.InsertRow( revisionId, latestRevisionId, userTransactionId );
		}

		void RevisionHistoryProvider.UpdateRevision( int revisionId, int latestRevisionId, int userTransactionId, int revisionIdForWhereClause ) {
			RevisionsModification.UpdateRows(
				revisionId,
				latestRevisionId,
				userTransactionId,
				new RevisionsTableEqualityConditions.RevisionId( revisionIdForWhereClause ) );
		}

		string RevisionHistoryProvider.GetLatestRevisionsQuery() {
			return
				$"SELECT {RevisionsTable.RevisionIdColumn.Name} FROM {RevisionsTable.Name} WHERE {RevisionsTable.RevisionIdColumn.Name} = {RevisionsTable.LatestRevisionIdColumn.Name}";
		}
	}
}