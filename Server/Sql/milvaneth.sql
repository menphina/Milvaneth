-- CREATE DATABASE milvaneth;
-- CREATE USER milvaneth WITH ENCRYPTED PASSWORD 'password';
-- GRANT ALL PRIVILEGES ON DATABASE milvaneth TO milvaneth;
-- Note: will grant milvaneth all permissions at the end of this sql file.

CREATE EXTENSION IF NOT EXISTS timescaledb CASCADE;


----------------------------------------------------------------------------------------------
-- Account Store
CREATE TABLE privilege_config (
    privilege_level    SERIAL           NOT NULL        PRIMARY KEY,
    name               VARCHAR(40)      NOT NULL,
    access_data        BOOLEAN          NOT NULL,
    login              BOOLEAN          NOT NULL,
    ignore_karma       BOOLEAN          NOT NULL,
    access_statics     BOOLEAN          NOT NULL,
    debug              BOOLEAN          NOT NULL,
    batch_read         BOOLEAN          NOT NULL,
    batch_write        BOOLEAN          NOT NULL,
    account_operation  BOOLEAN          NOT NULL,
    release_update     BOOLEAN          NOT NULL,
    delete_record      BOOLEAN          NOT NULL,
    account_management BOOLEAN          NOT NULL
);

CREATE TABLE account_data (
    account_id         BIGSERIAL         NOT NULL        PRIMARY KEY,
    account_name       VARCHAR(20)       NOT NULL        UNIQUE,
    display_name       VARCHAR(20),
    email              VARCHAR(40),
    email_confirmed    BOOLEAN           NOT NULL,
    salt               BYTEA             NOT NULL,
    verifier           BYTEA             NOT NULL,
    group_param        SMALLINT          NOT NULL,
    register_service   INTEGER           NOT NULL,
    related_service    INTEGER[],
    played_character   BIGINT[],
    trace              BIGINT[]          NOT NULL,
    karma              BIGINT            NOT NULL,
    -- Checked when issuing token or access API requiring identity check.
    privilege_level    INTEGER           NOT NULL        REFERENCES privilege_config(privilege_level),
    suspend_until      TIMESTAMP,
    password_retry     SMALLINT,
    last_retry         TIMESTAMP
);

INSERT INTO privilege_config (name, access_data, login, ignore_karma, access_statics, debug, batch_read, batch_write, account_operation, release_update, delete_record, account_management) VALUES
    ('User',                        TRUE,        TRUE,   FALSE,       FALSE,          FALSE, FALSE,      FALSE,       TRUE,              FALSE,          FALSE,         FALSE),
    ('Blocked',                     FALSE,       FALSE,  FALSE,       FALSE,          FALSE, FALSE,      FALSE,       FALSE,             FALSE,          FALSE,         FALSE),
    ('Unverified',                  FALSE,       TRUE,   FALSE,       FALSE,          FALSE, FALSE,      FALSE,       TRUE,              FALSE,          FALSE,         FALSE),
    ('Bot',                         FALSE,       FALSE,  TRUE,        TRUE,           FALSE, FALSE,      TRUE,        FALSE,             FALSE,          FALSE,         FALSE),
    ('Account Operator',            FALSE,       TRUE,   TRUE,        TRUE,           FALSE, FALSE,      FALSE,       TRUE,              FALSE,          FALSE,         TRUE),
    ('Data Operator',               TRUE,        TRUE,   TRUE,        FALSE,          FALSE, TRUE,       TRUE,        TRUE,              FALSE,          TRUE,          FALSE),
    ('Admin',                       TRUE,        TRUE,   TRUE,        TRUE,           TRUE,  TRUE,       TRUE,        TRUE,              TRUE,           TRUE,          TRUE),
    ('Third Party',                 TRUE,        FALSE,  TRUE,        TRUE,           FALSE, TRUE,       FALSE,       FALSE,             FALSE,          FALSE,         FALSE),
    ('Debugger',                    FALSE,       FALSE,  TRUE,        TRUE,           TRUE,  FALSE,      FALSE,       FALSE,             FALSE,          FALSE,         FALSE),
    ('Status',                      FALSE,       FALSE,  TRUE,        TRUE,           FALSE, FALSE,      FALSE,       FALSE,             FALSE,          FALSE,         FALSE);

