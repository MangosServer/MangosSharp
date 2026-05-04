SELECT
    realmlist.clusterId            AS clusterId,
    realmlist.clusterAdminEndpoint AS clusterAdminEndpoint,
    realmlist.displayTag           AS displayTag,
    realmlist.markerPosition       AS markerPosition
FROM realmlist
WHERE realmlist.clusterId > 0
  AND realmlist.clusterAdminEndpoint <> ''
