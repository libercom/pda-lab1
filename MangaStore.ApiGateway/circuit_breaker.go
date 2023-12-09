package main

import (
	"context"
	"log"
	"net/http"
	"time"
)

type CircuitBreaker struct {
	Services map[string]bool
}

func NewCircuitBreaker() *CircuitBreaker {
	return &CircuitBreaker{
		Services: map[string]bool{},
	}
}

func (cb *CircuitBreaker) CheckService(url string, timeout time.Duration) {
	cb.Services[url] = false
	// todo
	go cb.checkServiceImpl(url, timeout)
}

func (cb *CircuitBreaker) IsHealthy(url string) bool {
	isHealthy, exists := cb.Services[url]

	if exists {
		return isHealthy
	} else {
		cb.Services[url] = true

		return true
	}
}

func (cb *CircuitBreaker) checkServiceImpl(url string, timeout time.Duration) {
	stop := time.Now().Add(time.Duration(float64(timeout) * 3.5) * time.Second)
	errors := 0

	log.Println("GOT HERE: " + url + "/status")

	for time.Now().Before(stop) {
		ctx, cancel := context.WithTimeout(context.Background(), timeout * time.Second)
		defer cancel()

		client := &http.Client{}
		req, err := http.NewRequest("GET", url + "/status", nil)

		if err != nil {
			log.Println("Error creating request:", err)
			return
		}
		req = req.WithContext(ctx)

		_, err = client.Do(req)
		
		if err != nil {
			errors += 1
		}

		if errors == 3 {
			log.Println("The service at " + url + " is unhealty")
			cb.Services[url] = false
			return
		}
	}
}