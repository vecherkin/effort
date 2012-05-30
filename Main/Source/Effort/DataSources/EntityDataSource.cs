﻿#region License

// Copyright (c) 2011 Effort Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

#endregion

using System;
using System.Data;
using System.Data.Common;
using System.Data.EntityClient;
using System.Data.Metadata.Edm;
using Effort.Internal.Common;
using Effort.Internal.TypeConversion;
using System.Data.Common.CommandTrees;

namespace Effort.DataProviders
{
    internal class EntityDataSource : DataSourceBase
    {
        private EntityConnection connection;
        private MetadataWorkspace workspace;
        private EntitySet entitySet;

        public EntityDataSource(Type entityType, ITypeConverter converter, EntityConnection connection, string tableName)
            : base(entityType, converter)
        {
            this.connection = connection;
            this.workspace = connection.GetMetadataWorkspace();
            this.entitySet = MetadataWorkspaceHelper.GetEntityContainer(this.workspace).GetEntitySetByName(tableName, true);
        }

        protected override IDataReader CreateDataReader()
        {
            // Build a command tree, which queries all records
            DbCommandTree commandTree = CommandTreeBuilder.CreateSelectAll(this.workspace, this.entitySet);
            // Get the provider services of the wrapped connection
            DbProviderServices providerServices = DbProviderServices.GetProviderServices(connection.StoreConnection);
            // Get current manifest token
            string manifestToken = providerServices.GetProviderManifestToken(connection.StoreConnection);
            // Get provider manifest
            DbProviderManifest providerManifest = providerServices.GetProviderManifest(manifestToken);

            // Create a command definition from the command tree
            DbCommandDefinition commandDefinition = providerServices.CreateCommandDefinition(providerManifest, commandTree);
            // Compile command 
            DbCommand command = commandDefinition.CreateCommand();

            // Setup command 
            command.Connection = connection.StoreConnection;
            // Execute
            DbDataReader reader = command.ExecuteReader();

            return reader;
        }

        protected override object ConvertValue(object value, Type type)
        {
            if (value is DBNull)
            {
                return null;
            }

            return base.ConvertValue(value, type);
        }
    }
}