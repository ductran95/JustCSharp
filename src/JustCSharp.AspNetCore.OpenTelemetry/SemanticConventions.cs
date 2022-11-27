namespace JustCSharp.AspNetCore.OpenTelemetry;

public static class SemanticConventions
{
    // The set of constants matches the specification as of this commit.
    // https://github.com/open-telemetry/opentelemetry-specification/tree/main/specification/trace/semantic_conventions
    // https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/semantic_conventions/exceptions.md
    public const string AttributeNetTransport = "net.transport";
    public const string AttributeNetPeerIp = "net.peer.ip";
    public const string AttributeNetPeerPort = "net.peer.port";
    public const string AttributeNetPeerName = "net.peer.name";
    public const string AttributeNetHostIp = "net.host.ip";
    public const string AttributeNetHostPort = "net.host.port";
    public const string AttributeNetHostName = "net.host.name";

    public const string AttributeEnduserId = "enduser.id";
    public const string AttributeEnduserRole = "enduser.role";
    public const string AttributeEnduserScope = "enduser.scope";

    public const string AttributePeerService = "peer.service";

    public const string AttributeHttpMethod = "http.method";
    public const string AttributeHttpUrl = "http.url";
    public const string AttributeHttpTarget = "http.target";
    public const string AttributeHttpHost = "http.host";
    public const string AttributeHttpScheme = "http.scheme";
    public const string AttributeHttpStatusCode = "http.status_code";
    public const string AttributeHttpStatusText = "http.status_text";
    public const string AttributeHttpFlavor = "http.flavor";
    public const string AttributeHttpServerName = "http.server_name";
    public const string AttributeHttpRoute = "http.route";
    public const string AttributeHttpClientIP = "http.client_ip";
    public const string AttributeHttpUserAgent = "http.user_agent";
    public const string AttributeHttpRequestContentLength = "http.request_content_length";
    public const string AttributeHttpRequestContentType = "http.request_content_type";
    public const string AttributeHttpRequestBody = "http.request_body";
    public const string AttributeHttpRequestContentLengthUncompressed = "http.request_content_length_uncompressed";
    public const string AttributeHttpRequestHeader = "http.request.header.{0}";
    public const string AttributeHttpResponse = "http.response";
    public const string AttributeHttpResponseContent = "http.response_content";
    public const string AttributeHttpResponseBody = "http.response_body";
    public const string AttributeHttpResponseContentLength = "http.response_content_length";
    public const string AttributeHttpResponseContentType = "http.response_content_type";
    public const string AttributeHttpResponseHeader = "http.response.header.{0}";
    public const string AttributeHttpResponseContentLengthUncompressed = "http.response_content_length_uncompressed";

    public const string AttributeDbSystem = "db.system";
    public const string AttributeDbConnectionString = "db.connection_string";
    public const string AttributeDbUser = "db.user";
    public const string AttributeDbMsSqlInstanceName = "db.mssql.instance_name";
    public const string AttributeDbJdbcDriverClassName = "db.jdbc.driver_classname";
    public const string AttributeDbName = "db.name";
    public const string AttributeDbStatement = "db.statement";
    public const string AttributeDbOperation = "db.operation";
    public const string AttributeDbInstance = "db.instance";
    public const string AttributeDbUrl = "db.url";
    public const string AttributeDbCassandraKeyspace = "db.cassandra.keyspace";
    public const string AttributeDbHBaseNamespace = "db.hbase.namespace";
    public const string AttributeDbRedisDatabaseIndex = "db.redis.database_index";
    public const string AttributeDbMongoDbCollection = "db.mongodb.collection";

    public const string AttributeRpcSystem = "rpc.system";
    public const string AttributeRpcService = "rpc.service";
    public const string AttributeRpcMethod = "rpc.method";
    public const string AttributeRpcGrpcStatusCode = "rpc.grpc.status_code";

    public const string AttributeMessageTime = "message.time";
    public const string AttributeMessageType = "message.type";
    public const string AttributeMessageId = "message.id";
    public const string AttributeMessageCompressedSize = "message.compressed_size";
    public const string AttributeMessageUncompressedSize = "message.uncompressed_size";

    public const string AttributeFaasTrigger = "faas.trigger";
    public const string AttributeFaasExecution = "faas.execution";
    public const string AttributeFaasDocumentCollection = "faas.document.collection";
    public const string AttributeFaasDocumentOperation = "faas.document.operation";
    public const string AttributeFaasDocumentTime = "faas.document.time";
    public const string AttributeFaasDocumentName = "faas.document.name";
    public const string AttributeFaasTime = "faas.time";
    public const string AttributeFaasCron = "faas.cron";

    public const string AttributeMessagingSystem = "messaging.system";
    public const string AttributeMessagingDestination = "messaging.destination";
    public const string AttributeMessagingDestinationKind = "messaging.destination_kind";
    public const string AttributeMessagingTempDestination = "messaging.temp_destination";
    public const string AttributeMessagingProtocol = "messaging.protocol";
    public const string AttributeMessagingProtocolVersion = "messaging.protocol_version";
    public const string AttributeMessagingUrl = "messaging.url";
    public const string AttributeMessagingMessageId = "messaging.message_id";
    public const string AttributeMessagingConversationId = "messaging.conversation_id";
    public const string AttributeMessagingPayloadSize = "messaging.message_payload_size_bytes";
    public const string AttributeMessagingPayloadCompressedSize = "messaging.message_payload_compressed_size_bytes";
    public const string AttributeMessagingOperation = "messaging.operation";

    public const string AttributeExceptionEventName = "exception";
    public const string AttributeExceptionType = "exception.type";
    public const string AttributeExceptionMessage = "exception.message";
    public const string AttributeExceptionStacktrace = "exception.stacktrace";
    public const string AttributeExceptionErrors = "exception.errors";

    /// <summary>
    ///     Username or client_id extracted from the access token or Authorization header in the inbound request from outside
    ///     the system.
    /// </summary>
    /// <example>E.g. username.</example>
    public const string AttributeEndUserId = "enduser.id";

    /// <summary>
    ///     Actual/assumed role the client is making the request under extracted from token or application security context.
    /// </summary>
    /// <example>E.g. admin.</example>
    public const string AttributeEndUserRole = "enduser.role";

    /// <summary>
    ///     Scopes or granted authorities the client currently possesses extracted from token or application security context.
    ///     The value would come from the scope associated with an OAuth 2.0 Access Token or an attribute value in a SAML 2.0
    ///     Assertion.
    /// </summary>
    /// <example>E.g. read:message,write:files.</example>
    public const string AttributeEndUserScope = "enduser.scope";

    public const string AttributeApiVersion = "api.version";
}