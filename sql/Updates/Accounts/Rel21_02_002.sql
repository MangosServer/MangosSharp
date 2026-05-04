--
-- Federation support: peer-cluster admin endpoints + per-realm display
-- markers + per-account opt-in for showing those markers.
--
-- Applied as part of the world-cluster proxy refactor (PR #4).
-- Idempotent: safe to re-run after a partial apply.
--

-- Realmlist: cluster identity, federation listener address, and the short
-- tag that's prepended/appended to player names when chat or roster info
-- is replicated cross-realm.
ALTER TABLE `realmlist`
    ADD COLUMN IF NOT EXISTS `clusterId`             INT UNSIGNED NOT NULL DEFAULT 0
        COMMENT 'Federation cluster id; 0 disables federation for this realm',
    ADD COLUMN IF NOT EXISTS `clusterAdminEndpoint`  VARCHAR(128) NOT NULL DEFAULT ''
        COMMENT 'host:port of cluster federation listener; empty disables peer admin/chat',
    ADD COLUMN IF NOT EXISTS `displayTag`            VARCHAR(8)   NOT NULL DEFAULT ''
        COMMENT 'Short marker shown for cross-realm chat and player frames',
    ADD COLUMN IF NOT EXISTS `markerPosition`        ENUM('prefix','suffix','none') NOT NULL DEFAULT 'prefix'
        COMMENT 'Where to attach displayTag when rendering foreign-realm names';

-- Per-account toggle. When a player flips this off they stop seeing the
-- [tag] markers on cross-realm chat and player frames. Whispers always
-- carry the marker regardless so reply targeting still works.
ALTER TABLE `account`
    ADD COLUMN IF NOT EXISTS `federation_show_markers` TINYINT(1) NOT NULL DEFAULT 1
        COMMENT 'Show [tag] on cross-realm players/chat for this account';

-- Cluster-local mirror of federated groups. The leader's cluster owns
-- the authoritative copy; peers replicate enough to draw the party UI.
CREATE TABLE IF NOT EXISTS `federation_group` (
    `groupId`          BIGINT UNSIGNED NOT NULL,
    `leaderRealmId`    INT UNSIGNED    NOT NULL,
    `leaderGuid`       BIGINT UNSIGNED NOT NULL,
    `groupType`        TINYINT UNSIGNED NOT NULL COMMENT '0=party, 1=raid',
    `shardKey`         BIGINT UNSIGNED NOT NULL DEFAULT 0 COMMENT 'Reserved for Phase B co-location',
    `createdAt`        DATETIME        NOT NULL,
    `updatedAt`        DATETIME        NOT NULL,
    PRIMARY KEY (`groupId`, `leaderRealmId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `federation_group_member` (
    `groupId`          BIGINT UNSIGNED NOT NULL,
    `leaderRealmId`    INT UNSIGNED    NOT NULL,
    `memberRealmId`    INT UNSIGNED    NOT NULL,
    `memberGuid`       BIGINT UNSIGNED NOT NULL,
    `memberName`       VARCHAR(12)     NOT NULL,
    `role`             TINYINT UNSIGNED NOT NULL DEFAULT 0
        COMMENT 'Bitfield: 1=leader, 2=assist, 4=mainTank, 8=mainAssist',
    PRIMARY KEY (`groupId`, `leaderRealmId`, `memberRealmId`, `memberGuid`),
    KEY `idx_member` (`memberRealmId`, `memberGuid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Bump db_version so DbVersionChecker accepts the new schema.
INSERT INTO `db_version`(`version`,`structure`,`content`,`description`,`comment`)
VALUES (21,2,2,'Federation_columns','PR #4 world-cluster proxy: peer admin endpoints + cross-realm markers')
ON DUPLICATE KEY UPDATE `description`=VALUES(`description`), `comment`=VALUES(`comment`);
