package main

import (
	"context"
	"os"

	"github.com/go-redis/redis/v8"
)

type Cache struct {
	client *redis.Client
}

func NewCache() *Cache {
	client := redis.NewClient(&redis.Options{
        Addr:     os.Getenv("REDIS_CACHE"),
        Password: "", 
        DB:       0,
    })

	_, err := client.Ping(context.Background()).Result()
	
    if err != nil {
        panic(err)
    }

    return &Cache{
		client: client,
	}
}