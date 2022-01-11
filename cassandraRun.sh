docker network create cassandra

docker volume create srds 
docker volume create srds-2 
docker volume create srds-3

docker volume ls

docker run --name srds --network cassandra -p 127.0.0.1:9042:9042 -v srds:/var/lib/cassandra -e CASSANDRA_SEEDS=srds,srds-2 -e CASSANDRA_CLUSTER_NAME=brak_nazwy -d cassandra:latest


docker run --name srds-2 --network cassandra -p 127.0.0.1:9043:9042 -d -v srds-2:/var/lib/cassandra -e CASSANDRA_CLUSTER_NAME=brak_nazwy -e CASSANDRA_SEEDS=srds,srds-2 cassandra:latest


docker run --name srds-3 --network cassandra -p 127.0.0.1:9044:9042 -d -v srds-3:/var/lib/cassandra -e CASSANDRA_CLUSTER_NAME=brak_nazwy -e CASSANDRA_SEEDS=srds,srds-2 cassandra:latest

