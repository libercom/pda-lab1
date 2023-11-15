package main

type CircuitBreaker struct {
	Services map[string]bool
}

func NewCircuitBreaker() *CircuitBreaker {
	return &CircuitBreaker{
		Services: map[string]bool{},
	}
}

func (cb *CircuitBreaker) CheckService(url string) {
	cb.Services[url] = false
	// todo
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