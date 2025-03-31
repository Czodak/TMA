#!/bin/bash
set -e

echo "Initializing auth database..."

echo "Waiting for SQL Server to be ready..."
until /opt/mssql-tools/bin/sqlcmd -S sql_authapi -U sa -P "$SQL_AUTHAPI_SA_PASS" -Q "SELECT 1" > /dev/null 2>&1; do
    echo "SQL Server not ready yet. Retrying in 5 seconds..."
    sleep 5
done
echo "SQL Server is ready!"

# Create the database (if it doesn't exist)
echo "Creating database ${SQL_AUTHAPI_DB}..."
/opt/mssql-tools/bin/sqlcmd -S sql_authapi -U sa -P "$SQL_AUTHAPI_SA_PASS" -Q "
    IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '${SQL_AUTHAPI_DB}')
    BEGIN
        CREATE DATABASE ${SQL_AUTHAPI_DB};
        PRINT 'Database ${SQL_AUTHAPI_DB} created.';
    END
    ELSE
    BEGIN
        PRINT 'Database ${SQL_AUTHAPI_DB} already exists.';
    END
"

# Create the API user (if it doesn't exist)
echo "Creating API user ${SQL_AUTHAPI_USER}..."
/opt/mssql-tools/bin/sqlcmd -S sql_authapi -U sa -P "$SQL_AUTHAPI_SA_PASS" -Q "
    IF NOT EXISTS (SELECT * FROM sys.sql_logins WHERE name = '${SQL_AUTHAPI_USER}')
    BEGIN
        CREATE LOGIN ${SQL_AUTHAPI_USER} WITH PASSWORD = '${SQL_AUTHAPI_PASS}';
        PRINT 'Login ${SQL_AUTHAPI_USER} created.';
    END
    ELSE
    BEGIN
        PRINT 'Login ${SQL_AUTHAPI_USER} already exists.';
    END
    USE ${SQL_AUTHAPI_DB};
    IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = '${SQL_AUTHAPI_USER}')
    BEGIN
        CREATE USER ${SQL_AUTHAPI_USER} FOR LOGIN ${SQL_AUTHAPI_USER};
        ALTER ROLE db_datareader ADD MEMBER ${SQL_AUTHAPI_USER};
        ALTER ROLE db_datawriter ADD MEMBER ${SQL_AUTHAPI_USER};
        PRINT 'User ${SQL_AUTHAPI_USER} created in database ${SQL_AUTHAPI_DB}.';
    END
    ELSE
    BEGIN
        PRINT 'User ${SQL_AUTHAPI_USER} already exists in database ${SQL_AUTHAPI_DB}.';
    END
"

# Create the EF Core user (if it doesn't exist)
echo "Creating EF Core user ${SQL_AUTHAPI_EF_USER}..."
/opt/mssql-tools/bin/sqlcmd -S sql_authapi -U sa -P "$SQL_AUTHAPI_SA_PASS" -Q "
    IF NOT EXISTS (SELECT * FROM sys.sql_logins WHERE name = '${SQL_AUTHAPI_EF_USER}')
    BEGIN
        CREATE LOGIN ${SQL_AUTHAPI_EF_USER} WITH PASSWORD = '${SQL_AUTHAPI_EF_PASS}';
        PRINT 'Login ${SQL_AUTHAPI_EF_USER} created.';
    END
    ELSE
    BEGIN
        PRINT 'Login ${SQL_AUTHAPI_EF_USER} already exists.';
    END
    USE ${SQL_AUTHAPI_DB};
    IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = '${SQL_AUTHAPI_EF_USER}')
    BEGIN
        CREATE USER ${SQL_AUTHAPI_EF_USER} FOR LOGIN ${SQL_AUTHAPI_EF_USER};
        ALTER ROLE db_owner ADD MEMBER ${SQL_AUTHAPI_EF_USER};
        PRINT 'User ${SQL_AUTHAPI_EF_USER} created in database ${SQL_AUTHAPI_DB}.';
    END
    ELSE
    BEGIN
        PRINT 'User ${SQL_AUTHAPI_EF_USER} already exists in database ${SQL_AUTHAPI_DB}.';
    END
"

echo "Database initialization complete."