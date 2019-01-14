﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Neo4j.Driver.V1;
using ServiceStack;

namespace Neo4jMapper
{
    public static class SessionExtensions
    {
        private const string GetNodeStatement = @"
                MATCH (node)
                WHERE id(node) = $p1
                RETURN node";
        
        private const string SetNodeStatement = @"
                MATCH (node)
                WHERE id(node) = $p1
                SET node = $p2";

        private static readonly string NodeIdUnspecifiedMessage = $"{nameof(NodeIdAttribute)} not specified or the Node Id is null";

        public static TEntity GetNode<TEntity>(
            this ISession session,
            long nodeId) where TEntity : class
        {
            var parameters = new {
                p1 = nodeId
            };

            return session
                .Run(GetNodeStatement, parameters)
                .Map<TEntity>()
                .SingleOrDefault();
        }

        public static async Task<TEntity> GetNodeAsync<TEntity>(
            this ISession session,
            long nodeId) where TEntity : class
        {
            var parameters = new {
                p1 = nodeId
            };

            var statementResultCursor = await session.RunAsync(GetNodeStatement, parameters);

            return (await statementResultCursor.SingleAsync()).Map<TEntity>();
        }

        public static IStatementResult SetNode<TEntity>(
            this ISession session,
            TEntity entity) where TEntity : class
        {
            var nodeId = EntityAccessor.GetNodeId(entity);
            if (nodeId == null)
                throw new InvalidOperationException(NodeIdUnspecifiedMessage);

            var parameters = new {
                p1 = nodeId,
                p2 = entity.ToObjectDictionary()
            };

            return session.Run(SetNodeStatement, parameters);
        }

        public static async Task<IStatementResultCursor> SetNodeAsync<TEntity>(
            this ISession session,
            TEntity entity) where TEntity : class
        {
            var nodeId = EntityAccessor.GetNodeId(entity);
            if (nodeId == null)
                throw new InvalidOperationException(NodeIdUnspecifiedMessage);

            var parameters = new {
                p1 = nodeId,
                p2 = entity.ToObjectDictionary()
            };

            return await session.RunAsync(SetNodeStatement, parameters);
        }
    }
}