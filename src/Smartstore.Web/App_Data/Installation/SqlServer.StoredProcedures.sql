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
	@VisitorId INT,
	@Message nvarchar(MAX),
    @MessageTypeId INT)
AS
BEGIN

    INSERT INTO dbo.CompanyMessage
    (   Message,
        CompanyCustomerId,
        VisitorId,
        CompanyId,
		CreatedOnUTc,
        MessageTypeId)
    VALUES
         (@Message,                 -- Message - nvarchar(max)
          @CompanyCustomerId,       -- CompanyCustomerId - int
          @VisitorId,               -- VisitorId - int
          @CompanyId,               -- CompanyId - int
          GETUTCDATE(),             -- CreatedOnUTc - datetime
          @MessageTypeId)           -- MessageTypeId - int

     SELECT SCOPE_IDENTITY();
END;
GO

CREATE PROCEDURE [dbo].[CompanyMessage_GetVisitorList]
    (
    @CompanyId INT,
	@VisitorId INT,
	@CompanyCustomerId INT,
	@VisitorCall BIT)
AS
BEGIN
    SELECT cm.Id,
           cm.Message,
           cm.CompanyCustomerId,
           cm.VisitorId,
           cm.CompanyId,
           cm.CreatedOnUtc,
           CASE WHEN ISNULL(cm.CompanyCustomerId, '') = '' AND @VisitorCall = 1 THEN
                    CAST(1 AS BIT)
           ELSE CAST(0 AS BIT)END AS Sent

    FROM   dbo.CompanyMessage cm WITH(NOLOCK)
    WHERE  (ISNULL(cm.VisitorId, 0) = 0 OR cm.VisitorId = @VisitorId)
           --AND (ISNULL(cm.CompanyCustomerId, 0) = 0 OR cm.CompanyCustomerId = @CompanyCustomerId)
           AND cm.CompanyId = @CompanyId;
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
