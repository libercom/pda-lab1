run:
	docker-compose run --remove-orphans
build:
	docker-compose run --build --remove-orphans

.PHONY:
	run build
