docker stop srds srds-2 srds-3
docker rm srds srds-2 srds-3

docker network rm cassandra

docker volume rm srds srds-2 srds-3