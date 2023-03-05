CREATE PROCEDURE [dbo].[Company_GetDetails]
     (@CompanyId INT,
	  @CompanyKey NVARCHAR(400))
AS
BEGIN

    SELECT TOP 1 c.Id AS Id,
				 c.Name AS Name,
				 c.[Key] AS [Key]

    FROM   dbo.Company c WITH(NOLOCK)
    WHERE  c.Deleted = 0 
	AND ((ISNULL(@CompanyId,0) > 0 AND c.Id = @CompanyId) OR (ISNULL(@CompanyKey,'') != '' AND c.[Key] = @CompanyKey));

END;
GO

CREATE PROCEDURE [dbo].[Visitor_CreateAndOrGetDetails]
     (
    @CompanyId INT,
	@UniqueId NVARCHAR(MAX),
	@Guid NVARCHAR(36))
AS
BEGIN
    DECLARE @visitorId INT;

    IF EXISTS (SELECT 1
               FROM   dbo.Visitor v WITH(NOLOCK)
               WHERE  v.CompanyId = @CompanyId
                      AND v.Deleted = 0
                      AND ((ISNULL(@UniqueId, '') <> '' AND v.UniqueId = @UniqueId)
                           OR (ISNULL(@UniqueId, '') <> '' OR (ISNULL(@Guid, '') <> '' AND v.Guid = @Guid))))
    BEGIN
        SET @visitorId = (SELECT TOP 1 v.Id
                                       FROM   dbo.Visitor v WITH(NOLOCK)
                                       WHERE  v.CompanyId = @CompanyId
                                              AND v.Deleted = 0
                                              AND ((ISNULL(@UniqueId, '') <> '' AND v.UniqueId = @UniqueId)
                                                   OR (ISNULL(@UniqueId, '') <> ''
                                                       OR (ISNULL(@Guid, '') <> '' AND v.Guid = @Guid))));
    END;
    ELSE
    BEGIN
        INSERT INTO dbo.Visitor
        (   Guid,
            UniqueId,
            Deleted,
            CompanyId)
        VALUES
             (@Guid,          -- Guid - uniqueidentifier
              @UniqueId,      -- CompanyUniqueId - nvarchar(max)
              CAST(0 AS BIT), -- Deleted - bit
              @CompanyId      -- CompanyId - int
            );

        SET @visitorId = SCOPE_IDENTITY();
    END;

    SELECT v.Id, 
           v.Guid, 
           v.UniqueId, 
           v.Deleted, 
           v.CompanyId

    FROM   dbo.Visitor v WITH(NOLOCK)
    WHERE  v.Id = @visitorId 
           AND v.CompanyId = @CompanyId;
END;
GO

CREATE PROCEDURE [dbo].[CompanyMessage_Insert]
(
    @CompanyId INT,
    @CompanyCustomerId INT,
    @VisitorUniqueId INT,
    @Message NVARCHAR(MAX),
    @MessageTypeId INT
)
AS
BEGIN
    DECLARE @visitorId INT = (SELECT TOP 1 v.Id 
								FROM Visitor v WITH(NOLOCK) 
								WHERE v.UniqueId = @VisitorUniqueId
									AND v.Deleted = 0
									AND v.CompanyId = @CompanyId
								);

    IF ISNULL(@visitorId,0) = 0
    BEGIN
        INSERT INTO Visitor (
            Guid,
            UniqueId,
            Deleted,
            CompanyId
        ) VALUES (
            NEWID(),
            @VisitorUniqueId,
            0,
            @CompanyId
        );

        SET @visitorId = SCOPE_IDENTITY();
    END;

    INSERT INTO dbo.CompanyMessage (
        Message,
        CompanyCustomerId,
        VisitorId,
        CompanyId,
        CreatedOnUTc,
        MessageTypeId
    ) VALUES (
        @Message,
        @CompanyCustomerId,
        @visitorId,
        @CompanyId,
        GETUTCDATE(),
        @MessageTypeId
    );

    SELECT SCOPE_IDENTITY();
END;
GO

CREATE PROCEDURE [dbo].[CompanyMessage_GetList]
    (@CompanyId INT,
     @PageSize INT)