INSERT INTO account_data (account_id, account_name, display_name, email, email_confirmed, salt, verifier, group_param, register_service, related_service, played_character, trace, karma, privilege_level, suspend_until, password_retry, last_retry) VALUES
    (0, '', '', '', FALSE, '0x00', '0x00', 2048, 0, NULL, NULL, '{0}', 0, 2, NULL, 0, NULL);



----------------------------------------------------------------------------------------------
-- Token and API key
CREATE TABLE token_issue_list (
    -- TBH, this is a audit log
    token_serial     BIGSERIAL         NOT NULL        PRIMARY KEY,
    holding_account  BIGINT            NOT NULL        REFERENCES account_data(account_id),
    reason           INTEGER           NOT NULL,
    issue_time       TIMESTAMP         NOT NULL,
    valid_until      TIMESTAMP         NOT NULL
);
CREATE INDEX token_account ON token_issue_list(holding_account);

CREATE TABLE token_revocation_list (
    token_serial     BIGINT            NOT NULL        PRIMARY KEY        REFERENCES token_issue_list(token_serial),
    reason           INTEGER           NOT NULL,
    revoke_since     TIMESTAMP         NOT NULL
);

CREATE TABLE email_verify_code (
    event_id         BIGSERIAL         NOT NULL        PRIMARY KEY,
    account_id       BIGINT            NOT NULL        REFERENCES account_data(account_id),
    email            VARCHAR(40)       NOT NULL,
    failed_retry     SMALLINT          NOT NULL,
    send_time        TIMESTAMP         NOT NULL,
    valid_to         TIMESTAMP         NOT NULL,
    code             VARCHAR(40)       NOT NULL
);

CREATE TABLE key_usage (
    usage            SERIAL            NOT NULL        PRIMARY KEY,
    name             VARCHAR(40)       NOT NULL,
    prove_identity   BOOLEAN           NOT NULL,
    create_session   BOOLEAN           NOT NULL,
    renew_session    BOOLEAN           NOT NULL,
    get_change_token BOOLEAN           NOT NULL,
    change_password  BOOLEAN           NOT NULL,
    access_data      BOOLEAN           NOT NULL,
    batch_read       BOOLEAN           NOT NULL,
    batch_write      BOOLEAN           NOT NULL
);

CREATE TABLE key_store (
    key_id           BIGSERIAL         NOT NULL        PRIMARY KEY,
    key              BYTEA             NOT NULL        UNIQUE,
    holding_account  BIGINT            NOT NULL        REFERENCES account_data(account_id),
    valid_from       TIMESTAMP         NOT NULL,
    valid_until      TIMESTAMP         NOT NULL,
    last_active      TIMESTAMP         NOT NULL,
    usage            INTEGER           NOT NULL        REFERENCES key_usage(usage),
    reuse_counter    INTEGER           NOT NULL,
    quota            INTEGER           NOT NULL
);
CREATE INDEX key_key ON key_store(key);
CREATE INDEX key_account ON key_store(holding_account);

INSERT INTO key_usage (name, prove_identity, create_session, renew_session, get_change_token, change_password, access_data, batch_read, batch_write) VALUES
    -- Short Lived Token (a.k.a. Normal user token)
    ('Auth Token', FALSE, TRUE, FALSE, TRUE, FALSE, FALSE, FALSE, FALSE),
    ('Renew Token', FALSE, TRUE, TRUE, FALSE, FALSE, FALSE, FALSE, FALSE),
    ('Session Token', FALSE, FALSE, FALSE, FALSE, FALSE, TRUE, FALSE, FALSE),
    ('Password Recovery Token', FALSE, FALSE, FALSE, TRUE, FALSE, FALSE, FALSE, FALSE),
    ('Password Change Token', FALSE, FALSE, FALSE, FALSE, TRUE, FALSE, FALSE, FALSE),
    -- Long Lived Token
    ('Identity Token', TRUE, FALSE, FALSE, FALSE, FALSE, FALSE, FALSE, FALSE),
    ('Bot Token', TRUE, FALSE, FALSE, FALSE, FALSE, TRUE, FALSE, TRUE),
    ('Third Party Token', TRUE, FALSE, FALSE, FALSE, FALSE, TRUE, TRUE, FALSE);



