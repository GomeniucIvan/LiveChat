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

CREATE PROCEDURE [dbo].[CompanyGuestCustomer_CreateAndOrGetDetails]
     (
    @CompanyId INT,
	@UniqueId NVARCHAR(MAX),
	@Guid NVARCHAR(36))
AS
BEGIN
    DECLARE @companyGuestCustomerId INT;

    IF EXISTS (SELECT 1
               FROM   dbo.CompanyGuestCustomer cgc WITH(NOLOCK)
               WHERE  cgc.CompanyId = @CompanyId
                      AND cgc.Deleted = 0
                      AND ((ISNULL(@UniqueId, '') <> '' AND cgc.CustomerUniqueId = @UniqueId)
                           OR (ISNULL(@UniqueId, '') <> '' OR (ISNULL(@Guid, '') <> '' AND cgc.Guid = @Guid))))
    BEGIN
        SET @companyGuestCustomerId = (SELECT TOP 1 cgc.Id
                                       FROM   dbo.CompanyGuestCustomer cgc WITH(NOLOCK)
                                       WHERE  cgc.CompanyId = @CompanyId
                                              AND cgc.Deleted = 0
                                              AND ((ISNULL(@UniqueId, '') <> '' AND cgc.CustomerUniqueId = @UniqueId)
                                                   OR (ISNULL(@UniqueId, '') <> ''
                                                       OR (ISNULL(@Guid, '') <> '' AND cgc.Guid = @Guid))));
    END;
    ELSE
    BEGIN
        INSERT INTO dbo.CompanyGuestCustomer
        (   Guid,
            CustomerUniqueId,
            Deleted,
            CompanyId)
        VALUES
             (@Guid,          -- Guid - uniqueidentifier
              @UniqueId,      -- CompanyUniqueId - nvarchar(max)
              CAST(0 AS BIT), -- Deleted - bit
              @CompanyId      -- CompanyId - int
            );

        SET @companyGuestCustomerId = SCOPE_IDENTITY();
    END;

    SELECT cgc.Id, 
           cgc.Guid, 
           cgc.CustomerUniqueId, 
           cgc.Deleted, 
           cgc.CompanyId

    FROM   dbo.CompanyGuestCustomer cgc WITH(NOLOCK)
    WHERE  cgc.Id = @companyGuestCustomerId 
           AND cgc.CompanyId = @CompanyId;
END;
GO

CREATE PROCEDURE [dbo].[CompanyMessage_Insert]
     (
    @CompanyId INT,
	@CompanyCustomerId INT,
	@CompanyGuestCustomerId INT,
	@Message nvarchar(MAX))
AS
BEGIN

    INSERT INTO dbo.CompanyMessage
    (   Message,
        CompanyCustomerId,
        CompanyGuestCustomerId,
        CompanyId,
		CreatedOnUTc)
    VALUES
         (@Message,                 -- Message - nvarchar(max)
          @CompanyCustomerId,       -- CompanyCustomerId - int
          @CompanyGuestCustomerId,  -- CompanyGuestCustomerId - int
          @CompanyId,               -- CompanyId - int
          GETUTCDATE())             -- CreatedOnUTc - datetime

     SELECT SCOPE_IDENTITY();
END;
GO

CREATE PROCEDURE [dbo].[CompanyMessage_GetList]
    (
    @CompanyId INT,
	@CompanyGuestCustomerId INT,
	@CompanyCustomerId INT,
	@GuestCall BIT)
AS
BEGIN
    SELECT cm.Id,
           cm.Message,
           cm.CompanyCustomerId,
           cm.CompanyGuestCustomerId,
           cm.CompanyId,
           cm.CreatedOnUtc,
           CASE WHEN ISNULL(cm.CompanyCustomerId, '') = '' AND @GuestCall = 1 THEN
                    CAST(1 AS BIT)
           ELSE CAST(0 AS BIT)END AS Sent

    FROM   dbo.CompanyMessage cm WITH(NOLOCK)
    WHERE  (ISNULL(cm.CompanyGuestCustomerId, 0) = 0 OR cm.CompanyGuestCustomerId = @CompanyGuestCustomerId)
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
