docker run --name srds -e CASSANDRA_CLUSTER_NAME=brak_nazwy -p 9042:9042 -d cassandra:latest
INSTANCE1=$(docker inspect --format='{{ .NetworkSettings.IPAddress }}' srds)
echo "Instance 1: ${INSTANCE1}"

docker run --name srds-2 -p 9043:9042 -d -e CASSANDRA_CLUSTER_NAME=brak_nazwy -e CASSANDRA_SEEDS=$INSTANCE1 cassandra:latest
INSTANCE2=$(docker inspect --format='{{ .NetworkSettings.IPAddress }}' srds-2)
echo "Instance 2: ${INSTANCE2}"

docker run --name srds-3 -p 9044:9042 -d -e CASSANDRA_CLUSTER_NAME=brak_nazwy -e CASSANDRA_SEEDS=$INSTANCE1 cassandra:latest
INSTANCE3=$(docker inspect --format='{{ .NetworkSettings.IPAddress }}' srds-3)
echo "Instance 2: ${INSTANCE3}"