----------------------------------------------------------------------------------------------
-- Identity Store
CREATE TABLE character_data (
    character_id       BIGINT            NOT NULL        PRIMARY KEY,
    character_name     VARCHAR(20),
    service_id         INTEGER,
    account_id         BIGINT                            REFERENCES account_data(account_id),
    home_world         INTEGER           NOT NULL,
    retainer_list      BIGINT[],
    gil_hold           INTEGER,
    job_levels         SMALLINT[],
    -- Serialized data for feature use
    inventory         BYTEA
);
CREATE INDEX chara_world ON character_data(home_world);
CREATE INDEX chara_service ON character_data(service_id);
CREATE INDEX chara_account ON character_data(account_id);
CREATE INDEX chara_name ON character_data(character_name);

CREATE TABLE retainer_data (
    retainer_id        BIGINT            NOT NULL        PRIMARY KEY,
    retainer_name      VARCHAR(20)       NOT NULL,
    character          BIGINT            NOT NULL        REFERENCES character_data(character_id),
    world              INTEGER           NOT NULL,
    location           SMALLINT          NOT NULL,
    -- Serialized data for feature use
    inventory         BYTEA
);
CREATE INDEX retain_name ON retainer_data(retainer_name);
CREATE INDEX retain_chara ON retainer_data(character);

INSERT INTO character_data (character_id, character_name, service_id, account_id, home_world, retainer_list, job_levels, inventory) VALUES
    (0, '', 0, 0, 0, NULL, NULL, NULL);



----------------------------------------------------------------------------------------------
-- Data Store
CREATE TABLE listing_data (
    record_id        BIGSERIAL         NOT NULL,
    bucket_id        UUID              NOT NULL,
    report_time      TIMESTAMP         NOT NULL,
    world            INTEGER           NOT NULL,
    reporter_id      BIGINT            NOT NULL        REFERENCES character_data(character_id),
    -- Structure Data
    listing_id       BIGINT            NOT NULL,
    retainer_id      BIGINT            NOT NULL        REFERENCES retainer_data(retainer_id),
    owner_id         BIGINT            NOT NULL        REFERENCES character_data(character_id),
    artisan_id       BIGINT            NOT NULL        REFERENCES character_data(character_id),
    unit_price       INTEGER           NOT NULL,
    total_tax        INTEGER           NOT NULL,
    quantity         INTEGER           NOT NULL,
    item_id          INTEGER           NOT NULL,
    update_time      BIGINT            NOT NULL,
    container_id     SMALLINT          NOT NULL,
    slot_id          SMALLINT          NOT NULL,
    condition        SMALLINT          NOT NULL,
    spirit_bond      SMALLINT          NOT NULL,
    materia1         SMALLINT          NOT NULL,
    materia2         SMALLINT          NOT NULL,
    materia3         SMALLINT          NOT NULL,
    materia4         SMALLINT          NOT NULL,
    materia5         SMALLINT          NOT NULL,
    retainer_name    VARCHAR(40),
    player_name      VARCHAR(40),
    is_hq            BOOLEAN           NOT NULL,
    materia_count    SMALLINT          NOT NULL,
    on_mannequin     BOOLEAN           NOT NULL,
    retainer_loc     SMALLINT          NOT NULL,
    dye_id           SMALLINT          NOT NULL,
    PRIMARY KEY(record_id, report_time)
);
CREATE INDEX list_world ON listing_data(world);
CREATE INDEX list_price ON listing_data(unit_price);
CREATE INDEX list_quantity ON listing_data(quantity);
CREATE INDEX list_item ON listing_data(item_id);

CREATE TABLE history_data (
    record_id        BIGSERIAL         NOT NULL,
    bucket_id        UUID              NOT NULL,
    report_time      TIMESTAMP         NOT NULL,
    world            INTEGER           NOT NULL,
    reporter_id      BIGINT            NOT NULL        REFERENCES character_data(character_id),
    -- Structure Data
    item_id          INTEGER           NOT NULL,
    unit_price       INTEGER           NOT NULL,
    purchase_time    BIGINT            NOT NULL,
    quantity         INTEGER           NOT NULL,
    is_hq            BOOLEAN           NOT NULL,
    padding          BOOLEAN           NOT NULL,
    on_mannequin     BOOLEAN           NOT NULL,
    buyer_name       VARCHAR(40)       NOT NULL,
    PRIMARY KEY(record_id, report_time)
);
CREATE INDEX hist_world ON history_data(world);
CREATE INDEX hist_price ON history_data(unit_price);
CREATE INDEX hist_quantity ON history_data(quantity);
CREATE INDEX history_time ON history_data(purchase_time);

