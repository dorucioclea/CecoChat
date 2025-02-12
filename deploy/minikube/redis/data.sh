# partitioning
redis-cli SET   partitioning.count 12
redis-cli HMSET partitioning.partitions 0 "0-5" 1 "6-11"
redis-cli HMSET partitioning.addresses 0 "https://messaging.cecochat.com/m0" 1 "https://messaging.cecochat.com/m1"

# history
redis-cli SET history.message-count 32

# snowflake
redis-cli HSET snowflake.generator-ids 0 "0,1" 1 "2,3"
