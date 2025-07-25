CREATE TABLE `calendar_streams` (
    `id`                        CHAR(36)        NOT NULL    PRIMARY KEY                 COMMENT 'stream uuid',
    `user_id`                   CHAR(36)        NOT NULL    UNIQUE                      COMMENT 'stream author user uuid',
    `token`                     VARCHAR(32)     NOT NULL,
    `created_at`                DATETIME        NOT NULL    DEFAULT CURRENT_TIMESTAMP,
    `updated_at`                DATETIME        NOT NULL    DEFAULT CURRENT_TIMESTAMP   ON UPDATE CURRENT_TIMESTAMP
)   DEFAULT CHARSET=utf8mb4;