AS
BEGIN
	CREATE TABLE #TempVisitorIds (
        VisitorId INT,
		CompanyMessageId INT
    )

    INSERT INTO #TempVisitorIds (VisitorId, CompanyMessageId)
    SELECT DISTINCT TOP (@PageSize)
	cm.VisitorId,
	MAX(cm.Id)
    FROM dbo.CompanyMessage cm WITH (NOLOCK)         
    WHERE cm.CompanyId = @CompanyId
	GROUP BY cm.VisitorId;

    SELECT 
        cm.Id,
        cm.Message,
        cm.CompanyCustomerId,
		cm.VisitorId,
		cm.CompanyId,
		cm.CreatedOnUtc,
		cm.MessageTypeId as MessageType,
		c.FirstName + ' ' + c.LastName AS CustomerFullName,
		ga.Value AS VisitorFullName,
		v.UniqueId as VisitorUniqueId


    FROM #TempVisitorIds t
	JOIN dbo.CompanyMessage cm WITH(NOLOCK) ON cm.Id = t.CompanyMessageId
    JOIN dbo.Visitor v WITH (NOLOCK) ON v.Id = cm.VisitorId
    LEFT JOIN dbo.GenericAttribute ga WITH (NOLOCK) ON ga.EntityId = v.Id
                                                   AND ga.KeyGroup = 'Visitor'
                                                   AND ga.[Key] = 'FullName'
    LEFT JOIN dbo.CompanyCustomer cc WITH (NOLOCK) ON cc.Id = cm.CompanyCustomerId AND cc.Deleted = 0 AND  cc.CompanyId = @CompanyId
    LEFT JOIN dbo.Customer c WITH (NOLOCK) ON c.Id = cc.CustomerId AND c.Active = 1
	WHERE cm.VisitorId = cm.VisitorId

    DROP TABLE IF EXISTS #TempVisitorIds;
END;
GO

CREATE PROCEDURE [dbo].[CompanyMessage_GetVisitorList]
    (
    @CompanyId INT,
	@VisitorId INT,
	@CompanyCustomerId INT,
	@VisitorCall BIT,
    @NewMessagesCount INT = NULL OUT)
AS
BEGIN
    CREATE TABLE #messages (
        Id INT,
        Message VARCHAR(MAX),
        CompanyCustomerId INT,
        VisitorId INT,
        CompanyId INT,
        CreatedOnUtc DATETIME,
        MessageType INT,
	    ReadOnUtc DATETIME
    );

    INSERT INTO #messages (Id, Message, CompanyCustomerId, VisitorId, CompanyId, CreatedOnUtc, MessageType, ReadOnUtc)
    SELECT cm.Id,
           cm.Message,
           cm.CompanyCustomerId,
           cm.VisitorId,
           cm.CompanyId,
           cm.CreatedOnUtc,
           cm.MessageTypeId as MessageType,
	       cm.ReadOnUtc
    FROM   dbo.CompanyMessage cm WITH(NOLOCK)
    WHERE  (ISNULL(cm.VisitorId, 0) = 0 OR cm.VisitorId = @VisitorId)
           --AND (ISNULL(cm.CompanyCustomerId, 0) = 0 OR cm.CompanyCustomerId = @CompanyCustomerId)
           AND cm.CompanyId = @CompanyId;


    IF ISNULL(@VisitorCall,0) = 1
    BEGIN
        SET @NewMessagesCount = (SELECT COUNT(m.Id) FROM #messages m WHERE m.ReadOnUtc IS NULL AND m.MessageType != 0);
    END

    IF ISNULL(@VisitorCall,0) = 0
    BEGIN
        SET @NewMessagesCount = (SELECT COUNT(m.Id) FROM #messages m WHERE m.ReadOnUtc IS NULL AND m.MessageType = 0);
    END

    SELECT * FROM #messages;

    DROP TABLE IF EXISTS #messages;
END;
GO

CREATE PROCEDURE [dbo].[Customer_ApiDetails]
     (@Email NVARCHAR(500),
	  @UpdateLastLoginDate BIT = NULL)
AS
BEGIN

	IF ISNULL(@UpdateLastLoginDate,0) = 1
	BEGIN

		Update c
		SET c.LastLoginDateUtc = GETUTCDATE()
		FROM dbo.Customer c WITH(NOLOCK)
		WHERE c.Email = @Email 

	END

    SELECT TOP 1 c.Id AS Id,
				 c.Email,
				 c.Password,
				 c.Active,
				 c.CustomerGuid,
				 (SELECT STRING_AGG(cc.CompanyId,',') 
				 from dbo.CompanyCustomer cc WITH(NOLOCK)
				 WHERE cc.Deleted = 0 AND cc.CustomerId = c.Id) AS CompanyIdsString

    FROM   dbo.Customer c WITH(NOLOCK)
    WHERE  c.Deleted = 0 AND c.Email = @Email;

END;
GO

CREATE PROCEDURE [dbo].[Customer_WebContext]
     (@CustomerId INT)
AS
BEGIN

    SELECT TOP 1 c.Id AS Id,
				 c.Email,
				 c.Password,
				 c.Active,
				 c.CustomerGuid,
				 JSON_QUERY((SELECT cr.Id, cr.SystemName, cr.Name
                   FROM dbo.CustomerRole cr WITH(NOLOCK)
                   JOIN dbo.CustomerRoleMapping crm WITH(NOLOCK) ON crm.CustomerRoleId = cr.ID
                   WHERE crm.CustomerId = c.Id AND cr.Active = 1
                   FOR JSON PATH)) AS RolesJson,
				   ga.Value as LanguageId

    FROM   dbo.Customer c WITH(NOLOCK)
	LEFT JOIN dbo.GenericAttribute ga WITH(NOLOCK) ON ga.EntityId = c.Id AND ga.KeyGroup = 'Customer' AND ga.[Key] = 'LanguageId'
    WHERE  c.Deleted = 0 AND c.Id = @CustomerId;

END;
GO
