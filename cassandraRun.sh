docker run --name srds -e CASSANDRA_CLUSTER_NAME=brak_nazwy -d cassandra:latest
docker run --name srds-2 -e CASSANDRA_SEEDS=srds,srds-2 -e CASSANDRA_CLUSTER_NAME=brak_nazwy cassandra:latest
docker run --name srds-3 -e CASSANDRA_SEEDS=srds,srds-2 -e CASSANDRA_CLUSTER_NAME=brak_nazwy cassandra:latest
