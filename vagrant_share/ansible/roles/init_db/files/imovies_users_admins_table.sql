DROP TABLE IF EXISTS admins;

CREATE TABLE admins (
    uid VARCHAR(64) NOT NULL DEFAULT '',
    PRIMARY KEY(uid)
);

INSERT INTO admins VALUES ('ms');
