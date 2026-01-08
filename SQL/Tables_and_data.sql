-- Tables

CREATE TABLE Users (
    Id int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    UserName varchar(50) NOT NULL UNIQUE,
    Email varchar(200) NOT NULL UNIQUE,
    Password varchar(100) NOT NULL,
    Created timestamp NOT NULL DEFAULT NOW(),
    Modified timestamp NULL
);

CREATE TABLE UsersExp (
    Id int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    UserName varchar(50) NOT NULL UNIQUE,
    Email varchar(200) NOT NULL UNIQUE,
    Password varchar(100) NOT NULL,
    Original int NOT NULL,
    Expired timestamp NOT NULL DEFAULT NOW(),
    Deleted boolean NULL
);

CREATE TABLE Device (
    Id int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    Name varchar(50) NOT NULL,
    IPAddress varchar(20) NULL,
    Connected boolean NOT NULL,
    MeasurementCount int NULL,
    Created timestamp NOT NULL DEFAULT NOW(),
    Modified timestamp NULL
);

CREATE TABLE DeviceExp (
    Id int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    Name varchar(50) NOT NULL,
    IPAddress varchar(20) NULL,
    Connected boolean NOT NULL,
    MeasurementCount int NULL,
    Original int NOT NULL,
    Expired timestamp NOT NULL DEFAULT NOW(),
    Deleted boolean NULL
);

CREATE TABLE DeviceParam (
    Id int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    Name varchar(50) NOT NULL,
    DeviceId int NOT NULL,
    Value int NOT NULL,
    Modifier varchar(50) NULL,
    Created timestamp NOT NULL DEFAULT NOW(),
    Modified timestamp NULL
);

ALTER TABLE DeviceParam
ADD CONSTRAINT fk_deviceparam_device
    FOREIGN KEY (DeviceId)
    REFERENCES Device(Id)
    ON DELETE CASCADE;

CREATE TABLE DeviceParamExp (
    Id int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    Name varchar(50) NOT NULL,
    DeviceId int NOT NULL,
    Value int NOT NULL,
    Modifier varchar(50) NULL,
    Original int NOT NULL,
    Expired timestamp NOT NULL DEFAULT NOW(),
    Deleted boolean NULL
);

CREATE TABLE SystemUsage (
    Id int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    DeviceId int NOT NULL,
    MeasurementName varchar(50) NOT NULL,
    Usage real NOT NULL,
    Timestamp timestamp NULL,
    Created timestamp NOT NULL DEFAULT NOW()
);

ALTER TABLE SystemUsage
ADD CONSTRAINT fk_systemusage_device
    FOREIGN KEY (DeviceId)
    REFERENCES Device(Id)
    ON DELETE CASCADE;

CREATE TABLE SystemUsageExp (
    Id int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    DeviceId int NOT NULL,
    MeasurementName varchar(50) NOT NULL,
    Usage real NOT NULL,
    Timestamp timestamp NULL,
    Original int NOT NULL,
    Expired timestamp NOT NULL DEFAULT NOW(),
    Deleted boolean NULL
);

CREATE TABLE Logs (
    Id int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    Source varchar(50) NOT NULL,
    LogType varchar(20) NOT NULL,
    Message text NOT NULL,
    Timestamp timestamp NULL,
    Created timestamp NOT NULL DEFAULT NOW()
);

-- Triggers

-- Users
CREATE OR REPLACE FUNCTION users_update_trigger()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
    IF OLD IS DISTINCT FROM NEW THEN
        INSERT INTO UsersExp (
            UserName,
            Email,
            Password,
            Original,
            Expired,
            Deleted
        )
        VALUES (
            OLD.UserName,
            OLD.Email,
            OLD.Password,
            OLD.Id,
            NOW(),
            FALSE
        );
    
        NEW.Modified := NOW();
    END IF;
    RETURN NEW;
END;
$$;

CREATE TRIGGER users_before_update
BEFORE UPDATE ON Users
FOR EACH ROW
EXECUTE FUNCTION users_update_trigger();

