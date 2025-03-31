#!/bin/bash
set -e

# Wait for SQL Server to start
echo "Waiting for SQL Server to be ready..."
until /opt/mssql-tools/bin/sqlcmd -S sql_taskapi -U sa -P "$SQL_TASKAPI_SA_PASS" -Q "SELECT 1" > /dev/null 2>&1; do
    echo "SQL Server not ready yet. Retrying in 5 seconds..."
    sleep 5
done
echo "SQL Server is ready!"

# Create the database (if it doesn't exist)
/opt/mssql-tools/bin/sqlcmd -S sql_taskapi -U sa -P "$SQL_TASKAPI_SA_PASS" -Q "
    IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '${SQL_TASKAPI_DB}')
    BEGIN
        CREATE DATABASE ${SQL_TASKAPI_DB};
        PRINT 'Database ${SQL_TASKAPI_DB} created.';
    END
    ELSE
    BEGIN
        PRINT 'Database ${SQL_TASKAPI_DB} already exists.';
    END
"

# Create the API user (if it doesn't exist)
/opt/mssql-tools/bin/sqlcmd -S sql_taskapi -U sa -P "$SQL_TASKAPI_SA_PASS" -Q "
    IF NOT EXISTS (SELECT * FROM sys.sql_logins WHERE name = '${SQL_TASKAPI_USER}')
    BEGIN
        CREATE LOGIN ${SQL_TASKAPI_USER} WITH PASSWORD = '${SQL_TASKAPI_PASS}';
        PRINT 'Login ${SQL_TASKAPI_USER} created.';
    END
    ELSE
    BEGIN
        PRINT 'Login ${SQL_TASKAPI_USER} already exists.';
    END
    USE ${SQL_TASKAPI_DB};
    IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = '${SQL_TASKAPI_USER}')
    BEGIN
        CREATE USER ${SQL_TASKAPI_USER} FOR LOGIN ${SQL_TASKAPI_USER};
        ALTER ROLE db_datareader ADD MEMBER ${SQL_TASKAPI_USER};
        ALTER ROLE db_datawriter ADD MEMBER ${SQL_TASKAPI_USER};
        PRINT 'User ${SQL_TASKAPI_USER} created in database ${SQL_TASKAPI_DB}.';
    END
    ELSE
    BEGIN
        PRINT 'User ${SQL_TASKAPI_USER} already exists in database ${SQL_TASKAPI_DB}.';
    END
"

# Create the Flyway user (if it doesn't exist)
/opt/mssql-tools/bin/sqlcmd -S sql_taskapi -U sa -P "$SQL_TASKAPI_SA_PASS" -Q "
    IF NOT EXISTS (SELECT * FROM sys.sql_logins WHERE name = '${SQL_TASKAPI_FLYWAY_USER}')
    BEGIN
        CREATE LOGIN ${SQL_TASKAPI_FLYWAY_USER} WITH PASSWORD = '${SQL_TASKAPI_FLYWAY_PASS}';
        PRINT 'Login ${SQL_TASKAPI_FLYWAY_USER} created.';
    END
    ELSE
    BEGIN
        PRINT 'Login ${SQL_TASKAPI_FLYWAY_USER} already exists.';
    END
    USE ${SQL_TASKAPI_DB};
    IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = '${SQL_TASKAPI_FLYWAY_USER}')
    BEGIN
        CREATE USER ${SQL_TASKAPI_FLYWAY_USER} FOR LOGIN ${SQL_TASKAPI_FLYWAY_USER};
        ALTER ROLE db_owner ADD MEMBER ${SQL_TASKAPI_FLYWAY_USER};
        PRINT 'User ${SQL_TASKAPI_FLYWAY_USER} created in database ${SQL_TASKAPI_DB}.';
    END
    ELSE
    BEGIN
        PRINT 'User ${SQL_TASKAPI_FLYWAY_USER} already exists in database ${SQL_TASKAPI_DB}.';
    END
"

echo "Database initialization complete."