CREATE TABLE overview_data (
    record_id        BIGSERIAL         NOT NULL,
    bucket_id        UUID              NOT NULL,
    report_time      TIMESTAMP         NOT NULL,
    world            INTEGER           NOT NULL,
    reporter_id      BIGINT            NOT NULL        REFERENCES character_data(character_id),
    -- Structure Data
    item_id          INTEGER           NOT NULL,
    open_listing     SMALLINT          NOT NULL,
    demand           SMALLINT          NOT NULL,
    PRIMARY KEY(record_id, report_time)
);


----------------------------------------------------------------------------------------------
-- Supportive Data
CREATE TABLE version_download (
    bundle_key       VARCHAR(40)       NOT NULL        PRIMARY KEY,
    file_server      VARCHAR(64)       NOT NULL,
    files            VARCHAR(256)[]    NOT NULL,
    argument         VARCHAR(128)      NOT NULL,
    binary_update    BOOLEAN           NOT NULL,
    data_update      BOOLEAN           NOT NULL,
    updater_update   BOOLEAN           NOT NULL
);

CREATE TABLE version_data (
    version_id       SERIAL            NOT NULL        PRIMARY KEY,
    mil_version      INTEGER           NOT NULL,
    data_version     INTEGER           NOT NULL,
    game_version     INTEGER           NOT NULL,
    update_to        INTEGER,
    force_update     BOOLEAN           NOT NULL,
    bundle_key       VARCHAR(40)       NOT NULL        REFERENCES version_download(bundle_key),
    custom_message   VARCHAR(256)
);
CREATE INDEX version_mil ON version_data(mil_version);
CREATE INDEX version_dataver ON version_data(data_version);



----------------------------------------------------------------------------------------------
-- Audit Log
CREATE TABLE karma_log (
    record_id          BIGSERIAL       NOT NULL,
    report_time        TIMESTAMP       NOT NULL,
    account_id         BIGINT          NOT NULL        REFERENCES account_data(account_id),
    reason             INTEGER         NOT NULL,
    before             BIGINT          NOT NULL,
    after              BIGINT          NOT NULL,
    PRIMARY KEY(record_id, report_time)
);

CREATE TABLE account_log (
    record_id          BIGSERIAL       NOT NULL,
    report_time        TIMESTAMP       NOT NULL,
    account_id         BIGINT          NOT NULL        REFERENCES account_data(account_id),
    message            INTEGER         NOT NULL,
    detail             VARCHAR(256)    NOT NULL,
    ip_address         VARCHAR(40)     NOT NULL,
    PRIMARY KEY(record_id, report_time)
);

CREATE TABLE api_log (
    record_id          BIGSERIAL       NOT NULL,
    report_time        TIMESTAMP       NOT NULL,
    account_id         BIGINT          NOT NULL        REFERENCES account_data(account_id),
    key_id             BIGINT          NOT NULL        REFERENCES key_store(key_id),
    operation          INTEGER         NOT NULL,
    detail             VARCHAR(256)    NOT NULL,
    ip_address         VARCHAR(40)     NOT NULL,
    PRIMARY KEY(record_id, report_time)
);

CREATE TABLE data_log (
    -- Sensitive data change rollback record
    record_id          BIGSERIAL       NOT NULL,
    report_time        TIMESTAMP       NOT NULL,
    table_column       SMALLINT        NOT NULL,
    key                BIGINT          NOT NULL,
    from_value         VARCHAR(256),
    to_value           VARCHAR(256)    NOT NULL,
    operator           BIGINT          NOT NULL        REFERENCES account_data(account_id),
    PRIMARY KEY(record_id, report_time)
);

----------------------------------------------------------------------------------------------
-- Hypertable of TimescaleDB
SELECT create_hypertable('listing_data', 'report_time');
SELECT create_hypertable('history_data', 'report_time');
SELECT create_hypertable('overview_data', 'report_time');
SELECT create_hypertable('karma_log', 'report_time');
SELECT create_hypertable('account_log', 'report_time');
SELECT create_hypertable('api_log', 'report_time');
SELECT create_hypertable('data_log', 'report_time');

-- GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO milvaneth;
-- GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO milvaneth;