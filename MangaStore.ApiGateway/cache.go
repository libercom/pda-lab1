package main

import (
	"context"

	"github.com/go-redis/redis/v8"
)

type Cache struct {
	client *redis.Client
}

func NewCache() *Cache {
	client := redis.NewClient(&redis.Options{
        Addr:     "localhost:6379",
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