CREATE TABLE public_certificates (
    serial_number BIGINT UNSIGNED NOT NULL,
    uid VARCHAR(64) NOT NULL DEFAULT '',
    public_cert TEXT NOT NULL,
    revoked BOOLEAN DEFAULT FALSE,
    PRIMARY KEY(serial_number)
);


CREATE TABLE private_certificates (
    sk_id INT NOT NULL AUTO_INCREMENT UNIQUE,
    uid varchar(64) NOT NULL DEFAULT '' UNIQUE,
    private_cert TEXT,
    PRIMARY KEY(uid)
);