CREATE OR REPLACE FUNCTION users_delete_trigger()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
    INSERT INTO UsersExp (
        UserName,
        Email,
        Password,
        Original,
        Expired,
        Deleted
    )
    VALUES (
        OLD.UserName,
        OLD.Email,
        OLD.Password,
        OLD.Id,
        NOW(),
        TRUE
    );

    RETURN OLD;
END;
$$;

CREATE TRIGGER users_before_delete
BEFORE DELETE ON Users
FOR EACH ROW
EXECUTE FUNCTION users_delete_trigger();

-- Device
CREATE OR REPLACE FUNCTION device_update_trigger()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
    IF OLD IS DISTINCT FROM NEW THEN
        INSERT INTO DeviceExp (
            Name,
            IPAddress,
            Connected,
            MeasurementCount,
            Original,
            Expired,
            Deleted
        )
        VALUES (
            OLD.Name,
            OLD.IPAddress,
            OLD.Connected,
            OLD.MeasurementCount,
            OLD.Id,
            NOW(),
            FALSE
        );
    
        NEW.Modified := NOW();
    END IF;
    RETURN NEW;
END;
$$;

CREATE TRIGGER device_before_update
BEFORE UPDATE ON Device
FOR EACH ROW
EXECUTE FUNCTION device_update_trigger();

CREATE OR REPLACE FUNCTION device_delete_trigger()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
    INSERT INTO DeviceExp (
        Name,
        IPAddress,
        Connected,
        MeasurementCount,
        Original,
        Expired,
        Deleted
    )
    VALUES (
        OLD.Name,
        OLD.IPAddress,
        OLD.Connected,
        OLD.MeasurementCount,
        OLD.Id,
        NOW(),
        TRUE
    );

    RETURN OLD;
END;
$$;

CREATE TRIGGER device_before_delete
BEFORE DELETE ON Device
FOR EACH ROW
EXECUTE FUNCTION device_delete_trigger();

-- DeviceParam
CREATE OR REPLACE FUNCTION deviceparam_update_trigger()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
    IF OLD IS DISTINCT FROM NEW THEN
        INSERT INTO DeviceParamExp (
            Name,
            DeviceId,
            Value,
            Modifier,
            Original,
            Expired,
            Deleted
        )
        VALUES (
            OLD.Name,
            OLD.DeviceId,
            OLD.Value,
            OLD.Modifier,
            OLD.Id,
            NOW(),
            FALSE
        );
    
        NEW.Modified := NOW();
    END IF;
    RETURN NEW;
END;
$$;

CREATE TRIGGER deviceparam_before_update
BEFORE UPDATE ON DeviceParam
FOR EACH ROW
EXECUTE FUNCTION deviceparam_update_trigger();

CREATE OR REPLACE FUNCTION deviceparam_delete_trigger()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
    INSERT INTO DeviceParamExp (
        Name,
        DeviceId,
        Value,
        Modifier,
        Original,
        Expired,
        Deleted
    )
    VALUES (
        OLD.Name,
        OLD.DeviceId,
        OLD.Value,
        OLD.Modifier,
        OLD.Id,
        NOW(),
        TRUE
    );

    RETURN OLD;
END;
$$;

CREATE TRIGGER deviceparam_before_delete
BEFORE DELETE ON DeviceParam
FOR EACH ROW
EXECUTE FUNCTION deviceparam_delete_trigger();

-- SystemUsage
CREATE OR REPLACE FUNCTION systemusage_delete_trigger()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
    INSERT INTO SystemUsageExp (
        DeviceId,
        MeasurementName,
        Usage,
        Timestamp,
        Original,
        Expired,
        Deleted
    )
    VALUES (
        OLD.DeviceId,
        OLD.MeasurementName,
        OLD.Usage,
        OLD.Timestamp,
        OLD.Id,
        NOW(),
        TRUE
    );

    RETURN OLD;
END;
$$;

CREATE TRIGGER systemusage_before_delete
BEFORE DELETE ON SystemUsage
FOR EACH ROW
EXECUTE FUNCTION systemusage_delete_trigger();